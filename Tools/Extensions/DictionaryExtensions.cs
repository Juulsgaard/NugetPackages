using System.Collections;

namespace Juulsgaard.Tools.Extensions;

public static class DictionaryExtensions
{
    /// <summary>
    /// Try to read a value from an IDictionary.
    /// If no value is found, return default.
    /// </summary>
    /// <param name="collection">The dictionary</param>
    /// <param name="key">The key to use for lookup</param>
    /// <returns>The value or default</returns>
    /// <exception cref="ArgumentNullException">Thrown an exception if <paramref name="key"/> is null</exception>
    public static object? ReadValueOrDefault<TKey>(this IDictionary collection, TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        return !collection.Contains(key) ? default : collection[key];
    }
}