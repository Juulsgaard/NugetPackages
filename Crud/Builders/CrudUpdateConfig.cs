using System.Linq.Expressions;
using AutoMapper;
using Crud.Domain.Interfaces;
using Crud.Exceptions;
using Crud.Models;
using Crud.Monitoring;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using Tools.Exceptions;
using Tools.Extensions;

namespace Crud.Builders;

/// <summary>
/// This is the config for updating an Entity
/// </summary>
/// <typeparam name="TModel">The tracked model</typeparam>
/// <typeparam name="TUpdate">The type of the <see cref="CrudUpdateConfig{TModel,TUpdate}.UpdateModel"/> used to update the Target</typeparam>
public class CrudUpdateConfig<TModel, TUpdate> where TModel : class
{
	protected Expression<Func<ISorted, int>> GetSortableKey = indexed => indexed.Index;
		
	protected internal CrudTargetConfig<TModel> Target { get; }
		
	protected internal TUpdate? UpdateModel { get; }
	protected internal Action<TModel>? ModifyFunc { get; protected set; }
		
	protected internal List<IUpdateMonitor<TModel>> Monitors { get; } = new();
	protected internal List<IPropertyUpdateMonitor<TModel>> ParentMonitors { get; } = new();
		
	protected internal bool WillSave { get; protected set; }
		
	protected internal DbContext Context => Target.Context;
	protected internal IMapper? Mapper => Target.Mapper;
	protected CrudExceptionLookup? ExceptionLookup;
	

	/// <summary>
	/// Manually instantiate a <see cref="CrudUpdateConfig{TModel,TUpdate}"/>
	/// </summary>
	/// <param name="target">The target to update</param>
	/// <param name="updateModel">The model to apply when updating</param>
	/// <remarks>It's preferred to instantiate a <see cref="CrudUpdateConfig{TModel,TUpdate}"/> using the fluid CRUD syntax</remarks>
	public CrudUpdateConfig(CrudTargetConfig<TModel> target, TUpdate updateModel)
	{
		Target = target;
		UpdateModel = updateModel;
	}
		
	/// <summary>
	/// Manually instantiate a <see cref="CrudUpdateConfig{TModel,TUpdate}"/>
	/// </summary>
	/// <param name="target">The target to update</param>
	/// <param name="modify">A method to update the model</param>
	/// <remarks>It's preferred to instantiate a <see cref="CrudUpdateConfig{TModel,TUpdate}"/> using the fluid CRUD syntax</remarks>
	public CrudUpdateConfig(CrudTargetConfig<TModel> target, Action<TModel> modify)
	{
		Target = target;
		UpdateModel = default;
		Modify(modify);
	}
		
	/// <summary>
	/// Manually instantiate a <see cref="CrudUpdateConfig{TModel,TUpdate}"/>
	/// </summary>
	/// <param name="oldUpdateConfig">The <see cref="CrudUpdateConfig{TModel,TUpdate}"/> to copy</param>
	/// <remarks>It's preferred to instantiate a <see cref="CrudUpdateConfig{TModel,TUpdate}"/> using the fluid CRUD syntax</remarks>
	public CrudUpdateConfig(CrudUpdateConfig<TModel, TUpdate> oldUpdateConfig)
	{
		Monitors = oldUpdateConfig.Monitors;
		ParentMonitors = oldUpdateConfig.ParentMonitors;
		UpdateModel = oldUpdateConfig.UpdateModel;
		WillSave = oldUpdateConfig.WillSave;
		Target = oldUpdateConfig.Target;
		ModifyFunc = oldUpdateConfig.ModifyFunc;
		ExceptionLookup = oldUpdateConfig.ExceptionLookup;
	}

	#region Modify Config

	/// <summary>
	/// Enable saving of the updated model
	/// </summary>
	/// <returns>The current Config</returns>
	/// <remarks>Without applying this config you will have to manually save after executing</remarks>
	public CrudUpdateConfig<TModel, TUpdate> Save()
	{
		WillSave = true;
		return this;
	}
		
	/// <summary>
	/// Add a modification method that will be applied after the <see cref="UpdateModel"/> is mapped to a <typeparamref name="TModel"/> and applied to the target,
	/// but before the changes are saved
	/// </summary>
	/// <param name="func">The modification to be applied to the mapped <typeparamref name="TModel"/></param>
	/// <returns>The current Config</returns>
	/// <example>
	///     Neuter a dog, and decrease it's happiness
	///     <code>
	///         Context.Dogs
	///             .Filter(x => x.Name == "Fido")
	///             .Update(new DogUpdateModel {Neutered = true})
	///             .Modify(dog => dog.Happiness -= 10)
	///             .ExecuteAsync()
	///     </code>
	/// </example>
	public CrudUpdateConfig<TModel, TUpdate> Modify(Action<TModel> func)
	{
		ModifyFunc = func;
		return this;
	}

	/// <summary>
	/// Monitor changes to a prop during the update execution
	/// </summary>
	/// <param name="selector">An expression returning the prop to monitor</param>
	/// <param name="monitor">The outputted <see cref="PropertyUpdateMonitor{TModel,TProp}"/> that will contain the update state of the prop after execution</param>
	/// <typeparam name="TProp">The type of the prop to monitor</typeparam>
	/// <returns>The current Config</returns>
	/// <example>
	///     <code>
	///         await Context.Dogs
	///             .Filter(x => x.Name == "Fido")
	///             .Update(new DogUpdateModel {InHeat = true})
	///             .Monitor(x => x.InHeat, out var inHeatChanged)
	///             .ExecuteAsync();
	///
	/// 
	///         if (inHeatChanged.Changed) {
	///             Logger.Info("Fido has entered heat!");
	///         } else {
	///             Logger.Warning("Fido is still horny");
	///         }
	///     </code>
	/// </example>
	public CrudUpdateConfig<TModel, TUpdate> MonitorProp<TProp>(
		Expression<Func<TModel, TProp>> selector,
		out PropertyUpdateMonitor<TModel, TProp> monitor
	)
	{
		monitor = new PropertyUpdateMonitor<TModel, TProp>(selector, Target.Context);
		Monitors.Add(monitor);

		return this;
	}
	
	/// <summary>
	/// Monitor changes to a list during the update execution
	/// </summary>
	/// <param name="selector">An expression returning the list to monitor</param>
	/// <param name="monitor">The outputted <see cref="ListUpdateMonitor{TModel,TProp}"/> that will contain the update state of the list after execution</param>
	/// <typeparam name="TProp">The item type of the list to monitor</typeparam>
	/// <returns>The current Config</returns>
	public CrudUpdateConfig<TModel, TUpdate> MonitorList<TProp>(
		Expression<Func<TModel, IEnumerable<TProp>>> selector,
		out ListUpdateMonitor<TModel, TProp> monitor
	)
	{
		monitor = new ListUpdateMonitor<TModel, TProp>(selector, Target.Context);
		Monitors.Add(monitor);

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
	public CrudUpdateConfig<TModel, TUpdate> WithExceptions(
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

	#endregion

	#region Upsert

	/// <summary>
	/// Create the item if none is found, using a <paramref name="createModel"/>
	/// </summary>
	/// <param name="createModel">A separate model for creation</param>
	/// <typeparam name="TCreate">The type of the <paramref name="createModel"/></typeparam>
	/// <returns>A <see cref="CrudUpsertConfig{TModel,TUpdate,TCreate}"/> to configure and execute the upsert</returns>
	public CrudUpsertConfig<TModel, TUpdate, TCreate> Create<TCreate>(TCreate createModel)
	{
		return new CrudUpsertConfig<TModel, TUpdate, TCreate>(this, createModel);
	}

	/// <summary>
	/// Create the item if none is found, using the <see cref="CrudUpdateConfig{TModel,TUpdate}.UpdateModel"/>
	/// </summary>
	/// <returns>A <see cref="CrudUpsertConfig{TModel,TUpdate,TCreate}"/> to configure and execute the upsert</returns>
	public CrudUpsertConfig<TModel, TUpdate, TUpdate> Create()
	{
		if (UpdateModel == null) {
			throw new InternalException("Implicit CreateModel for Upsert requires a defined UpdateModel");
		}
		return new CrudUpsertConfig<TModel, TUpdate, TUpdate>(this, UpdateModel);
	}

	#endregion

	/// <summary>
	/// Execute the update on a target model
	/// </summary>
	/// <param name="model">The model to update</param>
	/// <param name="forRange">Marks if the method should use range specific logic</param>
	/// <returns>An updated model</returns>
	protected TModel Update(TModel model, bool forRange = false)
	{
		if (!forRange) {
			Monitors.ForEach(m => m.UpdateOld(model));
		}
		ParentMonitors.ForEach(m => m.UpdateOld(model));

		Mapper!.Map(UpdateModel, model);

		ModifyFunc?.Invoke(model);

		if (!forRange) {
			Monitors.ForEach(m => m.UpdateNew(model));
		}

		ParentMonitors.ForEach(m => m.UpdateNew(model));

		return model;
	}

	/// <summary>
	/// An AfterUpdate method to handle items (With indices) moved between parents
	/// </summary>
	/// <param name="model">The updated model</param>
	/// <returns>An awaitable <see cref="Task"/></returns>
	protected async ValueTask DetectParentChanges(TModel model)
	{
		if (ParentMonitors.Count > 0 && ParentMonitors.Any(m => m.Changed) && model is ISorted indexed) {

			var greaterIndex = Expression.Lambda<Func<ISorted, bool>>(
				Expression.GreaterThan(
					GetSortableKey.Body,
					Expression.Constant(indexed.Index)
				),
				GetSortableKey.Parameters[0]
			);

			IQueryable<TModel> q = Target.Set;
				
			foreach (var monitor in ParentMonitors) {
				q = q.Where(monitor.HasOldValueExpression());
			}
				
			if (q is IQueryable<ISorted> query) {
				await query.Where(greaterIndex).ForEachAsync(x => x.Index--);
			}

			IQueryable<TModel> indexQ = Target.Set;
				
			foreach (var monitor in ParentMonitors) {
				indexQ = indexQ.Where(monitor.HasNewValueExpression());
			}
				
			if (indexQ is IQueryable<ISorted> indexQuery) {
				indexed.Index = await indexQuery.OrderByDescending(m => m.Index)
				   .Select(m => m.Index + 1)
				   .FirstOrDefaultAsync();
			}
		}
	}

	/// <summary>
	/// Executes the update configuration
	/// </summary>
	/// <returns>The updated model</returns>
	public async Task<TModel> ExecuteAsync()
	{
		if (Mapper == null) {
			throw new InternalException("DbContext does not expose IMapper");
		}
			
		var model = await Target.FirstAsync();

		var result = Update(model);

		await DetectParentChanges(result);

		await SaveAndHandleErrors();
		
		return result;
	}
		
	/// <summary>
	/// Executes the update configuration on multiple entities
	/// </summary>
	/// <returns>The updated models</returns>
	public async Task<List<TModel>> ExecuteForRangeAsync()
	{
		if (Mapper == null) {
			throw new InternalException("DbContext does not expose IMapper");
		}
			
		var models = await Target.ToListAsync();

		var results = new List<TModel>();
			
		foreach (var model in models) {
			var result = Update(model, true);
			await DetectParentChanges(result);
			results.Add(result);
		}

		await SaveAndHandleErrors();
			
		return results;
	}
	
	private async ValueTask SaveAndHandleErrors()
	{
		try {
			if (WillSave) {
				await Context.SaveChangesAsync();
			}
		}
		catch (DbUpdateConcurrencyException e) {
			Log.Error(e, "Concurrency error while updating {EntityName}", Target.EntityName);
			throw new DatabaseConflictException(
				ExceptionLookup?.Concurrency ?? $"Someone else has edited this {Target.EntityName}",
				e.InnerException
			);
		}
		catch (UniqueConstraintException e) {
			Log.Error(e, "Update of {EntityName} violates Unique Constraint", Target.EntityName);
			if (e.InnerException is PostgresException pe) {
				var prop = pe.Data.ReadValueOrDefault("ConstraintName")?.ToString()?.Split('_')[^1];
			}
			throw new DatabaseConflictException(
				ExceptionLookup?.UniqueConflict ?? $"This version of {Target.EntityName} already exists",
				e.InnerException
			);
		}
		catch (CannotInsertNullException e) {
			var columnName = (string?)e.InnerException?.Data.ReadValueOrDefault("ColumnName");
			Log.Error(
				e,
				"Tried to insert null in non-nullable field ({ColumnName}) while updating {EntityName}",
				columnName ?? "N/A",
				Target.EntityName
			);
			throw new DatabaseException(
				ExceptionLookup?.NullInsert
			 ?? $"Cannot update {Target.EntityName} with null value{(columnName != null ? $" - {columnName}" : "")}",
				e.InnerException
			);
		}
		catch (DbUpdateException e) {
			Log.Error(
				e,
				"Database error while updating {EntityName}: {InnerMessage}",
				Target.EntityName,
				e.InnerException?.Message ?? "No Details"
			);
			throw new DatabaseException(ExceptionLookup?.Default ?? $"Failed to update {Target.EntityName}", e.InnerException);
		}
	}

	/// <summary>
	/// Executes the update configuration and maps to <typeparamref name="TMap"/>
	/// </summary>
	/// <typeparam name="TMap">The type to map the Model to</typeparam>
	/// <returns>The updated and mapped model</returns>
	public async Task<TMap> ExecuteAndMapAsync<TMap>()
	{
		var model = await ExecuteAsync();
		return Mapper!.Map<TMap>(model);
	}
		
	/// <summary>
	/// Executes the update configuration for all multiple entities and maps them to a <typeparamref name="TMap"/> list
	/// </summary>
	/// <typeparam name="TMap">The type to map the Model to</typeparam>
	/// <returns>The updated and mapped model</returns>
	public async Task<List<TMap>> ExecuteForRangeAndMapAsync<TMap>()
	{
		var models = await ExecuteForRangeAsync();
		return Mapper!.Map<List<TMap>>(models);
	}
}