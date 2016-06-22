using System;
using System.Diagnostics;
using Light.GuardClauses;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParser" /> that can parse JSON boolean values to .NET <see cref="bool" /> instances.
    /// </summary>
    public sealed class BooleanParser : BaseJsonStringToPrimitiveParser<bool>, IJsonStringToPrimitiveParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the given JSON token is either true or false.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            return context.Token.JsonType == JsonTokenType.True || context.Token.JsonType == JsonTokenType.False;
        }

        /// <summary>
        ///     Parses the JSON token with the specified deserialization context as a .NET boolean.
        ///     Please note that you may only call this method if <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        /// <returns>True if the JSON token is a true, else false.</returns>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            CheckTokenTyp(context.Token);
            return ParseResult.FromParsedValue(context.Token.JsonType == JsonTokenType.True);
        }

        /// <summary>
        ///     Tries to parse the specified string as a boolean value.
        /// </summary>
        public JsonStringParseResult TryParse(JsonDeserializationContext context, string deserializedString)
        {
            switch (deserializedString)
            {
                case JsonSymbols.False:
                    return new JsonStringParseResult(true, false);
                case JsonSymbols.True:
                    return new JsonStringParseResult(true, true);
                default:
                    return new JsonStringParseResult(false);
            }
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckTokenTyp(JsonToken token)
        {
            if (token.JsonType == JsonTokenType.True || token.JsonType == JsonTokenType.False)
                return;

            throw new ArgumentException($"You must not call the BooleanParser with a JSON token other than \"true\" or \"false\", but you specified \"{token}\".");
        }
    }
}