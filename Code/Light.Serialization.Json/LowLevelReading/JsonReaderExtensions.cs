using System;
using Light.GuardClauses;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Provides extension methods for the <see cref="IJsonReader" /> interface.
    /// </summary>
    public static class JsonReaderExtensions
    {
        /// <summary>
        ///     Reads the next JSON token from the document and expects it to be a delimiter (,) after a value in a JSON array or complex JSON object.
        /// </summary>
        /// <param name="reader">The reader to be used.</param>
        /// <param name="exceptionMessage">The message that should be injected into the resulting exception (optional).</param>
        /// <returns>The reader for method chaining.</returns>
        /// <exception cref="JsonDocumentException">Thrown when the next token is no value delimiter.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reader" /> is null.</exception>
        public static IJsonReader ReadAndExpectValueDelimiterToken(this IJsonReader reader, string exceptionMessage = null)
        {
            reader.MustNotBeNull(nameof(reader));

            var token = reader.ReadNextToken();
            if (token.JsonType != JsonTokenType.ValueDelimiter)
                throw new JsonDocumentException(exceptionMessage ?? $"Expected value delimiter token in JSON document, but found {token}.", token);

            return reader;
        }

        /// <summary>
        ///     Reads the next JSON token and expects it to be either a value delimiter, or an end of object symbol.
        /// </summary>
        /// <param name="reader">The reader to be used.</param>
        /// <param name="exceptionMessage">The message that should be injected into the resulting exception (optional).</param>
        /// <returns>The token type of the read token.</returns>
        /// <exception cref="JsonDocumentException">Thrown when the next token is no value delimiter or end of object symbol.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reader" /> is null.</exception>
        public static JsonTokenType ReadAndExpectEndOfObjectOrValueDelimiter(this IJsonReader reader, string exceptionMessage = null)
        {
            reader.MustNotBeNull(nameof(reader));

            var token = reader.ReadNextToken();

            switch (token.JsonType)
            {
                case JsonTokenType.EndOfObject:
                    return JsonTokenType.EndOfObject;
                case JsonTokenType.ValueDelimiter:
                    return JsonTokenType.ValueDelimiter;
                default:
                    throw new JsonDocumentException(exceptionMessage ?? $"Expected value delimiter or end of complex JSON object, but found {token}.", token);
            }
        }

        /// <summary>
        ///     Reads the next JSON token and expects it to be a pair delimiter symbol between a key-value pair in a complex JSON object.
        /// </summary>
        /// <param name="reader">The reader to be used.</param>
        /// <param name="exceptionMessage">The message that should be injected into the resulting exception (optional).</param>
        /// <returns>The reader for method chaining.</returns>
        /// <exception cref="JsonDocumentException">Thrown when the next token is no pair delimiter symbol.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reader" /> is null.</exception>
        public static IJsonReader ReadAndExpectPairDelimiterToken(this IJsonReader reader, string exceptionMessage = null)
        {
            var token = reader.ReadNextToken();
            if (token.JsonType != JsonTokenType.PairDelimiter)
                throw new JsonDocumentException(exceptionMessage ?? $"Expected delimiter between label and value in complex JSON object, but found {token}.", token);

            return reader;
        }

        /// <summary>
        ///     Reads the next JSON token and expects it to be a begin of array symbol.
        /// </summary>
        /// <param name="reader">The reader to be used.</param>
        /// <param name="exceptionMessage">The message that should be injected into the resulting exception (optional).</param>
        /// <returns>The reader for method chaining.</returns>
        /// <exception cref="JsonDocumentException">Thrown when the next token is no begin of array symbol.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reader" /> is null.</exception>
        public static IJsonReader ReadAndExpectBeginOfArray(this IJsonReader reader, string exceptionMessage = null)
        {
            var token = reader.ReadNextToken();
            if (token.JsonType != JsonTokenType.BeginOfArray)
                throw new JsonDocumentException(exceptionMessage ?? $"Expected begin of JSON array, but found {token}.", token);

            return reader;
        }

        /// <summary>
        ///     Reads the next JSON token and expects it to be an end of array symbol.
        /// </summary>
        /// <param name="reader">The reader to be used.</param>
        /// <param name="exceptionMessage">The message that should be injected into the resulting exception (optional).</param>
        /// <returns>The reader for method chaining.</returns>
        /// <exception cref="JsonDocumentException">Thrown when the next token is no end of array symbol.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reader" /> is null.</exception>
        public static IJsonReader ReadAndExpectedEndOfArray(this IJsonReader reader, string exceptionMessage = null)
        {
            var token = reader.ReadNextToken();
            if (token.JsonType != JsonTokenType.EndOfArray)
                throw new JsonDocumentException(exceptionMessage ?? $"Expected end of JSON array, but found {token}.", token);

            return reader;
        }

        /// <summary>
        ///     Reads the next JSON token and expects it to be an end of object symbol.
        /// </summary>
        /// <param name="reader">The reader to be used.</param>
        /// <param name="exceptionMessage">The message that should be injected into the resulting exception (optional).</param>
        /// <returns>The reader for method chaining.</returns>
        /// <exception cref="JsonDocumentException">Thrown when the next token is no end of object symbol.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reader" /> is null.</exception>
        public static IJsonReader ReadAndExpectEndOfObject(this IJsonReader reader, string exceptionMessage = null)
        {
            var token = reader.ReadNextToken();
            if (token.JsonType != JsonTokenType.EndOfObject)
                throw new JsonDocumentException(exceptionMessage ?? $"Expected end of JSON object, but found {token}.", token);

            return reader;
        }

        /// <summary>
        ///     Reads the next JSON token and expects it to be a JSON number.
        /// </summary>
        /// <param name="reader">The reader to be used.</param>
        /// <param name="exceptionMessage">The message that should be injected into the resulting exception (optional).</param>
        /// <returns>The token that was read.</returns>
        /// <exception cref="JsonDocumentException">Thrown when the next token is no JSON number.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reader" /> is null.</exception>
        public static JsonToken ReadAndExpectNumber(this IJsonReader reader, string exceptionMessage = null)
        {
            var token = reader.ReadNextToken();
            if (token.JsonType != JsonTokenType.IntegerNumber || token.JsonType == JsonTokenType.FloatingPointNumber)
                throw new JsonDocumentException(exceptionMessage ?? $"Expected JSON number in document, but found {token}.", token);

            return token;
        }
    }
}