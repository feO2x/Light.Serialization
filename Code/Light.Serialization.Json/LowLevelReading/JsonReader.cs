using System;
using Light.GuardClauses;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents an <see cref="IJsonReader" /> that creates JSON tokens from an <see cref="ICharacterStream" />.
    /// </summary>
    public sealed class JsonReader : IJsonReader
    {
        private readonly ICharacterStream _stream;

        /// <summary>
        ///     Creates a new instance of <see cref="JsonReader" />.
        /// </summary>
        /// <param name="stream">The stream to be read from.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream" /> is null.</exception>
        public JsonReader(ICharacterStream stream)
        {
            stream.MustNotBeNull(nameof(stream));

            _stream = stream;
        }

        /// <summary>
        ///     Reads the next token of the underlying character stream.
        /// </summary>
        /// <returns>The next JSON token in the document.</returns>
        public JsonToken ReadNextToken()
        {
            IgnoreWhiteSpace();

            if (_stream.IsAtEndOfStream)
                return CreateToken(_stream.Position, JsonTokenType.EndOfDocument);

            var firstCharacter = _stream.CurrentCharacter;

            if (char.IsDigit(firstCharacter))
                return ReadPositiveNumber(firstCharacter);
            if (firstCharacter == JsonSymbols.Minus)
                return ReadNegativeNumber();
            if (firstCharacter == JsonSymbols.StringDelimiter)
                return ReadString();
            if (firstCharacter == JsonSymbols.False[0])
                return ReadConstantToken(JsonSymbols.False, JsonTokenType.False);
            if (firstCharacter == JsonSymbols.True[0])
                return ReadConstantToken(JsonSymbols.True, JsonTokenType.True);
            if (firstCharacter == JsonSymbols.Null[0])
                return ReadConstantToken(JsonSymbols.Null, JsonTokenType.Null);
            if (firstCharacter == JsonSymbols.BeginOfArray)
                return ReadSingleCharacterAndCreateToken(JsonTokenType.BeginOfArray);
            if (firstCharacter == JsonSymbols.EndOfArray)
                return ReadSingleCharacterAndCreateToken(JsonTokenType.EndOfArray);
            if (firstCharacter == JsonSymbols.BeginOfObject)
                return ReadSingleCharacterAndCreateToken(JsonTokenType.BeginOfObject);
            if (firstCharacter == JsonSymbols.EndOfObject)
                return ReadSingleCharacterAndCreateToken(JsonTokenType.EndOfObject);
            if (firstCharacter == JsonSymbols.PairDelimiter)
                return ReadSingleCharacterAndCreateToken(JsonTokenType.PairDelimiter);
            if (firstCharacter == JsonSymbols.ValueDelimiter)
                return ReadSingleCharacterAndCreateToken(JsonTokenType.ValueDelimiter);

            var startIndex = _stream.PinPosition();
            ReadToEndOfToken();
            var token = CreateToken(startIndex, JsonTokenType.Error);
            throw new JsonDocumentException($"The Json Reader cannot recognize the sequence {token} and therefore cannot tranlate it to a valid JsonToken", token);
        }

        /// <summary>
        ///     Disposes of the internal stream.
        /// </summary>
        public void Dispose()
        {
            _stream.Dispose();
        }

        private void IgnoreWhiteSpace()
        {
            while (true)
            {
                if (_stream.IsAtEndOfStream)
                    return;

                if (char.IsWhiteSpace(_stream.CurrentCharacter) == false)
                    return;

                _stream.Advance();
            }
        }

        private JsonToken ReadPositiveNumber(char firstCharacter)
        {
            var startIndex = _stream.PinPosition();
            var tokenType = CheckNumber(firstCharacter);
            if (tokenType == JsonTokenType.Error)
                throw ReadToEndOfTokenAndCreateJsonDocumentException(startIndex, JsonSymbols.Number);

            return CreateToken(startIndex, tokenType);
        }

        private JsonToken ReadNegativeNumber()
        {
            // The first character is definitely a negative sign, otherwise this method would not have been called
            var startIndex = _stream.PinPosition();

            // Advance the current index and check if it is a digit, if not, then there is only a minus sign, which results in an exception
            _stream.Advance();
            if (IsEndOfToken())
                throw CreateJsonDocumentException(startIndex, JsonSymbols.Number);

            var currentCharacter = _stream.CurrentCharacter;
            // If there is no digit, then the token is no proper JSON number
            if (char.IsDigit(currentCharacter) == false)
                throw ReadToEndOfTokenAndCreateJsonDocumentException(startIndex, JsonSymbols.Number);

            var tokenType = CheckNumber(currentCharacter);
            if (tokenType == JsonTokenType.Error)
                throw ReadToEndOfTokenAndCreateJsonDocumentException(startIndex, JsonSymbols.Number);

            return CreateToken(startIndex, tokenType);
        }

        private JsonTokenType CheckNumber(char firstCharacter)
        {
            // Check if the first character is zero
            if (firstCharacter == '0')
            {
                // If the number ends after the zero, then return an integer type
                _stream.Advance();
                if (IsEndOfToken())
                    return JsonTokenType.IntegerNumber;

                // Else check if there's a decimal part
                var currentCharacter = _stream.CurrentCharacter;
                return CheckDecimalPart(currentCharacter);
            }

            // Else the number starts with a digit other than zero
            while (true)
            {
                _stream.Advance();
                // If the number ends now, it's definitely an integer number
                if (IsEndOfToken())
                    return JsonTokenType.IntegerNumber;

                var currentCharacter = _stream.CurrentCharacter;
                // If the current character is a digit, then continue the loop to check the next character
                if (char.IsDigit(currentCharacter))
                    continue;

                return CheckDecimalPart(currentCharacter);
            }
        }

        private JsonTokenType CheckDecimalPart(char currentCharacter)
        {
            // Check if there's a decimal point
            if (currentCharacter != JsonSymbols.DecimalPoint)
                return CheckExponentialPart(currentCharacter);

            // If yes then there must be at least one digit
            _stream.Advance();
            if (IsEndOfToken() || char.IsDigit(_stream.CurrentCharacter) == false)
                return JsonTokenType.Error;

            // Else read in as much digits as possible
            while (true)
            {
                _stream.Advance();

                // If the token ends now, then the number is a correct floating point number
                if (IsEndOfToken())
                    return JsonTokenType.FloatingPointNumber;

                currentCharacter = _stream.CurrentCharacter;
                // If the current character is a digit, then continue this loop to check the next character
                if (char.IsDigit(currentCharacter))
                    continue;

                // Otherwise check the exponential part of the number
                return CheckExponentialPart(currentCharacter);
            }
        }

        private JsonTokenType CheckExponentialPart(char currentCharacter)
        {
            // The exponential part has to begin with an appropriate sign
            if (currentCharacter.IsExponentialSymbol() == false)
                return JsonTokenType.Error;

            // If it is an appropriate exponential sign, check if the next character is a possible plus or minus sign
            _stream.Advance();
            if (IsEndOfToken())
                return JsonTokenType.Error;

            currentCharacter = _stream.CurrentCharacter;
            if (currentCharacter == JsonSymbols.Plus || currentCharacter == JsonSymbols.Minus)
            {
                _stream.Advance();
                if (IsEndOfToken())
                    return JsonTokenType.Error;

                currentCharacter = _stream.CurrentCharacter;
            }

            // There must be at least one digit after the exponential symbol (or sign symbol)
            if (char.IsDigit(currentCharacter) == false)
                return JsonTokenType.Error;

            // Read in as much digits as possible
            while (true)
            {
                _stream.Advance();

                if (IsEndOfToken())
                    return JsonTokenType.FloatingPointNumber;

                currentCharacter = _stream.CurrentCharacter;
                if (char.IsDigit(currentCharacter))
                    continue;

                return JsonTokenType.Error;
            }
        }

        private JsonToken ReadString()
        {
            // The first digit is a string delimiter, otherwise this method would not have been called
            var startIndex = _stream.PinPosition();

            // Read in all following characters until we get a valid string delimiter that ends the JSON string
            var isPreviousCharacterEscapeCharacter = false;
            while (true)
            {
                CheckNextCharacter:
                _stream.Advance();
                // A string must end with a string delimiter, if the buffer ends now, then the string is erroneous
                if (_stream.IsAtEndOfStream)
                    throw CreateJsonDocumentException(startIndex, JsonSymbols.String);

                var currentCharacter = _stream.CurrentCharacter;

                // Check if the previous character was an escape character
                if (isPreviousCharacterEscapeCharacter)
                {
                    // If yes then check if the current character is a specially escaped character that only has one letter
                    foreach (var singleEscapedCharacter in JsonSymbols.SingleEscapedCharacters)
                    {
                        if (singleEscapedCharacter.ValueAfterEscapeCharacter != currentCharacter) continue;

                        isPreviousCharacterEscapeCharacter = false;
                        goto CheckNextCharacter;
                    }

                    // Otherwise check if this is an escape sequence of four hexadecimal digits
                    if (currentCharacter == JsonSymbols.HexadecimalEscapeIndicator)
                    {
                        if (CheckFourHexadecimalDigitsOfJsonEscapeSequence() == false)
                            throw ReadToEndOfStringTokenAndCreateJsonDocumentException(startIndex);

                        isPreviousCharacterEscapeCharacter = false;
                    }
                }

                // If not, then treat this character as a normal one
                else if (currentCharacter == JsonSymbols.StringDelimiter)
                    return ReadSingleCharacterAndCreateToken(startIndex, JsonTokenType.String);

                // Set the boolean value indicating that the next character is part of an escape sequence
                else if (currentCharacter == JsonSymbols.StringEscapeCharacter)
                    isPreviousCharacterEscapeCharacter = true;
            }
        }

        private bool CheckFourHexadecimalDigitsOfJsonEscapeSequence()
        {
            // There must be exactly 4 digits after the u
            for (var i = 0; i < 4; i++)
            {
                _stream.Advance();

                if (IsEndOfToken())
                    return false;

                if (_stream.CurrentCharacter.IsHexadecimal() == false)
                    return false;
            }

            return true;
        }

        private bool IsEndOfToken()
        {
            if (_stream.IsAtEndOfStream)
                return true;
            var currentCharacter = _stream.CurrentCharacter;
            return char.IsWhiteSpace(currentCharacter) ||
                   currentCharacter == JsonSymbols.ValueDelimiter ||
                   currentCharacter == JsonSymbols.EndOfArray ||
                   currentCharacter == JsonSymbols.EndOfObject ||
                   currentCharacter == JsonSymbols.PairDelimiter;
        }

        private JsonToken ReadConstantToken(string expectedToken, JsonTokenType tokenType)
        {
            var startIndex = _stream.PinPosition();
            for (var i = 1; i < expectedToken.Length; i++)
            {
                _stream.Advance();
                if (_stream.IsAtEndOfStream)
                    throw CreateJsonDocumentException(startIndex, expectedToken);

                if (_stream.CurrentCharacter == expectedToken[i])
                    continue;

                throw ReadToEndOfTokenAndCreateJsonDocumentException(startIndex, expectedToken);
            }

            return ReadSingleCharacterAndCreateToken(startIndex, tokenType);
        }

        private void ReadToEndOfToken()
        {
            while (true)
            {
                if (IsEndOfToken())
                    return;
                _stream.Advance();
            }
        }

        private JsonToken ReadSingleCharacterAndCreateToken(JsonTokenType tokenType)
        {
            var startIndex = _stream.PinPosition();
            _stream.Advance();
            return CreateToken(startIndex, tokenType);
        }

        private JsonToken ReadSingleCharacterAndCreateToken(int tokenStartIndex, JsonTokenType tokenType)
        {
            _stream.Advance();
            return CreateToken(tokenStartIndex, tokenType);
        }

        private JsonToken CreateToken(int startIndex, JsonTokenType tokenType)
        {
            var length = _stream.Position > startIndex ? _stream.Position - startIndex :
                             _stream.Buffer.Length - startIndex + _stream.Position;

            return new JsonToken(_stream.Buffer, startIndex, length, tokenType);
        }

        private JsonDocumentException CreateJsonDocumentException(int tokenStartIndex, string expectedJsonType)
        {
            var token = CreateToken(tokenStartIndex, JsonTokenType.Error);
            return new JsonDocumentException($"Cannot deserialize value {token} to {expectedJsonType}.", token);
        }

        private JsonDocumentException ReadToEndOfTokenAndCreateJsonDocumentException(int tokenStartIndex, string expectedJsonType)
        {
            ReadToEndOfToken();
            return CreateJsonDocumentException(tokenStartIndex, expectedJsonType);
        }

        private JsonDocumentException ReadToEndOfStringTokenAndCreateJsonDocumentException(int tokenStartIndex)
        {
            while (true)
            {
                _stream.Advance();
                if (_stream.IsAtEndOfStream)
                    break;
                if (_stream.CurrentCharacter != JsonSymbols.StringDelimiter)
                    continue;

                _stream.Advance();
                break;
            }

            return CreateJsonDocumentException(tokenStartIndex, JsonSymbols.String);
        }
    }
}