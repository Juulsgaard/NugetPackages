using System.Linq.Expressions;
using Crud.Domain.Interfaces;

namespace Crud.Builders.Extensions;

public static class CrudDeleteExtensions
{
	/// <summary>
	/// Updates surrounding indices while deleting
	/// </summary>
	/// <param name="target">The current <see cref="CrudDeleteConfig{TModel}"/></param>
	/// <param name="identifier">An optional identifier that return <c>true</c> if the two items are in the same subset</param>
	/// <typeparam name="TModel">The tracked model</typeparam>
	/// <returns>The current Config</returns>
	/// <remarks>
	/// <para>It is recommended to use the shorthand for this method <see cref="WithSortingIndex{TModel,TProp}"/> whenever possible!</para>
	/// <para>The <paramref name="identifier"/> compares two properties that categorizes a subset of items with their own order</para>
	/// </remarks>
	public static CrudDeleteConfig<TModel> WithSortingIndex<TModel>(
		this CrudDeleteConfig<TModel> target,
		Func<TModel, Expression<Func<TModel, bool>>>? identifier = null
	) where TModel : class, ISorted
	{
		target.UpdateIndex = true;
		target.SubSetIdentifier = identifier;
		return target;
	}

	/// <summary>
	/// Updates surrounding indices while deleting
	/// </summary>
	/// <param name="target">The current <see cref="CrudDeleteConfig{TModel}"/></param>
	/// <param name="selector">An optional delegate that returns the property by which to identify a subset of indices</param>
	/// <typeparam name="TModel">The tracked Model</typeparam>
	/// <typeparam name="TProp">The type of the grouping prop</typeparam>
	/// <returns>The current Config</returns>
	/// <remarks>The <paramref name="selector"/>'s target is a property that categorizes a subset of items with their own order</remarks>
	public static CrudDeleteConfig<TModel> WithSortingIndex<TModel, TProp>(
		this CrudDeleteConfig<TModel> target,
		Expression<Func<TModel, TProp>>? selector = null
	) where TModel : class, ISorted
	{
		if (selector == null) return target.WithSortingIndex();

		Expression<Func<TModel, bool>> Identifier(TModel model)
		{
			var value = selector.Compile()(model);
			var constant = Expression.Constant(value, typeof(TProp));
			var comparison = Expression.Equal(selector.Body, constant);
			return Expression.Lambda<Func<TModel, bool>>(comparison, selector.Parameters[0]);
		}

		return target.WithSortingIndex(Identifier);
	}
}