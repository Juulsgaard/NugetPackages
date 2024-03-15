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

	/// <summary>
	/// Execute the query and aggregate the result in a Lookup
	/// </summary>
	/// <param name="query">The query</param>
	/// <param name="keySelector">A selector for the key</param>
	/// <param name="valueSelector">A selector for the value</param>
	/// <returns>A Lookup Dictionary where values with the same key are grouped into lists</returns>
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
	
	/// <summary>
	/// Execute the query and aggregate the result in a Lookup
	/// </summary>
	/// <param name="query">The query</param>
	/// <param name="keySelector">A selector for the key</param>
	/// <returns>A Lookup Dictionary where values with the same key are grouped into lists</returns>
	public static Task<Dictionary<TKey, List<TModel>>> ToLookupAsync<TModel, TKey>(
		this IQueryable<TModel> query,
		Func<TModel, TKey> keySelector
	) where TKey : notnull
	{
		return query.ToLookupAsync(keySelector, x => x);
	}
		
	/// <summary>
	/// Execute the query and aggregate the result in a Lookup
	/// </summary>
	/// <param name="query">The query</param>
	/// <param name="keySelector">A selector for the key</param>
	/// <param name="valueSelector">A selector for the value</param>
	/// <returns>A Lookup Dictionary where values with the same key are grouped into hash sets</returns>
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
	
	/// <summary>
	/// Execute the query and aggregate the result in a Lookup
	/// </summary>
	/// <param name="query">The query</param>
	/// <param name="keySelector">A selector for the key</param>
	/// <returns>A Lookup Dictionary where values with the same key are grouped into hash sets</returns>
	public static Task<Dictionary<TKey, HashSet<TModel>>> ToSetLookupAsync<TModel, TKey>(
		this IQueryable<TModel> query,
		Func<TModel, TKey> keySelector
	) where TKey : notnull
	{
		return query.ToSetLookupAsync(keySelector, x => x);
	}
	
	/// <summary>
	/// Execute the query and turn the result into a HashSet
	/// </summary>
	/// <param name="query">The query</param>
	/// <param name="valueSelector">Mapping of the value</param>
	/// <returns>A hash set with all values</returns>
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
	
	/// <summary>
	/// Execute the query and turn the result into a HashSet
	/// </summary>
	/// <param name="query">The query</param>
	/// <returns>A hash set with all values</returns>
	public static Task<HashSet<TModel>> ToSetAsync<TModel>(this IQueryable<TModel> query)
	{
		return query.ToSetAsync(x => x);
	}
}