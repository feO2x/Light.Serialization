using System;
using Light.GuardClauses;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents a factory that creates SingleBufferJsonReader instances from the specified JSON document (as a string).
    /// </summary>
    public sealed class SingleBufferJsonReaderFactory : IJsonReaderFactory
    {
        /// <summary>
        ///     Creates a SingleBufferJsonReader instance from the specified JSON document.
        /// </summary>
        /// <param name="json">The JSON document as a string.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json" /> is null.</exception>
        public IJsonReader CreateFromString(string json)
        {
            json.MustNotBeNull(nameof(json));

            return new SingleBufferJsonReader(json.ToCharArray());
        }
    }
}