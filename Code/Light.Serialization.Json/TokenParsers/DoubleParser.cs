using System;
using System.Globalization;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents <see cref="IJsonTokenParser" /> that can parse JSON numbers to .NET <see cref="double" /> values.
    /// </summary>
    public sealed class DoubleParser : BaseJsonStringToPrimitiveParser<double>, IJsonStringToPrimitiveParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the specified token can be parsed to a .NET <see cref="double" /> value.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            var token = context.Token;
            var requestedType = context.RequestedType;
            return ((token.JsonType == JsonTokenType.FloatingPointNumber || token.JsonType == JsonTokenType.IntegerNumber || token.JsonType == JsonTokenType.String) && requestedType == typeof(double)) ||
                   (requestedType == typeof(object) || requestedType == typeof(ValueType)) && token.JsonType == JsonTokenType.FloatingPointNumber;
        }

        /// <summary>
        ///     Parses the given JSON token to a .NET <see cref="double" /> value.
        ///     Please note that you may only call this method if <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        /// <param name="context">The deserialization context of the specified JSON number.</param>
        /// <returns>The deserialized double value.</returns>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            var token = context.Token;
            if (token.JsonType == JsonTokenType.String)
                token = token.RemoveOuterQuotationMarks();

            var doubleString = token.ToString();
            double result;
            if (double.TryParse(doubleString, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result))
                return ParseResult.FromParsedValue(result);

            throw new JsonDocumentException($"Cannot deserialize value {doubleString} to a double value.", context.Token);
        }

        /// <summary>
        ///     Tries to parse the specified string as a <see cref="double" /> value.
        /// </summary>
        public JsonStringParseResult TryParse(JsonDeserializationContext context, string deserializedString)
        {
            double parsedValue;
            return double.TryParse(deserializedString, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out parsedValue) ? new JsonStringParseResult(true, parsedValue) : new JsonStringParseResult(false);
        }
    }
}