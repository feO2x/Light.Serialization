using System.Globalization;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParser" /> that can deserialize JSON numbers to .NET <see cref="decimal" /> values.
    /// </summary>
    public sealed class DecimalParser : IJsonTokenParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the specified token is either a floating point number, an integer number, or a string and
        ///     that the requested type is the .NET <see cref="decimal" /> type.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            var token = context.Token;
            return (token.JsonType == JsonTokenType.FloatingPointNumber || token.JsonType == JsonTokenType.IntegerNumber || token.JsonType == JsonTokenType.String) && context.RequestedType == typeof(decimal);
        }

        /// <summary>
        ///     Deserializes the given token to a .NET <see cref="decimal" /> value.
        ///     This method must only be called if <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            var token = context.Token;
            if (token.JsonType == JsonTokenType.String)
                token = token.RemoveOuterQuotationMarks();

            var decimalString = token.ToString();
            decimal result;
            if (decimal.TryParse(decimalString, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                return ParseResult.FromParsedValue(result);

            throw new JsonDocumentException($"Cannot deserialize value {decimalString} to a decimal value.", context.Token);
        }
    }
}