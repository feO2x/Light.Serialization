using System.Globalization;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParser"/> that can deserialize JSON numbers to .NET <see cref="float"/> values.
    /// </summary>
    public sealed class FloatParser : IJsonTokenParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks that the specified JSON token is either a floating point number, an integer number, or a string
        ///     and that the specified requested type is <see cref="float"/>.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            var token = context.Token;
            return (token.JsonType == JsonTokenType.FloatingPointNumber || token.JsonType == JsonTokenType.IntegerNumber || token.JsonType == JsonTokenType.String) && context.RequestedType == typeof(float);
        }

        /// <summary>
        ///     Parses the specified JSON token to a .NET <see cref="float"/> value.
        ///     This method must only be called if <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            var token = context.Token;
            if (token.JsonType == JsonTokenType.String)
                token = token.RemoveOuterQuotationMarks();

            var floatString = token.ToString();
            float result;
            if (float.TryParse(floatString, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result))
                return ParseResult.FromParsedValue(result);

            throw new JsonDocumentException($"Cannot deserialize value {floatString} to a float value.", context.Token);
        }
    }
}