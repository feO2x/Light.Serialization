using Light.GuardClauses;

namespace Light.Serialization.Json.LowLevelReading
{
    public static class JsonReaderExtensions
    {
        public static IJsonReader ReadAndExpectValueDelimiterToken(this IJsonReader reader, string exceptionMessage = null)
        {
            reader.MustNotBeNull(nameof(reader));

            var token = reader.ReadNextToken();
            if (token.JsonType != JsonTokenType.ValueDelimiter)
                throw new JsonDocumentException(exceptionMessage ?? $"Expected value delimiter token in JSON document, but found {token}.", token);

            return reader;
        }

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

        public static IJsonReader ReadAndExpectPairDelimiterToken(this IJsonReader reader, string exceptionMessage = null)
        {
            var token = reader.ReadNextToken();
            if (token.JsonType != JsonTokenType.PairDelimiter)
                throw new JsonDocumentException(exceptionMessage ?? $"Expected delimiter between label and value in complex JSON object, but found {token}.", token);

            return reader;
        }

        public static IJsonReader ReadAndExpectBeginOfArray(this IJsonReader reader, string exceptionMessage = null)
        {
            var token = reader.ReadNextToken();
            if (token.JsonType != JsonTokenType.BeginOfArray)
                throw new JsonDocumentException(exceptionMessage ?? $"Expected begin of JSON array, but found {token}.", token);

            return reader;
        }

        public static IJsonReader ReadAndExpectedEndOfArray(this IJsonReader reader, string exceptionMessage = null)
        {
            var token = reader.ReadNextToken();
            if (token.JsonType != JsonTokenType.EndOfArray)
                throw new JsonDocumentException(exceptionMessage ?? $"Expected end of JSON array, but found {token}.", token);

            return reader;
        }

        public static IJsonReader ReadAndExpectEndOfObject(this IJsonReader reader, string exceptionMessage = null)
        {
            var token = reader.ReadNextToken();
            if (token.JsonType != JsonTokenType.EndOfObject)
                throw new JsonDocumentException(exceptionMessage ?? $"Expected end of JSON object, but found {token}.", token);

            return reader;
        }

        public static JsonToken ReadAndExpectNumber(this IJsonReader reader, string exceptionMessage = null)
        {
            var token = reader.ReadNextToken();
            if (token.JsonType != JsonTokenType.IntegerNumber || token.JsonType == JsonTokenType.FloatingPointNumber)
                throw new JsonDocumentException(exceptionMessage ?? $"Expected JSON number in document, but found {token}.", token);

            return token;
        }
    }
}