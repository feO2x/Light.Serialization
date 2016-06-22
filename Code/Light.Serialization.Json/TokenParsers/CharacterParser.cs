using System;
using System.Globalization;
using Light.GuardClauses;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParser" /> that parses JSON strings to .NET <see cref="char" /> instances.
    /// </summary>
    public sealed class CharacterParser : BaseJsonStringToPrimitiveParser<char>, IJsonStringToPrimitiveParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the specified JSON token is a string and the requested type is the .NET <see cref="char"/> type.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            return context.Token.JsonType == JsonTokenType.String && context.RequestedType == typeof(char);
        }

        /// <summary>
        ///     Parser the given JSON token as a .NET <see cref="char"/> instance.
        ///     Please note that you may only call this method if <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        /// <param name="context">The deserialization context of the specified JSON token.</param>
        /// <returns>The deserialized .NET character.</returns>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            var token = context.Token;
            token.JsonType.MustBe(JsonTokenType.String);

            // Check if the JSON string is an empty string
            if (token.Length == 2)
                throw CreateException(token);

            // The token has at least two elements because it is a JSON string, thus accessing the second one is safe
            var currentCharacter = token[1];
            // If not then check if it is an escape sequence
            if (currentCharacter == JsonSymbols.StringEscapeCharacter)
                return ParseResult.FromParsedValue(ReadEscapeSequence(token));

            // If it's not an escape sequence, the token size must be three to be a single character (e.g. "a")
            if (token.Length != 3)
                throw CreateException(token);
            return ParseResult.FromParsedValue(currentCharacter);
        }

        /// <summary>
        ///     Tries to parse the specified string as a character.
        /// </summary>
        public JsonStringParseResult TryParse(JsonDeserializationContext context, string deserializedString)
        {
            return deserializedString.Length == 1 ? new JsonStringParseResult(true, deserializedString[0]) : new JsonStringParseResult(false);
        }

        private static char ReadEscapeSequence(JsonToken token)
        {
            var currentCharacter = token[2];
            // Check if the current character indicates a hexadecimal escape sequence
            if (currentCharacter == JsonSymbols.HexadecimalEscapeIndicator)
                return ReadHexadimalEscapeSequence(token);

            // If it is not hexadecimal, then it must be a special escape sequence with only a single character
            if (token.Length != 4)
                throw CreateException(token);

            foreach (var singleEscapedCharacter in JsonSymbols.SingleEscapedCharacters)
            {
                if (singleEscapedCharacter.ValueAfterEscapeCharacter == currentCharacter)
                    return singleEscapedCharacter.EscapedCharacter;
            }
            // If no single escape character could be found, throw an exception because the escaped character cannot be read
            throw CreateException(token);
        }

        private static char ReadHexadimalEscapeSequence(JsonToken token)
        {
            if (token.Length != 8)
                throw CreateException(token);

            var hexadecimalDigitsAsString = token.ToString(3, 4);
            return Convert.ToChar(int.Parse(hexadecimalDigitsAsString, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo));
        }

        private static DeserializationException CreateException(JsonToken token)
        {
            return new JsonDocumentException($"Cannot deserialize value {token} to a character.", token);
        }
    }
}