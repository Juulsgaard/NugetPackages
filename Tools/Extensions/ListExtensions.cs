namespace Juulsgaard.Tools.Extensions;

public static class ListExtensions
{
	private static readonly Random Rng = new();
		
	/// <summary>
	/// Shuffles the list in place
	/// </summary>
	/// <remarks>Uses an insecure random implementation</remarks>
	/// <param name="list">The list to shuffle</param>
	/// <returns>The list</returns>
	public static List<TVal> Shuffle<TVal>(this List<TVal> list)
	{
		var n = list.Count;  
		while (n > 1) {  
			n--;  
			var k = ListExtensions.Rng.Next(n + 1);
			list.Swap(k, n);
		}

		return list;
	}
		
	/// <summary>
	/// Swaps two positions in a list
	/// </summary>
	/// <param name="list"></param>
	/// <param name="i"></param>
	/// <param name="j"></param>
	/// <returns>The list</returns>
	public static List<TVal> Swap<TVal>(this List<TVal> list, int i, int j)
	{
		(list[i], list[j]) = (list[j], list[i]);
		return list;
	}
		
	/// <summary>
	/// Get a random value from a list
	/// </summary>
	/// <remarks>Uses an insecure random implementation</remarks>
	/// <param name="list">The list</param>
	/// <returns>A random value from the list</returns>
	/// <exception cref="ArgumentException">Thrown an exception if <paramref name="list"/> is empty</exception>
	public static TVal GetRandom<TVal>(this IReadOnlyList<TVal> list)
	{
		if (list.Count <= 0) throw new ArgumentException("Cannot get random element from an empty list", nameof(list));
		return list[ListExtensions.Rng.Next(list.Count)];
	}
	
	/// <summary>
	/// Get a list of random values from a list
	/// </summary>
	/// <param name="list">The list to get values from</param>
	/// <param name="elements">The amount of values to get</param>
	/// <returns>A list of random values</returns>
	public static List<TVal> GetRandom<TVal>(this IEnumerable<TVal> list, int elements)
	{
		var mutableList = list.ToList();
		if (mutableList.Count <= elements) return mutableList.Shuffle();
		
		var output = new List<TVal>();
		
		for (var i = 0; i < elements; i++) {
			if (mutableList.Count <= 0) break;
			var index = ListExtensions.Rng.Next(mutableList.Count);
			output.Add(mutableList[index]);
			mutableList.RemoveAt(index);
		}

		return output;
	}
}