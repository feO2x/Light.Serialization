using System;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Represents the abstraction for an object that normalizes keys used in complex JSON objects.
    /// </summary>
    public interface IJsonKeyNormalizer
    {
        /// <summary>
        ///     Normalizes the specified key.
        /// </summary>
        /// <param name="key">The key to be normalized.</param>
        /// <returns>The normalized key.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="key" /> is an empty string.</exception>
        string Normalize(string key);
    }
}