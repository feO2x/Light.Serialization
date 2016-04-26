using System;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON Token Parser that deserializes JSON strings in ISO 8601 duration format to .NET TimeSpan values.
    /// </summary>
    public sealed class TimeSpanParser : BaseJsonStringToPrimitiveParser<TimeSpan>, IJsonStringToPrimitiveParser
    {
        private readonly Type _timeSpanType = typeof(TimeSpan);

        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the token is a JSON string and that the requested type is a time span.
        /// </summary>
        public bool IsSuitableFor(JsonToken token, Type requestedType)
        {
            return token.JsonType == JsonTokenType.String && requestedType == _timeSpanType;
        }

        /// <summary>
        ///     Parses the specified JSON string as a .NET TimeSpan value.
        ///     This method must only be called when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public object ParseValue(JsonDeserializationContext context)
        {
            return ParseValue(context.Token);
        }

        /// <summary>
        ///     Tries to parse the specified JSON string as a .NET TimeSpan value.
        /// </summary>
        public JsonStringParseResult TryParse(JsonToken token)
        {
            try
            {
                return new JsonStringParseResult(true, ParseValue(token));
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