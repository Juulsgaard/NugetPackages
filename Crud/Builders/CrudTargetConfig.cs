using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crud.Exceptions;
using Crud.Extensions;
using Crud.Helpers;
using Crud.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Tools.Exceptions;

namespace Crud.Builders;

/// <summary>
/// A configuration for targeting entities
/// </summary>
/// <typeparam name="TModel">The tracked model</typeparam>
/// <remarks>This structure has some added capability due to the added <see cref="Set"/> and <see cref="Context"/></remarks>
public class CrudTargetConfig<TModel> where TModel : class
{
	protected internal Expression<Func<TModel, bool>>? Filter { get; }
	protected internal IQueryable<TModel> Query { get; set; }
	protected internal List<Func<EntityEntry<TModel>, Task>> LoadActions { get; set; } = new();
	protected internal bool WillProjectInMemory { get; protected set; }
	protected internal int SkipAmount { get; protected set; }
	protected internal int TakeAmount { get; protected set; }
	protected internal DbSet<TModel> Set { get; }
	protected internal DbContext Context { get; }

	protected internal string EntityName => EntityNameHelper.GetEntityName(Set.GetType().GenericTypeArguments[0].Name);
	internal readonly IMapper? Mapper;
	internal readonly bool Testing;
	protected string? CustomException;
		


	/// <summary>
	/// A manual constructor to instantiate a <see cref="CrudTargetConfig{TModel}"/>
	/// </summary>
	/// <param name="set">The set to base the Target off of</param>
	/// <param name="filter">An optional filter to specify the target</param>
	/// <remarks>It's preferred to instantiate a CrudContextTarget using extension methods</remarks>
	public CrudTargetConfig(DbSet<TModel> set, Expression<Func<TModel, bool>>? filter = null)
	{
		Filter = filter;
		Set = set;
		Query = set.AsQueryable();
		Context = set.GetService<ICurrentDbContext>().Context;
		Mapper = Context is ICrudDbContext db ? db.Mapper : null;
		Testing = Context is ITestDbContext;
	}

	/// <summary>
	/// Ensure that any potential projection happens in memory
	/// </summary>
	/// <returns>The current Target</returns>
	/// <remarks>Safe mode changes execution so that all mapping is handles in memory</remarks>
	/// <example>Use this in cases where you utilise a feature that conflicts with Automapper's <c>.ProjectTo()</c> method</example>
	public CrudTargetConfig<TModel> ProjectInMemory()
	{
		WillProjectInMemory = true;
		return this;
	}

	#region Modify Query

	/// <summary>
	/// Adds a filter to your base <see cref="IQueryable{TModel}"/>
	/// </summary>
	/// <param name="filter">A method to filter out unwanted results</param>
	/// <returns>The current Target</returns>
	public CrudTargetConfig<TModel> Where(Expression<Func<TModel, bool>> filter)
	{
		Query = Query.Where(filter);
		return this;
	}
		
	/// <summary>
	/// Adds a filter to your base <see cref="IQueryable{TModel}"/>
	/// </summary>
	/// <param name="filter">A method to filter out unwanted results</param>
	/// <param name="apply">An optional boolean to determine if the filter should apply or not</param>
	/// <returns>The current Target</returns>
	/// <example>
	/// 	Get all dogs if admin, but only your own if not:
	/// 	<code>
	/// 		Context.Dogs
	/// 			.Target(x => x.Breed == "Terrier")
	/// 			.Where(x => x.Mine, !isAdmin)
	/// 			.ToListAsync()
	/// 	</code>
	/// </example>
	public CrudTargetConfig<TModel> ConditionalWhere(Expression<Func<TModel, bool>> filter, bool apply = true)
	{
		if (apply) {
			Query = Query.Where(filter);
		}

		return this;
	}

	/// <summary>
	/// Skips over <paramref name="count"/> amount of elements in the resulting list
	/// </summary>
	/// <param name="count">THe amount of elements to skip</param>
	/// <returns>The current Target</returns>
	/// <remarks>This is only applicable when using <see cref="ToListAsync"/></remarks>
	public CrudTargetConfig<TModel> Skip(int count)
	{
		SkipAmount = count;
		return this;
	}

	/// <summary>
	/// Limit the amount of entries to return in the list
	/// </summary>
	/// <param name="count">The amount of entries to return</param>
	/// <returns>The current Target</returns>
	/// <remarks>This is only applicable when using <see cref="ToListAsync"/></remarks>
	public CrudTargetConfig<TModel> Take(int count)
	{
		TakeAmount = count;
		return this;
	}

	/// <summary>
	/// Include a navigational prop in the final result
	/// </summary>
	/// <param name="navigation">An expression returning the navigational prop you wish to include</param>
	/// <typeparam name="TProp">The type of the navigational prop</typeparam>
	/// <returns>The current Target</returns>
	public CrudTargetConfig<TModel> Include<TProp>(Expression<Func<TModel, TProp>> navigation)
	{
		Query = Query.Include(navigation);
		return this;
	}
	
	/// <summary>
	/// Perform additional loading after the initial query.
	/// This only works with single entity queries that aren't mapped / projected
	/// </summary>
	/// <param name="loadAction">The action to perform after load</param>
	/// <returns></returns>
	public CrudTargetConfig<TModel> SmartInclude(Func<EntityEntry<TModel>, Task> loadAction)
	{
		LoadActions.Add(loadAction);
		return this;
	}

	/// <summary>
	/// Conditionally include a navigational prop in the final result
	/// </summary>
	/// <param name="navigation">An expression returning the navigational prop you wish to include</param>
	/// <param name="include">Whether or not to include the property</param>
	/// <typeparam name="TProp">The type of the navigational prop</typeparam>
	/// <returns>The current Target</returns>
	public CrudTargetConfig<TModel> ConditionalInclude<TProp>(Expression<Func<TModel, TProp>> navigation, bool include)
	{
		if (!include) {
			return this;
		}

		Query = Query.Include(navigation);
		return this;
	}

	/// <summary>
	/// Include two navigational props in the final result
	/// </summary>
	/// <param name="navigation">An expression returning the navigational prop you wish to include</param>
	/// <param name="navigation2">A continued expression returning the second navigational prop you wish to include</param>
	/// <typeparam name="TProp">The type of the first navigational prop</typeparam>
	/// <typeparam name="TProp2">The type of the second navigational prop</typeparam>
	/// <returns>The current Target</returns>
	public CrudTargetConfig<TModel> Include<TProp, TProp2>(
		Expression<Func<TModel, IEnumerable<TProp>>> navigation,
		Expression<Func<TProp, TProp2>> navigation2
	)
	{
		Query = Query.Include(navigation).ThenInclude(navigation2);
		return this;
	}

	/// <summary>
	/// Order the query by a property in ascending order
	/// </summary>
	/// <param name="navigation">An expression returning the prop you wish to order by</param>
	/// <typeparam name="TProp">The type of the prop to order by</typeparam>
	/// <returns>The current Target</returns>
	public CrudTargetConfig<TModel> OrderBy<TProp>(Expression<Func<TModel, TProp>> navigation)
	{
		Query = Query.OrderBy(navigation);
		return this;
	}

	/// <summary>
	/// Order the query by a property in descending order
	/// </summary>
	/// <param name="navigation">An expression returning the prop you wish to order by</param>
	/// <typeparam name="TProp">The type of the prop to order by</typeparam>
	/// <returns>The current Target</returns>
	public CrudTargetConfig<TModel> OrderByDescending<TProp>(Expression<Func<TModel, TProp>> navigation)
	{
		Query = Query.OrderByDescending(navigation);
		return this;
	}
	
	/// <summary>
	/// Define Custom Exception Failed Query
	/// </summary>
	/// <param name="exception">New exception text - Used for all exceptions</param>
	/// <returns>The current Config</returns>
	public CrudTargetConfig<TModel> WithException(string exception)
	{
		CustomException = exception;
		return this;
	}
		
	#endregion

	#region CRUD Actions

	/// <summary>
	///  Update the targeted model using an <paramref name="updateModel"/>
	/// </summary>
	/// <param name="updateModel">The updateModel to apply to the target model</param>
	/// <typeparam name="TUpdate">The type of the <paramref name="updateModel"/></typeparam>
	/// <returns>A <see cref="CrudUpdateConfig{TModel,TUpdate}"/> instance to configure and execute the update</returns>
	public CrudUpdateConfig<TModel, TUpdate> Update<TUpdate>(TUpdate updateModel)
	{
		return new CrudUpdateConfig<TModel, TUpdate>(this, updateModel);
	}

	/// <summary>
	///  Update the targeted model using a method
	/// </summary>
	/// <param name="modify">A method to update the model</param>
	/// <returns>A <see cref="CrudUpdateConfig{TModel,TUpdate}"/> instance to configure and execute the update</returns>
	public CrudUpdateConfig<TModel, TModel> Update(Action<TModel> modify)
	{
		return new CrudUpdateConfig<TModel, TModel>(this, modify);
	}

	/// <summary>
	/// Delete the targeted model
	/// </summary>
	/// <returns>A <see cref="CrudDeleteConfig{TModel}"/> instance to configure and execute the delete operation</returns>
	public CrudDeleteConfig<TModel> Delete()
	{
		return new CrudDeleteConfig<TModel>(this);
	}

	/// <summary>
	/// Create a new model in the current <see cref="Set"/>
	/// </summary>
	/// <param name="createModel">The data used to populate the new Entity</param>
	/// <typeparam name="TCreate">The type of the population data</typeparam>
	/// <returns>A <see cref="CrudCreateConfig{TModel,TCreate}"/> instance to configure and execute the create operation</returns>
	public CrudCreateConfig<TModel, TCreate> Create<TCreate>(TCreate createModel)
	{
		return new CrudCreateConfig<TModel, TCreate>(this, createModel);
	}

	#endregion

	#region Read Operations

	/// <summary>
	/// Return the first element of the list
	/// </summary>
	/// <returns>An element of <typeparamref name="TModel"/> if found. Returns <c>null</c> if no element can be found</returns>
	public async Task<TModel?> FirstOrDefaultAsync()
	{
		try {
			var model = await Query
			   .ConditionalWhere(Filter, Filter != null)
			   .ConditionalSkip(SkipAmount, SkipAmount > 0)
			   .FirstOrDefaultAsync();

			if (model is null) return model;

			if (LoadActions.Count > 0) {
				var entry = Context.Entry(model);
				foreach (var loadAction in LoadActions) {
					await loadAction(entry);
				}
			}

			return model;
		}
		catch (RetryLimitExceededException e) {
			throw new DatabaseLoadException("We are experiencing high traffic right now, try again later", e);
		}
	}

	/// <summary>
	/// Return the first element mapped using the given <paramref name="selector"/>
	/// </summary>
	/// <param name="selector">A mapping to apply to the model</param>
	/// <typeparam name="TResult">The mapped type</typeparam>
	/// <returns>The mapped result from the first entity in the query. Returns <c>null</c> if no element can be found</returns>
	public async Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<TModel, TResult>> selector)
	{
		try {
			return await Query
			   .ConditionalWhere(Filter, Filter != null)
			   .ConditionalSkip(SkipAmount, SkipAmount > 0)
			   .Select(selector)
			   .FirstOrDefaultAsync();
		}
		catch (RetryLimitExceededException e) {
			throw new DatabaseLoadException("We are experiencing high traffic right now, try again later", e);
		}
	}

	/// <summary>
	/// Return the first element mapped using AutoMapper
	/// </summary>
	/// <typeparam name="TMap">The type to map to</typeparam>
	/// <returns>The mapped result from the first entity in the query. Returns <c>null</c> if no element can be found</returns>
	public async Task<TMap?> MapFirstOrDefaultAsync<TMap>() where TMap : class
	{
		if (Mapper == null) {
			throw new InternalException("DbContext does not expose IMapper");
		}

		try {
			if (!WillProjectInMemory) {
				return await Query
				   .ConditionalWhere(Filter, Filter != null)
				   .ConditionalSkip(SkipAmount, SkipAmount > 0)
				   .ProjectTo<TMap>(Mapper.ConfigurationProvider)
				   .FirstOrDefaultAsync();
			}

			var model = await Query
			   .ConditionalWhere(Filter, Filter != null)
			   .ConditionalSkip(SkipAmount, SkipAmount > 0)
			   .FirstOrDefaultAsync();
			
			if (model is null) return null;

			if (LoadActions.Count > 0) {
				var entry = Context.Entry(model);
				foreach (var loadAction in LoadActions) {
					await loadAction(entry);
				}
			}

			return Mapper.Map<TMap>(model);
		}
		catch (RetryLimitExceededException e) {
			throw new DatabaseLoadException("We are experiencing high traffic right now, try again later", e);
		}
	}

	/// <summary>
	/// Return the first matching element
	/// </summary>
	/// <returns>An element of type <typeparamref name="TModel"/></returns>
	/// <exception cref="NotFoundException">If the element is not found, an exception is thrown. The exception has a message describing what was missing</exception>
	public async Task<TModel> FirstAsync()
	{
		var model = await FirstOrDefaultAsync();
		if (model == null) throw GenerateException();
		return model;
	}

	/// <summary>
	/// Return the first element mapped using the given <paramref name="selector"/>
	/// </summary>
	/// <param name="selector">A mapping to apply to the model</param>
	/// <typeparam name="TResult">The mapped type</typeparam>
	/// <returns>The mapped result from the first entity in the query</returns>
	/// <exception cref="NotFoundException">If an element is not found, an exception is thrown. The exception has a message describing what was missing</exception>
	public async Task<TResult> FirstAsync<TResult>(Expression<Func<TModel, TResult>> selector)
	{
		var model = await FirstOrDefaultAsync(selector);
		if (model == null) throw GenerateException();
		return model;
	}

	/// <summary>
	/// Return the first element mapped using AutoMapper
	/// </summary>
	/// <typeparam name="TMap">The type to map to</typeparam>
	/// <returns>The mapped result from the first entity in the query</returns>
	/// <exception cref="NotFoundException">If an element is not found, an exception is thrown. The exception has a message describing what was missing</exception>
	public async Task<TMap> MapFirstAsync<TMap>() where TMap : class
	{
		var model = await MapFirstOrDefaultAsync<TMap>();
		if (model == null) throw GenerateException();
		return model;
	}

	protected Exception GenerateException()
	{
		return CustomException == null 
			? CrudErrorHelper.GenerateError(Filter) 
			: new NotFoundException(CustomException);
	}

	/// <summary>
	/// Returns a list of all the results from the query
	/// </summary>
	/// <returns>A list of all elements in the query</returns>
	public async Task<List<TModel>> ToListAsync()
	{
		try {
			return await Query
			   .ConditionalWhere(Filter, Filter != null)
			   .ConditionalSkip(SkipAmount, SkipAmount > 0)
			   .ConditionalTake(TakeAmount, TakeAmount > 0)
			   .ToListAsync();
		}
		catch (RetryLimitExceededException e) {
			throw new DatabaseLoadException("We are experiencing high traffic right now, try again later", e);
		}
	}

	/// <summary>
	/// Returns a list of all the results from the query, mapped using the <paramref name="selector"/>
	/// </summary>
	/// <typeparam name="TResult">The type to map the items to</typeparam>
	/// <returns>A mapped list of all elements in the query</returns>
	public async Task<List<TResult>> ToListAsync<TResult>(Expression<Func<TModel, TResult>> selector)
	{
		try {
			return await Query
			   .ConditionalWhere(Filter, Filter != null)
			   .ConditionalSkip(SkipAmount, SkipAmount > 0)
			   .ConditionalTake(TakeAmount, TakeAmount > 0)
			   .Select(selector)
			   .ToListAsync();
		}
		catch (RetryLimitExceededException e) {
			throw new DatabaseLoadException("We are experiencing high traffic right now, try again later", e);
		}
	}

	/// <summary>
	/// Returns a list of all the results from the query, mapped using AutoMapper
	/// </summary>
	/// <typeparam name="TMap">The type to map the items to</typeparam>
	/// <returns>A mapped list of all elements in the query</returns>
	public async Task<List<TMap>> ToMappedListAsync<TMap>() where TMap : class
	{
		if (Mapper == null) {
			throw new InternalException("DbContext does not expose IMapper");
		}

		var q = Query
		   .ConditionalWhere(Filter, Filter != null)
		   .ConditionalSkip(SkipAmount, SkipAmount > 0)
		   .ConditionalTake(TakeAmount, TakeAmount > 0);

		try {
			if (!WillProjectInMemory) {
				return await q
				   .ProjectTo<TMap>(Mapper.ConfigurationProvider)
				   .ToListAsync();
			}

			var models = await q.ToListAsync();

			return Mapper.Map<List<TMap>>(models);
		}
		catch (RetryLimitExceededException e) {
			throw new DatabaseLoadException("We are experiencing high traffic right now, try again later", e);
		}
	}
		
	/// <summary>
	/// Throws an exception with a descriptive error message if the entity does not exist
	/// </summary>
	/// <returns>Void</returns>
	/// <exception cref="NotFoundException">Exception thrown when entity does not exist</exception>
	public async Task EnsureExistsAsync()
	{
		try {
			var exists = await Query.ConditionalWhere(Filter, Filter != null).AnyAsync();

			if (exists) {
				return;
			}
		}
		catch (RetryLimitExceededException e) {
			throw new DatabaseLoadException("We are experiencing high traffic right now, try again later", e);
		}

		throw GenerateException();

	}

	#endregion

	/// <summary>
	/// Splits the query execution into multiple queries for nested collections
	/// </summary>
	/// <returns>The current configuration</returns>
	public CrudTargetConfig<TModel> AsSplitQuery()
	{
		Query = Query.AsSplitQuery();
		return this;
	}
	
	public CrudTargetConfig<TModel> AsNoTracking()
	{
		Query = Query.AsNoTracking();
		return this;
	}
}