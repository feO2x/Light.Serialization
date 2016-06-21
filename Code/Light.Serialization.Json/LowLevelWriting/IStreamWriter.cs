using System;

namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Represents the abstraction of an object that writes characters and strings to a stream.
    /// </summary>
    public interface IStreamWriter : IDisposable
    {
        /// <summary>
        ///     Writes a single character to the stream.
        /// </summary>
        /// <param name="character">The character to be written.</param>
        /// <returns>The stream writer for method chaining.</returns>
        IStreamWriter Write(char character);

        /// <summary>
        ///     Writes the specified string to the stream.
        /// </summary>
        /// <param name="string">The string to be written.</param>
        /// <returns>The stream writer for method chaining.</returns>
        IStreamWriter Write(string @string);
    }
}