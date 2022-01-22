using System.Collections.Generic;

namespace Coding4fun.DataTools.Analyzers.Extension
{
    public static class DictionaryExtension
    {
        public static TValue? GetValueOrNull<TKey, TValue>(this Dictionary<TKey, TValue> map, TKey key)
        {
            return !map.TryGetValue(key, out var value) ? default : value;
        }

        public static void SetValue(this Dictionary<string, string> map, string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                map.Remove(key);
            }
            else
            {
                map[key] = value!;
            }
        }
    }
}