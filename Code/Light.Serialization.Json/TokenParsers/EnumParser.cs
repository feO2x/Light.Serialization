using System;
using System.Reflection;
using Light.GuardClauses;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParser" /> that can parse .NET enum values.
    /// </summary>
    public sealed class EnumParser : IJsonTokenParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the specified token is a JSON string and if the requested type is an enum type.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            return context.Token.JsonType == JsonTokenType.String && context.RequestedType.GetTypeInfo().IsEnum;
        }

        /// <summary>
        ///     Deserializes the enum value with the specified deserialization context.
        ///     Please note that you may only call this method if <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        /// <param name="context">The deserialization context of the enum value to be deserialized.</param>
        /// <returns>The deserialized enum value.</returns>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            context.Token.JsonType.MustBe(JsonTokenType.String);

            var @string = context.DeserializeToken<string>(context.Token);

            return ParseResult.FromParsedValue(Enum.Parse(context.RequestedType, @string, true));
        }
    }
}