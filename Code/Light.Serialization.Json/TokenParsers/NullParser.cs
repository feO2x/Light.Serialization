using System;
using System.Reflection;
using Light.GuardClauses;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParser" /> that can parse the JSON null token.
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
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            var typeInfo = context.RequestedType.GetTypeInfo();
            return context.Token.JsonType == JsonTokenType.Null && (typeInfo.IsClass || typeInfo.IsInterface || IsNullableType(typeInfo));
        }

        /// <summary>
        ///     Deserializes the specified JSON null value to .NET null. This method must only be called
        ///     when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            context.Token.JsonType.MustBe(JsonTokenType.Null);

            return ParseResult.FromNull();
        }

        private static bool IsNullableType(TypeInfo typeInfo)
        {
            if (typeInfo.IsValueType == false && typeInfo.IsGenericType == false && typeInfo.IsGenericTypeDefinition)
                return false;

            return typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}