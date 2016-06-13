using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON token parser that can parse boolean value.
    /// </summary>
    public sealed class BooleanParser : BaseJsonStringToPrimitiveParser<bool>, IJsonStringToPrimitiveParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the given JSON token is either true or false.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            var token = context.Token;
            return token.JsonType == JsonTokenType.True || token.JsonType == JsonTokenType.False;
        }

        /// <summary>
        ///     Parses the JSON token with the specified deserialization context as a .NET boolean.
        ///     Please note that you may only call this method if <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        /// <returns>True if the JSON token is a true, else false.</returns>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            return ParseResult.FromParsedValue(context.Token.JsonType == JsonTokenType.True);
        }

        /// <summary>
        ///     Tries to parse the specified string as a boolean value.
        /// </summary>
        public JsonStringParseResult TryParse(JsonDeserializationContext context, string deserializedString)
        {
            switch (deserializedString)
            {
                case JsonSymbols.False:
                    return new JsonStringParseResult(true, false);
                case JsonSymbols.True:
                    return new JsonStringParseResult(true, true);
                default:
                    return new JsonStringParseResult(false);
            }
        }
    }
}