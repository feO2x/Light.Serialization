using System;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Represents the abstraction of an object that provides a low level API for writing a JSON document.
    /// </summary>
    public interface IJsonWriter
    {
        /// <summary>
        ///     Starts a new JSON array.
        /// </summary>
        /// <returns>The JSON writer for method chaining.</returns>
        IJsonWriter BeginArray();

        /// <summary>
        ///     Ends a currently open JSON array.
        /// </summary>
        /// <returns>The JSON writer for method chaining.</returns>
        IJsonWriter EndArray();

        /// <summary>
        ///     Starts a complex JSON object.
        /// </summary>
        /// <returns>The JSON writer for method chaining.</returns>
        IJsonWriter BeginObject();

        /// <summary>
        ///     Ends a currently open complex JSON object.
        /// </summary>
        /// <returns>The JSON writer for method chaining.</returns>
        IJsonWriter EndObject();

        /// <summary>
        ///     Writes the key and a key-value delimiter in a currently open complex JSON object. It is ensured that the key is surrounded by quotation marks.
        /// </summary>
        /// <param name="key">The key to be written.</param>
        /// <param name="shouldNormalizeKey">
        ///     The boolean indicating whether the specified key should be normalized to the default JSON naming style (lowerCamelCase).
        ///     This option is true by default. Don't use it for key types like GUIDs where normalization would change the value.
        /// </param>
        /// <returns>The JSON writer for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="key" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="key" /> contains only whitespace.</exception>
        IJsonWriter WriteKey(string key, bool shouldNormalizeKey = true);

        /// <summary>
        ///     Writers a delimiter after a value in a currently open JSON object or JSON array.
        /// </summary>
        /// <returns>The JSON writer for method chaining.</returns>
        IJsonWriter WriteDelimiter();

        /// <summary>
        ///     Writes the specified string as a raw value.
        /// </summary>
        /// <param name="string">The string to be written to the JSON document.</param>
        /// <returns>The JSON writer for method chaining.</returns>
        IJsonWriter WritePrimitiveValue(string @string);

        /// <summary>
        ///     Writes the specified string as a JSON string. It is ensured the the JSON string in the document will be surrounded by quotation marks.
        /// </summary>
        /// <param name="string">The string to be written.</param>
        /// <returns>The JSON writer for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="string" /> is null.</exception>
        IJsonWriter WriteString(string @string);

        /// <summary>
        ///     Writes null as a value to the JSON document.
        /// </summary>
        /// <returns>The JSON writer for method chaining.</returns>
        IJsonWriter WriteNull();
    }
}