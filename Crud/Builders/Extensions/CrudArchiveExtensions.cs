using System.Linq.Expressions;
using Crud.Domain.Interfaces;
using Crud.Extensions;

namespace Crud.Builders.Extensions;

public static class CrudArchiveExtensions
{
	/// <summary>
	/// Updates surrounding indices while archiving
	/// </summary>
	/// <param name="target">The current <see cref="CrudArchiveConfig{TModel}"/></param>
	/// <param name="identifier">An optional identifier that return <c>true</c> if the two items are in the same subset</param>
	/// <typeparam name="TModel">The tracked model</typeparam>
	/// <returns>The current Config</returns>
	/// <remarks>
	/// <para>It is recommended to use the shorthand for this method <see cref="WithSortingIndex{TModel,TProp}"/> whenever possible!</para>
	/// <para>The <paramref name="identifier"/> compares two properties that categorizes a subset of items with their own order</para>
	/// </remarks>
	public static CrudArchiveConfig<TModel> WithSortingIndex<TModel>(
		this CrudArchiveConfig<TModel> target,
		Func<TModel, Expression<Func<TModel, bool>>>? identifier = null
	) where TModel : class, ISorted, IArchivable
	{
		target.UpdateIndex = true;
		target.SubSetIdentifier = identifier;
		return target;
	}

	/// <summary>
	/// Updates surrounding indices while archiving
	/// </summary>
	/// <param name="target">The current <see cref="CrudArchiveConfig{TModel}"/></param>
	/// <param name="selector">An optional delegate that returns the property by which to identify a subset of indices</param>
	/// <typeparam name="TModel">The tracked Model</typeparam>
	/// <typeparam name="TProp">The type of the grouping prop</typeparam>
	/// <returns>The current Config</returns>
	/// <remarks>The <paramref name="selector"/>'s target is a property that categorizes a subset of items with their own order</remarks>
	public static CrudArchiveConfig<TModel> WithSortingIndex<TModel, TProp>(
		this CrudArchiveConfig<TModel> target,
		Expression<Func<TModel, TProp>>? selector = null
	) where TModel : class, ISorted, IArchivable
	{
		if (selector == null) return target.WithSortingIndex();
		return target.WithSortingIndex(selector.ToIdentifier());
	}
}