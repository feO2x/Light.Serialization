using System;
using System.IO;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents the abstraction of a factory that can create <see cref="IJsonReader" /> instances.
    /// </summary>
    public interface IJsonReaderFactory
    {
        /// <summary>
        ///     Creates an <see cref="IJsonReader" /> instance from the specified string containing the JSON document.
        /// </summary>
        /// <param name="json">The JSON document as a string.</param>
        /// <returns>The created JSON reader.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json" /> is null.</exception>
        IJsonReader CreateFromString(string json);

        /// <summary>
        ///     Creates an <see cref="IJsonReader" /> instance from the specified text reader that encapsulated the stream containing the JSON document.
        /// </summary>
        /// <param name="textReader">The text reader encapsulating the stream that contains the JSON document.</param>
        /// <returns>The created JSON reader.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="textReader" /> is null.</exception>
        IJsonReader CreateFromTextReader(TextReader textReader);

        /// <summary>
        ///     Creates an <see cref="IJsonReader" /> instance from the specified binary reader that encapsulates the stream containing the JSON document.
        /// </summary>
        /// <param name="binaryReader">The binary reader encapsulating the stream that contains the JSON document.</param>
        /// <returns>The created JSON reader.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="binaryReader" /> is null.</exception>
        IJsonReader CreateFromBinaryReader(BinaryReader binaryReader);
    }
}