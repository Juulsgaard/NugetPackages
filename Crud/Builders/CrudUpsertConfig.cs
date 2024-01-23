using System.Linq.Expressions;
using Lib.Exceptions;

namespace Crud.Builders;

/// <summary>
/// This is the config for Updating or Creating (upserting) an Entity
/// </summary>
/// <typeparam name="TModel">The tracked Model</typeparam>
/// <typeparam name="TUpdate">The type of the <see cref="CrudUpdateConfig{TModel,TUpdate}.UpdateModel"/> used to update the model</typeparam>
/// <typeparam name="TCreate">The type of the <see cref="CrudCreateConfig{TModel,TCreate}.CreateModel"/> used to create a new Entity</typeparam>
public class CrudUpsertConfig<TModel, TUpdate, TCreate> : CrudUpdateConfig<TModel, TUpdate> where TModel : class
{
	protected internal CrudCreateConfig<TModel, TCreate> CreateConfig { get; }
	protected internal Expression<Func<TModel, bool>>? IndexFilter => CreateConfig.SortingIndexFilter;

	public CrudUpsertConfig(CrudUpdateConfig<TModel, TUpdate> crudUpdateConfig, TCreate createModel) : base(crudUpdateConfig)
	{
		CreateConfig = new CrudCreateConfig<TModel, TCreate>(crudUpdateConfig.Target, createModel);
			
		if (ModifyFunc != null) {
			CreateConfig.Modify(ModifyFunc);
		}
	}


	/// <summary>
	/// Enable saving of the updated / created model
	/// </summary>
	/// <returns>The current Config</returns>
	/// <remarks>Without applying this config you will have to manually save after executing</remarks>
	public new CrudUpsertConfig<TModel, TUpdate, TCreate> Save()
	{
		base.Save();
		CreateConfig.Save();
		return this;
	}

	/// <summary>
	/// Add a modification method that will be applied after the
	/// <see cref="CrudCreateConfig{TModel,TCreate}.CreateModel"/> / <see cref="CrudUpdateConfig{TModel,TUpdate}.UpdateModel"/>
	/// is mapped to a <typeparamref name="TModel"/>, but before the entity is updated / created
	/// </summary>
	/// <param name="func">The modification to be applied to the mapped <typeparamref name="TModel"/></param>
	/// <returns>The current Config</returns>
	/// <remarks>
	/// No matter if the entity is updated or created this will still be applied just before saving.
	/// If saving is not enabled then it will apply right before execution is finished.
	/// </remarks>
	public new CrudUpsertConfig<TModel, TUpdate, TCreate> Modify(Action<TModel> func)
	{
		CreateConfig.Modify(func);
		return this;
	}
	
	/// <summary>
	/// Define Exception text overrides for different Exception types when creating
	/// </summary>
	/// <param name="default">Default exception text - Used for all undefined exceptions</param>
	/// <param name="uniqueConflict">Exception text for broken unique constraints</param>
	/// <param name="nullInsert">Exception text for null inserts in non-nullable fields</param>
	/// <param name="concurrency">Exception text for concurrency errors</param>
	/// <returns>The current Config</returns>
	public CrudUpsertConfig<TModel, TUpdate, TCreate> WithExceptions(
		string? @default = null,
		string? uniqueConflict = null,
		string? nullInsert = null,
		string? concurrency = null
	)
	{
		CreateConfig.WithExceptions(@default, uniqueConflict, nullInsert, concurrency);
		return this;
	}

	/// <summary>
	/// Executes the configured upsert
	/// </summary>
	/// <returns>The updated / created model</returns>
	public new async Task<TModel> ExecuteAsync()
	{
		var model = await Target.FirstOrDefaultAsync();
		TModel result;
			
		if (model == null) {
			result = await CreateConfig.ExecuteAsync();
		} else {

			result = Update(model);
			await DetectParentChanges(result);

			if (WillSave) {
				await Context.SaveChangesAsync();
			}
		}
			
		return result;
	}

	/// <summary>
	/// Executes the configured upsert, and maps it to <typeparamref name="TMap"/>
	/// </summary>
	/// <typeparam name="TMap">The type to map the Model to</typeparam>
	/// <returns>The updated / created and mapped model</returns>
	public new async Task<TMap> ExecuteAndMapAsync<TMap>()
	{
		if (Mapper == null) {
			throw new InternalException("DbContext does not expose IMapper");
		}
			
		var model = await ExecuteAsync();
		return Mapper.Map<TMap>(model);
	}
}