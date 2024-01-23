using System.Linq.Expressions;
using Juulsgaard.Crud.Domain.Interfaces;

namespace Juulsgaard.Crud.Builders.Extensions;

public static class CrudUpsertExtensions
{
    /// <summary>
    /// Adds an Index to the created Entity
    /// </summary>
    /// <param name="target">The current <see cref="CrudUpsertConfig{TModel,TUpdate,TCreate}"/></param>
    /// <param name="filter">A filter identifying what elements are in the same indexed subset</param>
    /// <typeparam name="TModel">The tracked Model</typeparam>
    /// <typeparam name="TUpdate">The updateModel of the current Config</typeparam>
    /// <typeparam name="TCreate">The createModel of the current Config</typeparam>
    /// <returns>The current Config</returns>
    public static CrudUpsertConfig<TModel, TUpdate, TCreate> AddSortingKey<TModel, TUpdate, TCreate>(
        this CrudUpsertConfig<TModel, TUpdate, TCreate> target,
        Expression<Func<TModel, bool>> filter
    ) where TModel : class, ISorted
    {
        target.CreateConfig.AddSortingIndex(filter);
        return target;
    }
}