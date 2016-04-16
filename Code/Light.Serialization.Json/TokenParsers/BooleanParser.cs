using System;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON token parser that can parse boolean value.
    /// </summary>
    public sealed class BooleanParser : IJsonTokenParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the given JSON token is either true or false.
        /// </summary>
        /// <param name="token">The JSON token to be checked.</param>
        /// <param name="requestedType">The requested type of the </param>
        /// <returns></returns>
        public bool IsSuitableFor(JsonToken token, Type requestedType)
        {
            return token.JsonType == JsonTokenType.True || token.JsonType == JsonTokenType.False;
        }

        /// <summary>
        ///     Parses the JSON token with the specified deserialization context as a .NET boolean.
        ///     Please note that you may only call this method if <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        /// <param name="context">The deserialization context of the specified JSON token.</param>
        /// <returns>True if the JSON token is a true, else false.</returns>
        public object ParseValue(JsonDeserializationContext context)
        {
            return context.Token.JsonType == JsonTokenType.True;
        }
    }
}