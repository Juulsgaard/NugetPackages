using System.Linq.Expressions;
using AutoMapper;
using Juulsgaard.Crud.Domain.Interfaces;
using Juulsgaard.Crud.Extensions;
using Juulsgaard.Crud.Models;
using Juulsgaard.Crud.Transactions;
using Juulsgaard.Tools.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Juulsgaard.Crud.Builders;

/// <summary>
/// This is the config for deleting an Entity
/// </summary>
/// <typeparam name="TModel">The tracked Model</typeparam>
public class CrudDeleteConfig<TModel> where TModel : class
{
	protected internal CrudTargetConfig<TModel> Target { get; }
	protected internal bool UpdateIndex { get; set; }
	protected internal Func<TModel, Expression<Func<TModel, bool>>>? SubSetIdentifier { get; set; }
	protected internal bool WillSave { get; protected set; }
		
	protected internal DbContext Context => Target.Context;
	protected internal IMapper? Mapper => Target.Mapper;
	protected CrudExceptionLookup? ExceptionLookup;
		
		
	/// <summary>
	/// Manually instantiate a <see cref="CrudDeleteConfig{TModel}"/>
	/// </summary>
	/// <param name="target">The target to delete</param>
	/// <remarks>It's preferred to instantiate a <see cref="CrudDeleteConfig{TModel}"/> using the fluid CRUD syntax</remarks>
	public CrudDeleteConfig(CrudTargetConfig<TModel> target)
	{
		Target = target;
	}

	/// <summary>
	/// Enable saving of the deletion
	/// </summary>
	/// <returns>The current Config</returns>
	/// <remarks>Without applying this config you will have to manually save after executing</remarks>
	public CrudDeleteConfig<TModel> Save()
	{
		WillSave = true;
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
	public CrudDeleteConfig<TModel> WithExceptions(
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

	protected async ValueTask Remove(TModel model)
	{
		Target.Set.Remove(model);

		if (UpdateIndex) {
			var query = Target.Query;

			if (SubSetIdentifier != null) {
				query = query.Where(SubSetIdentifier(model));
			}

			if (query is IQueryable<ISorted> tempQuery && model is ISorted indexModel) {
				await tempQuery.Where(x => x.Index > indexModel.Index).ForEachAsync(m => m.Index--);
			}
		}
	}

	/// <summary>
	/// Executes the deletion configuration
	/// </summary>
	/// <returns>The deleted model</returns>
	public async Task<TModel> ExecuteAsync()
	{
		var model = await Target.FirstAsync();
		await Remove(model);
			
		await SaveAndHandleErrors();

		return model;
	}

	private async ValueTask SaveAndHandleErrors()
	{
		if (!WillSave) return;
		
		try {
			await Context.SaveChangesAsync();
		}
		catch (DbUpdateException e) {
			throw e.ProcessAsDelete(Target.EntityName, ExceptionLookup);
		}
	}

	/// <summary>
	/// Executes the deletion configuration for several items
	/// </summary>
	/// <remarks>This does not support index updates (yet)</remarks>
	/// <returns>The deleted model</returns>
	public async Task<List<TModel>> ExecuteForRangeAsync()
	{
		if (UpdateIndex && !WillSave) {
			throw new InternalException("You cannot mass delete sorted models without saving");
		}
		
		var models = await Target.ToListAsync();

		await using var trx = await Context.BeginInnerTransactionAsync();

		foreach (var model in models) {
			await Remove(model);
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

	/// <summary>
	/// Executes the deletion configuration and maps the deleted model to <typeparamref name="TMap"/>
	/// </summary>
	/// <typeparam name="TMap">The type to map the deleted model to</typeparam>
	/// <returns>The deleted and mapped model</returns>
	public async Task<TMap> ExecuteAndMapAsync<TMap>()
	{
		if (Mapper == null) {
			throw new InternalException("DbContext does not expose IMapper");
		}
			
		var model = await ExecuteAsync();
		return Mapper!.Map<TMap>(model);
	}
}