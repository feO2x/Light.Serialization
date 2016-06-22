using System;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParser" /> that can handle <see cref="Nullable{T}" /> as the requested type.
    /// </summary>
    public sealed class NullableParser : IJsonTokenParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the actual type is an instance of <see cref="Nullable{T}" />.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            return context.Token.IsBeginOfValue &&
                   context.RequestedType.IsConstructedGenericType &&
                   context.RequestedType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        ///     Deserializes the specified token as null or an instance of the <see cref="Nullable{T}" /> element type.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            if (context.Token.JsonType == JsonTokenType.Null)
                return ParseResult.FromNull();

            var nullableElementType = context.RequestedType.GenericTypeArguments[0];
            return context.DeserializeToken(context.Token, nullableElementType);
        }
    }
}