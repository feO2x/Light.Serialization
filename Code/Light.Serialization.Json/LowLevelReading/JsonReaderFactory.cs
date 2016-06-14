using System;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents a factory that creates <see cref="JsonReader" /> instances from the specified JSON document.
    /// </summary>
    public sealed class JsonReaderFactory : IJsonReaderFactory
    {
        /// <summary>
        ///     Creates a <see cref="JsonReader" /> instance from the specified JSON string.
        /// </summary>
        /// <param name="json">The JSON document as a string.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json" /> is null.</exception>
        public IJsonReader CreateFromString(string json)
        {
            return new JsonReader(new StringStream(json));
        }
    }
}