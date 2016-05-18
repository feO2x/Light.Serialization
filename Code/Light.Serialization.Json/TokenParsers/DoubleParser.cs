using System;
using System.Globalization;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON token parser that can parse JSON numbers to .NET double values.
    /// </summary>
    public sealed class DoubleParser : IJsonTokenParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the specified token can be parsed to a .NET double.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            var token = context.Token;
            var requestedType = context.RequestedType;
            return ((token.JsonType == JsonTokenType.FloatingPointNumber || token.JsonType == JsonTokenType.IntegerNumber || token.JsonType == JsonTokenType.String) && requestedType == typeof(double)) ||
                   (requestedType == typeof(object) || requestedType == typeof(ValueType)) && token.JsonType == JsonTokenType.FloatingPointNumber;
        }

        /// <summary>
        ///     Parses the given JSON token to a .NET double.
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

            throw new DeserializationException($"Cannot deserialize value {doubleString} to a double value.");
        }
    }
}