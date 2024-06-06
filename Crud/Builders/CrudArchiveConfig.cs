using System.Linq.Expressions;
using AutoMapper;
using Juulsgaard.Crud.Domain.Interfaces;
using Juulsgaard.Crud.Extensions;
using Juulsgaard.Crud.Models;
using Juulsgaard.Crud.Transactions;
using Juulsgaard.Tools.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Juulsgaard.Crud.Builders;

public class CrudArchiveConfig<TModel> where TModel : class, IArchivable
{
	protected internal CrudTargetConfig<TModel> Target { get; }
	protected internal bool UpdateIndex { get; set; }
	protected internal Func<TModel, Expression<Func<TModel, bool>>>? SubSetIdentifier { get; set; }
	protected internal Action<TModel>? ModifyFunc { get; protected set; }
	protected internal bool WillSave { get; protected set; }
	protected internal bool RestoreMode { get; }
		
	protected internal DbContext Context => Target.Context;
	protected internal IMapper? Mapper => Target.Mapper;
	private string Mode => RestoreMode ? "restore" : "archive";
	protected CrudExceptionLookup? ExceptionLookup;


	/// <summary>
	/// Manually instantiate a <see cref="CrudArchiveConfig{TModel}"/>
	/// </summary>
	/// <param name="target">The target to archive</param>
	/// <param name="restore">Set the mode to restoration</param>
	/// <remarks>It's preferred to instantiate a <see cref="CrudArchiveConfig{TModel}"/> using the fluid CRUD syntax</remarks>
	public CrudArchiveConfig(CrudTargetConfig<TModel> target, bool restore = false)
	{
		Target = target;
		RestoreMode = restore;
	}

	/// <summary>
	/// Enable saving of the archiving
	/// </summary>
	/// <returns>The current Config</returns>
	/// <remarks>Without applying this config you will have to manually save after executing</remarks>
	public CrudArchiveConfig<TModel> Save()
	{
		WillSave = true;
		return this;
	}
	
	/// <summary>
	/// Add a modification method that will be applied after the Entity is archived,
	/// but before the changes are saved
	/// </summary>
	/// <param name="func">The modification to be applied to the archived <typeparamref name="TModel"/></param>
	/// <returns>The current Config</returns>
	public CrudArchiveConfig<TModel> Modify(Action<TModel> func)
	{
		ModifyFunc = func;
		return this;
	}

	/// <summary>
	/// Define Exception text overrides for different Exception types
	/// </summary>
	/// <param name="default">Default exception text - Used for all undefined exceptions</param>
	/// <param name="uniqueConflict">Exception text for broken unique constraints</param>
	/// <param name="nullInsert">Exception text for null inserts in non-nullable fields</param>
	/// <param name="concurrency">Exception text for concurrency errors</param>
	/// <param name="notFound">Exception text for not found errors</param>
	/// <returns>The current Config</returns>
	public CrudArchiveConfig<TModel> WithExceptions(
		string? @default = null,
		string? uniqueConflict = null,
		string? nullInsert = null,
		string? concurrency = null,
		string? notFound = null
	)
	{
		ExceptionLookup = new() {
			UniqueConflict = uniqueConflict ?? @default,
			NullInsert = nullInsert ?? @default,
			Concurrency = concurrency ?? @default,
			Default = @default
		};
		
		notFound ??= @default;
		if (notFound != null) Target.WithException(notFound);
		return this;
	}

	protected async ValueTask Archive(TModel model)
	{
		if (model.ArchivedAt != null) return;

		model.ArchivedAt = DateTime.UtcNow;
		
		ModifyFunc?.Invoke(model);

		if (UpdateIndex) await ArchiveUpdateIndices(model);
	}

	private async Task ArchiveUpdateIndices(TModel model)
	{
		var query = Target.Query;

		if (SubSetIdentifier != null) {
			query = query.Where(SubSetIdentifier(model));
		}

		if (query is not IQueryable<ISorted> tempQuery || model is not ISorted indexModel) return;
		if (indexModel.Index < 0) return;
		
		var oldIndex = indexModel.Index;
		indexModel.Index = -1;	

		if (!WillSave || Target.Testing)
		{
			await tempQuery.Where(x => x.Index > indexModel.Index).ForEachAsync(m => m.Index--);
		}
		else
		{
			await using var trx = await Target.Context.BeginInnerTransactionAsync();
			
			await SaveAndHandleErrors();
			
			await tempQuery.Where(x => x.Index > oldIndex)
				.ExecuteUpdateAsync(c => c.SetProperty(x => x.Index, x => x.Index - 1));

			await trx.CommitAsync();
		}
		
	}
	
	protected async ValueTask Restore(TModel model)
	{
		if (model.ArchivedAt == null) return;

		model.ArchivedAt = null;
		
		ModifyFunc?.Invoke(model);

		if (UpdateIndex) await RestoreUpdateIndices(model);
	}
	
	private async Task RestoreUpdateIndices(TModel model)
	{
		var query = Target.Query;

		if (SubSetIdentifier != null) {
			query = query.Where(SubSetIdentifier(model));
		}

		if (query is not IQueryable<ISorted> tempQuery || model is not ISorted indexModel) return;
		
		if (indexModel.Index >= 0) return;
		
		var index = await tempQuery
			.OrderByDescending(f => f.Index)
			.Select(f => f.Index + 1)
			.FirstOrDefaultAsync();
		
		indexModel.Index = index;
	}

	/// <summary>
	/// Executes the archival configuration
	/// </summary>
	/// <returns>The archived model</returns>
	public async Task<TModel> ExecuteAsync()
	{
		var model = await Target.FirstAsync();

		if (RestoreMode) {
			await Restore(model);
		} else {
			await Archive(model);	
		}

		await SaveAndHandleErrors();

		return model;
	}
		
	/// <summary>
	/// Executes the archival configuration for several items
	/// </summary>
	/// <remarks>This does not support index updates (yet)</remarks>
	/// <returns>The deleted model</returns>
	public async Task<List<TModel>> ExecuteForRangeAsync()
	{
		if (UpdateIndex && !WillSave) {
			throw new InternalException($"You cannot mass {Mode} sorted models without saving");
		}
		
		var models = await Target.ToListAsync();

		await using var trx = await Context.BeginInnerTransactionAsync();

		foreach (var model in models) {
			
			if (RestoreMode) {
				await Restore(model);
			} else {
				await Archive(model);
			}
			
			if (UpdateIndex) {
				await SaveAndHandleErrors();
			}
		}

		if (!UpdateIndex) {
			await SaveAndHandleErrors();
		}

		await trx.CommitAsync();

		return models;
	}

	private async ValueTask SaveAndHandleErrors()
	{
		if (!WillSave) return;
		
		try {
			await Context.SaveChangesAsync();
		}
		catch (DbUpdateException e) {
			throw e.ProcessAsArchive(Target.EntityName, ExceptionLookup);
		}
	}

	/// <summary>
	/// Executes the archival configuration and maps the archived model to <typeparamref name="TMap"/>
	/// </summary>
	/// <typeparam name="TMap">The type to map the archived model to</typeparam>
	/// <returns>The archived and mapped model</returns>
	public async Task<TMap> ExecuteAndMapAsync<TMap>()
	{
		if (Mapper == null) {
			throw new InternalException("DbContext does not expose IMapper");
		}
			
		var model = await ExecuteAsync();
		return Mapper!.Map<TMap>(model);
	}
}