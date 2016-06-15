using System;
using System.IO;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents the abstraction of a factory that can create a JSON reader.
    /// </summary>
    public interface IJsonReaderFactory
    {
        /// <summary>
        ///     Creates a JSON reader instance from the specified JSON document.
        /// </summary>
        /// <param name="json">The JSON document as a string.</param>
        /// <returns>The created JSON reader.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json" /> is null.</exception>
        IJsonReader CreateFromString(string json);

        /// <summary>
        ///     Creates a JSON reader instance from the specified JSON stream.
        /// </summary>
        /// <param name="textReader">The text reader encapsulating the stream that contains the JSON document.</param>
        /// <returns>The created JSON reader.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="textReader" /> is null.</exception>
        IJsonReader CreateFromTextReader(TextReader textReader);
    }
}