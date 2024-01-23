using System.Linq.Expressions;
using Juulsgaard.Crud.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Juulsgaard.Crud.Builders.Extensions;

public static class BuilderExtensions
{
	/// <summary>
	/// Enter CRUD Syntax
	/// </summary>
	/// <param name="set">The Set to target all CRUD operations on</param>
	/// <param name="filter">An optional filter to specify the target of CRUD operations</param>
	/// <typeparam name="TModel">The tracked Model</typeparam>
	/// <returns>A <see cref="CrudTargetConfig{TModel}"/> for further CRUD operations</returns>
	public static CrudTargetConfig<TModel> Target<TModel>(this DbSet<TModel> set, Expression<Func<TModel, bool>>? filter = null)
		where TModel : class
	{
		return new CrudTargetConfig<TModel>(set, filter);
	}

	/// <summary>
	/// Create a new Entity
	/// </summary>
	/// <param name="set">The set to add the new Entity to</param>
	/// <param name="createModel">The data used to populate the created Entity</param>
	/// <typeparam name="TModel">The tracked Model</typeparam>
	/// <typeparam name="TCreate">The createModel used to populate a new Entity</typeparam>
	/// <returns>A <see cref="CrudCreateConfig{TModel,TCreate}"/> for configuration and execution of the create operation</returns>
	public static CrudCreateConfig<TModel, TCreate> Create<TModel, TCreate>(this DbSet<TModel> set, TCreate createModel)
		where TModel : class
	{
		return new CrudCreateConfig<TModel, TCreate>(set.Target(), createModel);
	}

	/// <summary>
	/// Delete an Entity
	/// </summary>
	/// <param name="set">The set to remove the Entity from</param>
	/// <param name="filter">A selector to target the element to be deleted</param>
	/// <typeparam name="TModel">The tracked Model</typeparam>
	/// <returns>A <see cref="CrudDeleteConfig{TModel}"/> for configuration and execution of the delete operation</returns>
	public static CrudDeleteConfig<TModel> Delete<TModel>(this DbSet<TModel> set, Expression<Func<TModel, bool>> filter)
		where TModel : class
	{
		return new CrudDeleteConfig<TModel>(set.Target(filter));
	}
	
	/// <summary>
	/// Archive an Entity
	/// </summary>
	/// <param name="set">The set to archive the Entity from</param>
	/// <param name="filter">A selector to target the element to be archived</param>
	/// <typeparam name="TModel">The tracked Model</typeparam>
	/// <returns>A <see cref="CrudArchiveConfig{TModel}"/> for configuration and execution of the archive operation</returns>
	public static CrudArchiveConfig<TModel> Archive<TModel>(this DbSet<TModel> set, Expression<Func<TModel, bool>> filter)
		where TModel : class, IArchivable
	{
		return new CrudArchiveConfig<TModel>(set.Target(filter));
	}
	
	/// <summary>
	/// Restore an Entity from Archival
	/// </summary>
	/// <param name="set">The set to restore the Entity to</param>
	/// <param name="filter">A selector to target the element to be restored</param>
	/// <typeparam name="TModel">The tracked Model</typeparam>
	/// <returns>A <see cref="CrudArchiveConfig{TModel}"/> for configuration and execution of the restore operation</returns>
	public static CrudArchiveConfig<TModel> Restore<TModel>(this DbSet<TModel> set, Expression<Func<TModel, bool>> filter)
		where TModel : class, IArchivable
	{
		return new CrudArchiveConfig<TModel>(set.Target(filter), true);
	}
		
	/// <summary>
	/// Move an entity
	/// </summary>
	/// <param name="target">The target to move</param>
	/// <param name="index">The new index of the targeted Entity</param>
	/// <typeparam name="TModel">The tracked Model</typeparam>
	/// <returns>A <see cref="CrudMoveConfig{TModel}"/> for configuration and execution of the move operation</returns>
	public static CrudMoveConfig<TModel> Move<TModel>(this CrudTargetConfig<TModel> target, int index) where TModel : class, ISorted
	{
		return new CrudMoveConfig<TModel>(target, index);
	}
	
	/// <summary>
	/// Archive an Entity
	/// </summary>
	/// <param name="target">The target to archive</param>
	/// <typeparam name="TModel">The tracked Model</typeparam>
	/// <returns>A <see cref="CrudArchiveConfig{TModel}"/> for configuration and execution of the archive operation</returns>
	public static CrudArchiveConfig<TModel> Archive<TModel>(this CrudTargetConfig<TModel> target) where TModel : class, IArchivable
	{
		return new CrudArchiveConfig<TModel>(target);
	}
}