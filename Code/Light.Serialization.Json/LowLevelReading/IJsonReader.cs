using System;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents the abstraction of a low-level forward-only JSON reader.
    /// </summary>
    public interface IJsonReader : IDisposable
    {
        /// <summary>
        ///     Reads the next token in the JSON document.
        /// </summary>
        JsonToken ReadNextToken();
    }
}