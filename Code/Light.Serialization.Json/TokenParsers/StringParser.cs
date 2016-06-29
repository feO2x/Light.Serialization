using System;
using System.Globalization;
using Light.GuardClauses;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParser" /> that deserializes JSON strings to .NET <see cref="string" /> instances.
    /// </summary>
    public sealed class StringParser : IJsonTokenParser
    {
        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the specified token is a JSON string and the requested type is <see cref="string" />.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            return context.Token.JsonType == JsonTokenType.String && context.RequestedType == typeof(string);
        }

        /// <summary>
        ///     Deserializes the given token to a .NET string.
        ///     This method must only be called when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            return ParseResult.FromParsedValue(ParseValue(context.Token));
        }

        /// <summary>
        ///     Parses the specified JSON string to a .NET string.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the specified token is no JSON string.</exception>
        public string ParseValue(JsonToken token)
        {
            token.JsonType.MustBe(JsonTokenType.String, message: "The specified JSON token is no JSON string.");

            // If the token has only two character, then it is an empty string.
            if (token.Length == 2)
                return string.Empty;

            // The first and the last character in the token are the JSON string delimiters (").
            // Thus the following loop runs from 1 to Length - 1
            for (var i = 1; i < token.Length - 1; i++)
            {
                // Check if the token contains characters that need to be escaped
                var currentCharacter = token[i];
                if (currentCharacter == JsonSymbols.StringEscapeCharacter)
                    return ConvertEscapeSequencesInToken(token, i + 1);
            }
            // If none could be found, then return the inner characters without the surrounding quotation marks
            return token.ToStringWithoutQuotationMarks();
        }

        private static string ConvertEscapeSequencesInToken(JsonToken token, int currentTokenIndex)
        {
            // Calculate how many characters are needed for the new char array.
            // This means that we have to run through the whole token
            // and check how many single escape sequences and hexadecimal
            // escape sequences can be found.
            var numberOfSingleEscapeSequences = 0;
            var numberOfHexadecimalEscapeSequences = 0;
            var isPreviousCharacterTheStringEscapeCharacter = true;
            char currentCharacter;

            while (currentTokenIndex < token.Length - 1)
            {
                currentCharacter = token[currentTokenIndex++];
                // Check if the current character is part of an escape sequence
                if (isPreviousCharacterTheStringEscapeCharacter)
                {
                    // If it is a hexadecimal character then set the index after the escape sequence
                    if (currentCharacter == JsonSymbols.HexadecimalEscapeIndicator)
                    {
                        currentTokenIndex += 4;
                        numberOfHexadecimalEscapeSequences++;
                    }
                    // else it can only be a single character escape sequence
                    else
                    {
                        numberOfSingleEscapeSequences++;
                    }
                    isPreviousCharacterTheStringEscapeCharacter = false;
                    continue;
                }
                // Else check if this character is the beginning of an escape sequence
                if (currentCharacter == JsonSymbols.StringEscapeCharacter)
                    isPreviousCharacterTheStringEscapeCharacter = true;
            }

            // Calculate the actual number of characters that we need for the string
            var numberOfCharacters = token.Length - 2 - numberOfSingleEscapeSequences - numberOfHexadecimalEscapeSequences * 5;
            var characterArray = new char[numberOfCharacters];
            currentTokenIndex = 1; // Start copying from the first character after the initial JSON string delimiter

            // Fill the character array, escape where necessary
            for (var i = 0; i < numberOfCharacters; i++)
            {
                currentCharacter = token[currentTokenIndex++];
                // Check if the current character marks an escape sequence; if yes, transform it into a single character
                if (currentCharacter == JsonSymbols.StringEscapeCharacter)
                {
                    characterArray[i] = ReadEscapeSequence(token, ref currentTokenIndex);
                    continue;
                }
                // If it is not a escape sequence, just copy the character
                characterArray[i] = currentCharacter;
            }

            return new string(characterArray);
        }

        private static char ReadEscapeSequence(JsonToken token, ref int currentBufferIndex)
        {
            var currentCharacter = token[currentBufferIndex++];
            // Check if the second character in the escape sequence indicates a hexadecimal escape sequence
            if (currentCharacter == JsonSymbols.HexadecimalEscapeIndicator)
                return ReadHexadecimalEscapeSequence(token, ref currentBufferIndex);

            // If not, then the escape sequence must be one with a single character
            foreach (var singleEscapedCharacter in JsonSymbols.SingleEscapedCharacters)
            {
                if (currentCharacter == singleEscapedCharacter.ValueAfterEscapeCharacter)
                    return singleEscapedCharacter.EscapedCharacter;
            }

            throw new DeserializationException("This exception should never be thrown because the foreach loop above will find exactly one single escape character that fits. However, if you see this exception message nontheless, then please report a bug on Github: https://github.com/feO2x/Light.Serialization.");
        }

        private static char ReadHexadecimalEscapeSequence(JsonToken token, ref int currentBufferIndex)
        {
            var hexadecimalDigitsAsString = token.ToString(currentBufferIndex, 4);
            currentBufferIndex += 4; // Set the current index to the first character after the hexadecimal escape sequence
            return Convert.ToChar(int.Parse(hexadecimalDigitsAsString, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo));
        }
    }
}