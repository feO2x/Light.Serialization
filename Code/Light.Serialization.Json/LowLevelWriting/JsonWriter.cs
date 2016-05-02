using System;
using System.IO;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Represents the default JsonWriter inmplementation.
    /// </summary>
    public sealed class JsonWriter : IJsonWriter
    {
        private readonly IJsonKeyNormalizer _jsonKeyNormalizer;
        private readonly TextWriter _textWriter;
        private readonly IJsonWhitespaceFormatter _whitespaceFormatter;

        /// <summary>
        ///     Creates a new JsonWriter instance.
        /// </summary>
        /// <param name="textWriter">The text writer that will be written to.</param>
        /// <param name="whitespaceFormatter">The formatter that controls whitespace and indenting of the JSON document.</param>
        /// <param name="jsonKeyNormalizer">The object that normalizes the keys in a complex JSON object.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public JsonWriter(TextWriter textWriter, IJsonWhitespaceFormatter whitespaceFormatter, IJsonKeyNormalizer jsonKeyNormalizer)
        {
            textWriter.MustNotBeNull(nameof(textWriter));
            whitespaceFormatter.MustNotBeNull(nameof(whitespaceFormatter));
            jsonKeyNormalizer.MustNotBeNull(nameof(jsonKeyNormalizer));

            _textWriter = textWriter;
            _whitespaceFormatter = whitespaceFormatter;
            _jsonKeyNormalizer = jsonKeyNormalizer;
        }

        /// <summary>
        ///     Starts a new JSON array.
        /// </summary>
        /// <returns>The JSON writer for method chaining.</returns>
        public IJsonWriter BeginArray()
        {
            _textWriter.Write(JsonSymbols.BeginOfArray);
            _whitespaceFormatter.NewlineAndIncreaseIndent(this);
            return this;
        }

        /// <summary>
        ///     Ends a currently open JSON array.
        /// </summary>
        /// <returns>The JSON writer for method chaining.</returns>
        public IJsonWriter EndArray()
        {
            _whitespaceFormatter.NewlineAndDecreaseIndent(this);
            _textWriter.Write(JsonSymbols.EndOfArray);
            return this;
        }

        /// <summary>
        ///     Starts a complex JSON object.
        /// </summary>
        /// <returns>The JSON writer for method chaining.</returns>
        public IJsonWriter BeginObject()
        {
            _textWriter.Write(JsonSymbols.BeginOfObject);
            _whitespaceFormatter.NewlineAndIncreaseIndent(this);
            return this;
        }

        /// <summary>
        ///     Ends a currently open complex JSON object.
        /// </summary>
        /// <returns>The JSON writer for method chaining.</returns>
        public IJsonWriter EndObject()
        {
            _whitespaceFormatter.NewlineAndDecreaseIndent(this);
            _textWriter.Write(JsonSymbols.EndOfObject);
            return this;
        }

        /// <summary>
        ///     Writes the key and a key-value delimiter in a currently open complex JSON object. It is ensured the the JSON string in the document will be surrounded by quotation marks.
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
        public IJsonWriter WriteKey(string key, bool shouldNormalizeKey = true)
        {
            key.MustNotBeNullOrWhiteSpace(nameof(key));

            if (shouldNormalizeKey)
                key = _jsonKeyNormalizer.Normalize(key);

            if (key.IsSurroundedByQuotationMarks() == false)
                key = key.SurroundWithQuotationMarks();

            _textWriter.Write(key);
            _textWriter.Write(JsonSymbols.PairDelimiter);
            _whitespaceFormatter.InsertWhitespaceBetweenKeyAndValue(this);

            return this;
        }

        /// <summary>
        ///     Writers a delimiter after a value in a currently open JSON object or JSON array.
        /// </summary>
        /// <returns>The JSON writer for method chaining.</returns>
        public IJsonWriter WriteDelimiter()
        {
            _textWriter.Write(JsonSymbols.ValueDelimiter);
            _whitespaceFormatter.Newline(this);
            return this;
        }

        /// <summary>
        ///     Writes the specified string as a raw value.
        /// </summary>
        /// <param name="string">The string to be written to the JSON document.</param>
        /// <returns>The JSON writer for method chaining.</returns>
        public IJsonWriter WritePrimitiveValue(string @string)
        {
            _textWriter.Write(@string);
            return this;
        }

        /// <summary>
        ///     Writes the specified string as a JSON string. It is ensured the the JSON string in the document will be surrounded by quotation marks.
        /// </summary>
        /// <param name="string">The string to be written.</param>
        /// <returns>The JSON writer for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="string" /> is null.</exception>
        public IJsonWriter WriteString(string @string)
        {
            @string.MustNotBeNull(nameof(@string));

            if (@string == string.Empty)
            {
                _textWriter.Write(JsonSymbols.StringDelimiter);
                _textWriter.Write(JsonSymbols.StringDelimiter);
                return this;
            }

            if (@string[0] == JsonSymbols.StringDelimiter)
            {
                _textWriter.Write(@string);

                if (@string[@string.Length - 1] != JsonSymbols.StringDelimiter)
                    _textWriter.Write(JsonSymbols.StringDelimiter);

                return this;
            }

            _textWriter.Write(JsonSymbols.StringDelimiter);
            _textWriter.Write(@string);

            if (@string[@string.Length - 1] != JsonSymbols.StringDelimiter)
                _textWriter.Write(JsonSymbols.StringDelimiter);

            return this;
        }

        /// <summary>
        ///     Writes null as a value to the JSON document.
        /// </summary>
        /// <returns>The JSON writer for method chaining.</returns>
        public IJsonWriter WriteNull()
        {
            _textWriter.Write(JsonSymbols.Null);
            return this;
        }
    }
}