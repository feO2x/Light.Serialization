using System;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON Token Parser that deserializes JSON strings in ISO 8601 duration format to .NET TimeSpan values.
    /// </summary>
    public sealed class TimeSpanParser : BaseJsonStringToPrimitiveParser<TimeSpan>, IJsonStringToPrimitiveParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the token is a JSON string and that the requested type is a time span.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            return context.Token.JsonType == JsonTokenType.String && context.RequestedType == typeof(TimeSpan);
        }

        /// <summary>
        ///     Parses the specified JSON string as a .NET TimeSpan value.
        ///     This method must only be called when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            return ParseResult.FromParsedValue(ParseValue(context.Token));
        }

        /// <summary>
        ///     Tries to parse the specified JSON string as a .NET TimeSpan value.
        /// </summary>
        public JsonStringParseResult TryParse(JsonDeserializationContext context, string deserializedString)
        {
            try
            {
                return new JsonStringParseResult(true, new Iso8601DurationToTimeSpanParser().ParseToken(context.Token));
            }
            catch (JsonDocumentException)
            {
                return new JsonStringParseResult(false);
            }
        }

        private static object ParseValue(JsonToken token)
        {
            var parser = new Iso8601DurationToTimeSpanParser();
            return parser.ParseToken(token);
        }
    }
}