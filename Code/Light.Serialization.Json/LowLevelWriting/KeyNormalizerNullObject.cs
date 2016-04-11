using System;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Reprensents a JSON key normalizer that does nothing.
    /// </summary>
    public sealed class KeyNormalizerNullObject : IJsonKeyNormalizer
    {
        /// <summary>
        ///     Returns the specified key.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="key" /> is an empty string.</exception>
        public string Normalize(string key)
        {
            key.MustNotBeNullOrEmpty(nameof(key));

            return key;
        }
    }
}