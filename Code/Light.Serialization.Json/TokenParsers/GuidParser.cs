using System;
using Light.GuardClauses;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParser" /> that deserializes JSON strings to .NET <see cref="Guid" /> instances.
    /// </summary>
    public sealed class GuidParser : BaseJsonStringToPrimitiveParser<Guid>, IJsonStringToPrimitiveParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the specified token is a JSON string and if the requested type
        ///     is the .NET <see cref="Guid" /> type.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            return context.Token.JsonType == JsonTokenType.String && context.RequestedType == typeof(Guid);
        }

        /// <summary>
        ///     Parses the specified JSON string as a <see cref="Guid" /> value.
        ///     This method must only be called when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            context.Token.JsonType.MustBe(JsonTokenType.String);

            var token = context.Token;
            var guidString = token.ToStringWithoutQuotationMarks();

            Guid parsedGuid;
            if (Guid.TryParse(guidString, out parsedGuid) == false)
                throw new JsonDocumentException($"Could not deserialize token {token} to a valid GUID.", token);

            return ParseResult.FromParsedValue(parsedGuid);
        }

        /// <summary>
        ///     Tries to parse the specified JSON token as a <see cref="Guid" />.
        /// </summary>
        public JsonStringParseResult TryParse(JsonDeserializationContext context, string deserializedString)
        {
            Guid parsedGuid;
            return Guid.TryParse(deserializedString, out parsedGuid) == false ? new JsonStringParseResult(false) : new JsonStringParseResult(true, parsedGuid);
        }
    }
}