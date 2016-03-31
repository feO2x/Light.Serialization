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
        void BeginArray();

        /// <summary>
        ///     Ends a currently open JSON array.
        /// </summary>
        void EndArray();

        /// <summary>
        ///     Starts a complex JSON object.
        /// </summary>
        void BeginObject();

        /// <summary>
        ///     Ends a currently open complex JSON object.
        /// </summary>
        void EndObject();

        /// <summary>
        ///     Writes the key and a key-value delimiter in a currently open complex JSON object.
        /// </summary>
        /// <param name="key">The key to be written.</param>
        /// <param name="shouldNormalizeKey">
        ///     The boolean indicating whether the specified key should be normalized to the default JSON naming style (Lower-Camel-Case).
        ///     This option is true by default. Don't use it for key types like GUIDs where normalization would change the value.
        /// </param>
        void WriteKey(string key, bool shouldNormalizeKey = true);

        /// <summary>
        ///     Writers a delimiter after a value in currently open JSON object or JSON array.
        /// </summary>
        void WriteDelimiter();

        /// <summary>
        ///     Writes the specified string as a raw value.
        /// </summary>
        /// <param name="string">The string to be written to the JSON document.</param>
        void WritePrimitiveValue(string @string);

        /// <summary>
        ///     Writes null as a value to the JSON document.
        /// </summary>
        void WriteNull();
    }
}