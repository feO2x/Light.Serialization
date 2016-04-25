using System;
using System.Globalization;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON token parser that can deserialize JSON numbers to .NET float values.
    /// </summary>
    public sealed class FloatParser : IJsonTokenParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks that the specified JSON token is either a floating point number, an integer number, or a string
        ///     and that the specified requested type is float.
        /// </summary>
        /// <param name="token">The token to be deserialized.</param>
        /// <param name="requestedType">The requested type of the object graph.</param>
        public bool IsSuitableFor(JsonToken token, Type requestedType)
        {
            return (token.JsonType == JsonTokenType.FloatingPointNumber || token.JsonType == JsonTokenType.IntegerNumber || token.JsonType == JsonTokenType.String) && requestedType == typeof(float);
        }

        /// <summary>
        ///     Parses the specified JSON token to a .NET float value.
        ///     This method must only be called if <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public object ParseValue(JsonDeserializationContext context)
        {
            var token = context.Token;
            if (token.JsonType == JsonTokenType.String)
                token = token.RemoveOuterQuotationMarks();

            var floatString = token.ToString();
            float result;
            if (float.TryParse(floatString, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result))
                return result;

            throw new DeserializationException($"Cannot deserialize value {floatString} to a float value.");
        }
    }
}