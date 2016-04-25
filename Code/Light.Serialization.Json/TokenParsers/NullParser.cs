using System;
using System.Reflection;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON token parser that can parse the JSON null token.
    /// </summary>
    public sealed class NullParser : IJsonTokenParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the specified token is null and if the requested type is a reference or nullable type.
        /// </summary>
        /// <param name="token">The token to be deserialized.</param>
        /// <param name="requestedType">The requested type of the object graph.</param>
        public bool IsSuitableFor(JsonToken token, Type requestedType)
        {
            var typeInfo = requestedType.GetTypeInfo();
            return token.JsonType == JsonTokenType.Null && (typeInfo.IsClass || typeInfo.IsInterface);
        }

        /// <summary>
        ///     Deserializes the specified JSON null value to .NET null. This method must only be called
        ///     when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public object ParseValue(JsonDeserializationContext context)
        {
            return null;
        }
    }
}