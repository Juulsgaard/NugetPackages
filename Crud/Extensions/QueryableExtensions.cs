using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Juulsgaard.Crud.Extensions;

public static class QueryableExtensions
{
	/// <summary>
	/// Where statement only applied if <paramref name="apply"/> is true
	/// </summary>
	/// <param name="query">The Query</param>
	/// <param name="predicate">The Where clause</param>
	/// <param name="apply">The application condition</param>
	/// <typeparam name="TModel">Type of the entity</typeparam>
	/// <returns>The modified Query</returns>
	public static IQueryable<TModel> ConditionalWhere<TModel>(
		this IQueryable<TModel> query,
		Expression<Func<TModel, bool>>? predicate,
		bool apply
	)
	{
		if (!apply || predicate == null) {
			return query;
		}

		return query.Where(predicate);
	}

	/// <summary>
	/// Take statement only applied if <paramref name="apply"/> is true
	/// </summary>
	/// <param name="query">The original query</param>
	/// <param name="count">The amount to take</param>
	/// <param name="apply">The application condition</param>
	/// <typeparam name="TModel">Type of the entity</typeparam>
	/// <returns>The modified Query</returns>
	public static IQueryable<TModel> ConditionalTake<TModel>(this IQueryable<TModel> query, int count, bool apply)
	{
		return !apply ? query : query.Take(count);
	}

	/// <summary>
	/// Skip statement only applied if <paramref name="apply"/> is true
	/// </summary>
	/// <param name="query">The original query</param>
	/// <param name="count">The amount to skip</param>
	/// <param name="apply">The application condition</param>
	/// <typeparam name="TModel">Type of the entity</typeparam>
	/// <returns>The modified Query</returns>
	public static IQueryable<TModel> ConditionalSkip<TModel>(this IQueryable<TModel> query, int count, bool apply)
	{
		return !apply ? query : query.Skip(count);
	}

	public static async Task<Dictionary<TKey, List<TVal>>> ToLookupAsync<TModel, TKey, TVal>(
		this IQueryable<TModel> query,
		Func<TModel, TKey> keySelector,
		Func<TModel, TVal> valueSelector
	) where TKey : notnull
	{
		var dict = new Dictionary<TKey, List<TVal>>();
			
		await foreach (var model in query.AsAsyncEnumerable()) {
			var key = keySelector(model);
			var value = valueSelector(model);
				
			if (!dict.ContainsKey(key)) {
				dict.Add(key, new List<TVal> {value});
			} else {
				dict[key].Add(value);
			}
		}

		return dict;
	}
		
	public static async Task<Dictionary<TKey, HashSet<TVal>>> ToSetLookupAsync<TModel, TKey, TVal>(
		this IQueryable<TModel> query,
		Func<TModel, TKey> keySelector,
		Func<TModel, TVal> valueSelector
	) where TKey : notnull
	{
		var dict = new Dictionary<TKey, HashSet<TVal>>();
			
		await foreach (var model in query.AsAsyncEnumerable()) {
			var key = keySelector(model);
			var value = valueSelector(model);
				
			if (!dict.ContainsKey(key)) {
				dict.Add(key, new HashSet<TVal> {value});
			} else {
				dict[key].Add(value);
			}
		}

		return dict;
	}
	
	public static async Task<HashSet<TVal>> ToSetAsync<TModel, TVal>(
		this IQueryable<TModel> query,
		Func<TModel, TVal> valueSelector
	)
	{
		var set = new HashSet<TVal>();
			
		await foreach (var model in query.AsAsyncEnumerable()) {
			var value = valueSelector(model);
			set.Add(value);
		}

		return set;
	}
}