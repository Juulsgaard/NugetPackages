namespace Juulsgaard.Tools.Extensions;

public static class EnumerableExtensions
{
	/// <summary>
	/// Creates a dictionary from the IEnumerable, but ignored repeat key values
	/// </summary>
	/// <param name="values">The values to map</param>
	/// <param name="keySelector">A selector to get the key for each value</param>
	/// <param name="valueSelector">A selector to map the value of the entry</param>
	/// <returns></returns>
	public static Dictionary<TKey, TValue> DistinctToDictionary<TModel, TKey, TValue>(
		this IEnumerable<TModel> values,
		Func<TModel, TKey> keySelector,
		Func<TModel, TValue> valueSelector
	) where TKey : notnull
	{
		var dict = new Dictionary<TKey, TValue>();
			
		foreach (var model in values) {
			var key = keySelector(model);
			var value = valueSelector(model);
			dict.TryAdd(key, value);
		}

		return dict;
	}
	
	/// <summary>
	/// Creates a dictionary from the IEnumerable, but ignored repeat key values
	/// </summary>
	/// <param name="values">The values to map</param>
	/// <param name="keySelector">A selector to get the key for each value</param>
	/// <returns></returns>
	public static Dictionary<TKey, TModel> DistinctToDictionary<TModel, TKey>(
		this IEnumerable<TModel> values,
		Func<TModel, TKey> keySelector
	) where TKey : notnull
	{
		return values.DistinctToDictionary(keySelector, x => x);
	}
}