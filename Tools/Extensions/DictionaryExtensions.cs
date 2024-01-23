using System.Collections;

namespace Tools.Extensions;

public static class DictionaryExtensions
{
    public static object? ReadValueOrDefault<TKey>(this IDictionary collection, TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        return !collection.Contains(key) ? default : collection[key];
    }
}