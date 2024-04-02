using System.Linq.Expressions;
using AutoMapper;
using Juulsgaard.Crud.Domain.Interfaces;
using Juulsgaard.Crud.Extensions;
using Juulsgaard.Crud.Models;
using Juulsgaard.Tools.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Juulsgaard.Crud.Builders;

/// <summary>
/// This is the config for creating a new Entity
/// </summary>
/// <typeparam name="TModel">The tracked model</typeparam>
/// <typeparam name="TCreate">The type of the <see cref="CreateModel"/> used to create the entity</typeparam>
public class CrudCreateConfig<TModel, TCreate> where TModel : class
{
	protected internal CrudTargetConfig<TModel> Target { get; protected set; }
	protected internal TCreate CreateModel { get; protected set; }

	protected internal Action<TModel>? ModifyFunc { get; protected set; }
	protected internal bool WillSave { get; protected set; }
	protected internal Expression<Func<TModel, bool>>? SortingIndexFilter { get; set; }
		
	protected internal DbContext Context => Target.Context;
	protected internal IMapper? Mapper => Target.Mapper;
	protected CrudExceptionLookup? ExceptionLookup;


	/// <summary>
	/// Manually instantiate a <see cref="CrudCreateConfig{TModel,TCreate}"/>
	/// </summary>
	/// <param name="target">The target containing the target <see cref="CrudTargetConfig{TModel}.Set"/></param>
	/// <param name="createModel">The data for creating a new Entity</param>
	/// <remarks>It's preferred to instantiate a <see cref="CrudCreateConfig{TModel,TCreate}"/> using the fluid CRUD syntax</remarks>
	public CrudCreateConfig(CrudTargetConfig<TModel> target, TCreate createModel)
	{
		Target = target;
		CreateModel = createModel;
	}

	/// <summary>
	/// Add a modification method that will be applied after the <see cref="CreateModel"/> is mapped to a <typeparamref name="TModel"/>,
	/// but before the entity is created
	/// </summary>
	/// <param name="func">The modification to be applied to the mapped <typeparamref name="TModel"/></param>
	/// <returns>The current Config</returns>
	/// <example>
	///     Add an owner to a newly created <c>Dog</c> (When the Owner prop isn't in the createModel)
	///     <code>
	///         Context.Dogs
	///             .Create(new DogCreateModel {Name = "Fido"})
	///             .Modify(dog => dog.Owner = userId)
	///             .ExecuteAsync()
	///     </code>
	/// </example>
	public CrudCreateConfig<TModel, TCreate> Modify(Action<TModel> func)
	{
		ModifyFunc = func;
		return this;
	}

	/// <summary>
	/// Enable saving of the new entity
	/// </summary>
	/// <returns>The current Config</returns>
	/// <remarks>Without applying this config you will have to manually save after executing</remarks>
	public CrudCreateConfig<TModel, TCreate> Save()
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
	/// <returns>The current Config</returns>
	public CrudCreateConfig<TModel, TCreate> WithExceptions(
		string? @default = null,
		string? uniqueConflict = null,
		string? nullInsert = null,
		string? concurrency = null
	)
	{
		ExceptionLookup = new() {
			UniqueConflict = uniqueConflict ?? @default,
			NullInsert = nullInsert ?? @default,
			Concurrency = concurrency ?? @default,
			Default = @default
		};
		return this;
	}

	/// <summary>
	/// Executes the create configuration
	/// </summary>
	/// <returns>The created model</returns>
	public async ValueTask<TModel> ExecuteAsync()
	{
		if (Mapper == null) {
			throw new InternalException("DbContext does not expose IMapper");
		}
			
		var model = Mapper.Map<TModel>(CreateModel);

		if (SortingIndexFilter != null) {
			var q = Target.Set.Where(SortingIndexFilter);

			if (q is IQueryable<ISorted> query && model is ISorted sortableModel) {
				sortableModel.Index = await query.OrderByDescending(f => f.Index)
				   .Select(f => f.Index + 1)
				   .FirstOrDefaultAsync();
			}
		}

		ModifyFunc?.Invoke(model);

		Target.Set.Add(model);

		if (!WillSave) return model;
		try {
			await Context.SaveChangesAsync();
		}
		catch (DbUpdateException e) {
			throw e.ProcessAsCreate(Target.EntityName, ExceptionLookup);
		}
		
		return model;
	}

	/// <summary>
	/// Executes the creation configuration and maps to <typeparamref name="TMap"/>
	/// </summary>
	/// <typeparam name="TMap">The type to map the Model to</typeparam>
	/// <returns>The updated and mapped model</returns>
	public async Task<TMap> ExecuteAndMapAsync<TMap>()
	{
		var model = await ExecuteAsync();
		return Mapper!.Map<TMap>(model);
	}

	/// <summary>
	/// Tries to fetch the targeted model, and executes the configured creation if the target cannot be found
	/// </summary>
	/// <returns>The fetched or created model</returns>
	/// <remarks>This is referred to as <c>GetOrCreate</c></remarks>
	public async Task<TModel> FirstOrCreateAsync()
	{
		var model = await Target.FirstOrDefaultAsync();
		if (model == null) {
			return await ExecuteAsync();
		}

		return model;
	}

	/// <summary>
	/// Tries to fetch the targeted model, and executes the configured creation if the target cannot be found
	/// </summary>
	/// <typeparam name="TMap">The model to map the resulting type to</typeparam>
	/// <returns>The fetched or created model mapped to <typeparamref name="TMap"/></returns>
	/// <remarks>This is referred to as <c>GetOrCreate</c></remarks>
	public async Task<TMap> MapFirstOrCreatedAsync<TMap>() where TMap : class
	{
		var model = await Target.MapFirstOrDefaultAsync<TMap>();
		if (model == null) {
			return await ExecuteAndMapAsync<TMap>();
		}

		return model;
	}
}