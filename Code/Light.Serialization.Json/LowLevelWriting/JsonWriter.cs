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
        public void BeginArray()
        {
            _textWriter.Write(JsonSymbols.BeginOfArray);
            _whitespaceFormatter.NewlineAndIncreaseIndent(this);
        }

        /// <summary>
        ///     Ends a currently open JSON array.
        /// </summary>
        public void EndArray()
        {
            _whitespaceFormatter.NewlineAndDecreaseIndent(this);
            _textWriter.Write(JsonSymbols.EndOfArray);
        }

        /// <summary>
        ///     Starts a complex JSON object.
        /// </summary>
        public void BeginObject()
        {
            _textWriter.Write(JsonSymbols.BeginOfObject);
            _whitespaceFormatter.NewlineAndIncreaseIndent(this);
        }

        /// <summary>
        ///     Ends a currently open complex JSON object.
        /// </summary>
        public void EndObject()
        {
            _whitespaceFormatter.NewlineAndDecreaseIndent(this);
            _textWriter.Write(JsonSymbols.EndOfObject);
        }

        /// <summary>
        ///     Writes the key and a key-value delimiter in a currently open complex JSON object. It is ensured the the JSON string in the document will be surrounded by quotation marks.
        /// </summary>
        /// <param name="key">The key to be written.</param>
        /// <param name="shouldNormalizeKey">
        ///     The boolean indicating whether the specified key should be normalized to the default JSON naming style (lowerCamelCase).
        ///     This option is true by default. Don't use it for key types like GUIDs where normalization would change the value.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="key" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="key" /> contains only whitespace.</exception>
        public void WriteKey(string key, bool shouldNormalizeKey = true)
        {
            key.MustNotBeNullOrWhiteSpace(nameof(key));

            if (shouldNormalizeKey)
                key = _jsonKeyNormalizer.Normalize(key);

            if (key.IsSurroundedByQuotationMarks() == false)
                key = key.SurroundWithQuotationMarks();

            _textWriter.Write(key);
            _textWriter.Write(JsonSymbols.PairDelimiter);
            _whitespaceFormatter.InsertWhitespaceBetweenKeyAndValue(this);
        }

        /// <summary>
        ///     Writers a delimiter after a value in a currently open JSON object or JSON array.
        /// </summary>
        public void WriteDelimiter()
        {
            _textWriter.Write(JsonSymbols.ValueDelimiter);
            _whitespaceFormatter.Newline(this);
        }

        /// <summary>
        ///     Writes the specified string as a raw value.
        /// </summary>
        /// <param name="string">The string to be written to the JSON document.</param>
        public void WritePrimitiveValue(string @string)
        {
            _textWriter.Write(@string);
        }

        /// <summary>
        ///     Writes the specified string as a JSON string. It is ensured the the JSON string in the document will be surrounded by quotation marks.
        /// </summary>
        /// <param name="string">The string to be written.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="string" /> is null.</exception>
        public void WriteString(string @string)
        {
            @string.MustNotBeNull(nameof(@string));

            if (@string[0] == JsonSymbols.StringDelimiter)
            {
                _textWriter.Write(@string);

                if (@string[@string.Length - 1] != JsonSymbols.StringDelimiter)
                    _textWriter.Write(JsonSymbols.StringDelimiter);

                return;
            }

            _textWriter.Write(JsonSymbols.StringDelimiter);
            _textWriter.Write(@string);

            if (@string[@string.Length - 1] != JsonSymbols.StringDelimiter)
                _textWriter.Write(JsonSymbols.StringDelimiter);
        }

        /// <summary>
        ///     Writes null as a value to the JSON document.
        /// </summary>
        public void WriteNull()
        {
            _textWriter.Write(JsonSymbols.Null);
        }
    }
}