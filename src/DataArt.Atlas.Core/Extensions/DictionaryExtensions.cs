using System.Collections.Generic;

namespace DataArt.Atlas.Core.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Returns value from dictionary or specified default value if dictionary doesn't contain specified key.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <param name="dictionary">Target dictionary.</param>
        /// <param name="key">Target key in dictionary.</param>
        /// <param name="defaultValue">Default value if specified key doesn't exist in dictionary.</param>
        /// <returns>Value by specified key or default value.</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}
