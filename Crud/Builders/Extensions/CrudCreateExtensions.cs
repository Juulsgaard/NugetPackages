using System.Linq.Expressions;
using Crud.Domain.Interfaces;

namespace Crud.Builders.Extensions;

public static class CrudCreateExtensions
{
    /// <summary>
    /// Adds an Index to the created Entity
    /// </summary>
    /// <param name="target">The current <see cref="CrudCreateConfig{TModel,TCreate}"/></param>
    /// <param name="filter">A filter identifying what elements are in the same indexed subset</param>
    /// <typeparam name="TModel">The tracked Model</typeparam>
    /// <typeparam name="TCreate">The createModel of the current Config</typeparam>
    /// <returns>The current Config</returns>
    public static CrudCreateConfig<TModel, TCreate> AddSortingIndex<TModel, TCreate>(
        this CrudCreateConfig<TModel, TCreate> target,
        Expression<Func<TModel, bool>>? filter = null
    ) where TModel : class, ISorted
    {
        target.SortingIndexFilter = filter ?? (x => true);
        return target;
    }
}