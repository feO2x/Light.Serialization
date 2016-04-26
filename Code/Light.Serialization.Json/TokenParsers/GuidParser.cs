using System;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON token parser that deserializes JSON strings to .NET GUIDs.
    /// </summary>
    public sealed class GuidParser : BaseJsonStringToPrimitiveParser<Guid>, IJsonStringToPrimitiveParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the specified token is a JSON string and if the requested type
        ///     is the .NET Guid type.
        /// </summary>
        public bool IsSuitableFor(JsonToken token, Type requestedType)
        {
            return token.JsonType == JsonTokenType.String && requestedType == typeof(Guid);
        }

        /// <summary>
        ///     Parses the specified JSON string as a GUID.
        ///     This method must only be called when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public object ParseValue(JsonDeserializationContext context)
        {
            var token = context.Token;
            var guidString = token.ToStringWithoutQuotationMarks();

            Guid parsedGuid;
            if (Guid.TryParse(guidString, out parsedGuid) == false)
                throw new JsonDocumentException($"Could not deserialize token {token} to a valid GUID.", token);

            return parsedGuid;
        }

        /// <summary>
        ///     Tries to parse the specified JSON token as a GUID.
        /// </summary>
        public JsonStringParseResult TryParse(JsonToken token)
        {
            var guidString = token.ToStringWithoutQuotationMarks();

            Guid parsedGuid;
            return Guid.TryParse(guidString, out parsedGuid) == false ? new JsonStringParseResult(false) : new JsonStringParseResult(true, parsedGuid);
        }
    }
}