using System;
using System.Globalization;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON token parsers that can deserialize JSON numbers to .NET decimal values.
    /// </summary>
    public sealed class DecimalParser : IJsonTokenParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the specified token is either a floating point number, an integer number, or a string and
        ///     that the requested type is the .NET decimal type.
        /// </summary>
        public bool IsSuitableFor(JsonToken token, Type requestedType)
        {
            return (token.JsonType == JsonTokenType.FloatingPointNumber || token.JsonType == JsonTokenType.IntegerNumber || token.JsonType == JsonTokenType.String) && requestedType == typeof(decimal);
        }

        /// <summary>
        ///     Deserializes the given token to a .NET decimal value.
        ///     This method must only be called if <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public object ParseValue(JsonDeserializationContext context)
        {
            var token = context.Token;
            if (token.JsonType == JsonTokenType.String)
                token = token.RemoveOuterQuotationMarks();

            var decimalString = token.ToString();
            decimal result;
            if (decimal.TryParse(decimalString, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                return result;

            throw new DeserializationException($"Cannot deserialize value {decimalString} to a decimal value.");
        }
    }
}