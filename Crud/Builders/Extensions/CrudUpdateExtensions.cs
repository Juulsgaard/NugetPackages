using System.Linq.Expressions;
using Juulsgaard.Crud.Domain.Interfaces;
using Juulsgaard.Crud.Monitoring;

namespace Juulsgaard.Crud.Builders.Extensions;

public static class CrudUpdateExtensions
{
    /// <summary>
    /// Define a parent property, that has it's own index order
    /// </summary>
    /// <param name="target">The current <see cref="CrudUpdateConfig{TModel,TUpdate}"/></param>
    /// <param name="selector">A delegate returning the parent property</param>
    /// <typeparam name="TModel">The tracked Model</typeparam>
    /// <typeparam name="TUpdate">The updateModel from the current Config</typeparam>
    /// <typeparam name="TParent">The type of the parent prop</typeparam>
    /// <returns>The current Config</returns>
    /// <remarks>
    /// When this is applied and an item is moved between parents it will automatically update all surrounding indices
    /// in order to fill the gap the entity left, and to assign a new index at the end of the new indexed group
    /// </remarks>
    public static CrudUpdateConfig<TModel, TUpdate> HasParent<TModel, TUpdate, TParent>(
        this CrudUpdateConfig<TModel, TUpdate> target,
        Expression<Func<TModel, TParent>> selector
    ) where TModel : class, ISorted
    {
        var monitor = new PropertyUpdateMonitor<TModel, TParent>(selector, target.Context);
        target.ParentMonitors.Add(monitor);
        return target;
    }
}