using Light.GuardClauses;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents a JSON reader that creates JSON tokens from a character buffer.
    /// </summary>
    public sealed class SingleBufferJsonReader : IJsonReader
    {
        private readonly char[] _buffer;
        private int _currentIndex;

        /// <summary>
        ///     Creates a new instance of <see cref="SingleBufferJsonReader" />.
        /// </summary>
        /// <param name="buffer">The buffer to be read from.</param>
        public SingleBufferJsonReader(char[] buffer)
        {
            buffer.MustNotBeNull(nameof(buffer));

            _buffer = buffer;
        }

        /// <summary>
        ///     Reads the next token of the underlying character buffer.
        /// </summary>
        /// <returns>The next JSON token in the document.</returns>
        public JsonToken ReadNextToken()
        {
            var wasEndOfJsonDocumentReached = IgnoreWhiteSpace();

            if (wasEndOfJsonDocumentReached)
                return CreateToken(_currentIndex, JsonTokenType.EndOfDocument);

            var firstCharacter = _buffer[_currentIndex];

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

            var startIndex = _currentIndex;
            ReadToEndOfToken();
            var token = CreateToken(startIndex, JsonTokenType.Error);
            throw new JsonDocumentException($"The Json Reader cannot recognize the sequence {token} and therefore cannot tranlate it to a valid JsonToken", token);
        }

        private bool IgnoreWhiteSpace()
        {
            while (true)
            {
                if (_currentIndex == _buffer.Length)
                    return true;

                if (char.IsWhiteSpace(_buffer[_currentIndex]) == false)
                    return false;

                _currentIndex++;
            }
        }

        private JsonToken ReadPositiveNumber(char firstCharacter)
        {
            var startIndex = _currentIndex;
            var tokenType = CheckNumber(firstCharacter);
            if (tokenType == JsonTokenType.Error)
                throw ReadToEndOfTokenAndCreateJsonDocumentException(startIndex, JsonSymbols.Number);

            return CreateToken(startIndex, tokenType);
        }

        private JsonToken ReadNegativeNumber()
        {
            // The first character is definitely a negative sign, otherwise this method would not have been called
            var startIndex = _currentIndex;

            // Advance the current index and check if it is a digit
            _currentIndex++;
            if (IsEndOfToken())
                throw CreateJsonDocumentException(startIndex, JsonSymbols.Number);

            var currentCharacter = _buffer[_currentIndex];
            // If there is no number, then the document is not formatted properly
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
                _currentIndex++;
                if (IsEndOfToken())
                    return JsonTokenType.IntegerNumber;

                // Else check if there's a decimal part
                var currentCharacter = _buffer[_currentIndex];
                return CheckDecimalPart(currentCharacter);
            }

            // Else the number starts with a digit other than zero
            while (true)
            {
                _currentIndex++;
                // If the number ends now, it's definitely an integer number
                if (IsEndOfToken())
                    return JsonTokenType.IntegerNumber;

                var currentCharacter = _buffer[_currentIndex];
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
            _currentIndex++;
            if (IsEndOfToken() || char.IsDigit(_buffer[_currentIndex]) == false)
                return JsonTokenType.Error;

            // Else read in as much digits as possible
            while (true)
            {
                _currentIndex++;

                // If the token ends now, then the number is a correct floating point number
                if (IsEndOfToken())
                    return JsonTokenType.FloatingPointNumber;

                currentCharacter = _buffer[_currentIndex];
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
            _currentIndex++;
            if (IsEndOfToken())
                return JsonTokenType.Error;

            currentCharacter = _buffer[_currentIndex];
            if (currentCharacter == JsonSymbols.Plus || currentCharacter == JsonSymbols.Minus)
            {
                _currentIndex++;
                if (IsEndOfToken())
                    return JsonTokenType.Error;

                currentCharacter = _buffer[_currentIndex];
            }

            // There must be at least one digit after the exponential symbol (or sign symbol)
            if (char.IsDigit(currentCharacter) == false)
                return JsonTokenType.Error;

            // Read in as much digits as possible
            while (true)
            {
                _currentIndex++;

                if (IsEndOfToken())
                    return JsonTokenType.FloatingPointNumber;

                currentCharacter = _buffer[_currentIndex];
                if (char.IsDigit(currentCharacter))
                    continue;

                return JsonTokenType.Error;
            }
        }

        private JsonToken ReadString()
        {
            // The first digit is a string delimiter, otherwise this method would not have been called
            var startIndex = _currentIndex;

            // Read in all following characters until we get a valid string delimiter that ends the JSON string
            var isPreviousCharacterEscapeCharacter = false;
            while (true)
            {
                CheckNextCharacter:
                _currentIndex++;
                // A string must end with a string delimiter, if the buffer ends now, then the string is erroneous
                if (IsEndOfBuffer())
                    throw CreateJsonDocumentException(startIndex, JsonSymbols.String);

                var currentCharacter = _buffer[_currentIndex];

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
                _currentIndex++;

                if (IsEndOfToken())
                    return false;

                if (_buffer[_currentIndex].IsHexadecimal() == false)
                    return false;
            }

            return true;
        }

        private bool IsEndOfToken()
        {
            if (_currentIndex == _buffer.Length)
                return true;
            var currentCharacter = _buffer[_currentIndex];
            return char.IsWhiteSpace(currentCharacter) ||
                   currentCharacter == JsonSymbols.ValueDelimiter ||
                   currentCharacter == JsonSymbols.EndOfArray ||
                   currentCharacter == JsonSymbols.EndOfObject ||
                   currentCharacter == JsonSymbols.PairDelimiter;
        }

        private bool IsEndOfBuffer()
        {
            return _currentIndex == _buffer.Length;
        }

        private JsonToken ReadConstantToken(string expectedToken, JsonTokenType tokenType)
        {
            var startIndex = _currentIndex;
            for (var i = 1; i < expectedToken.Length; i++)
            {
                _currentIndex++;
                if (_currentIndex == _buffer.Length)
                    throw CreateJsonDocumentException(startIndex, expectedToken);

                if (_buffer[_currentIndex] == expectedToken[i])
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
                _currentIndex++;
            }
        }

        private JsonToken ReadSingleCharacterAndCreateToken(JsonTokenType tokenType)
        {
            return CreateToken(_currentIndex++, tokenType);
        }

        private JsonToken ReadSingleCharacterAndCreateToken(int tokenStartIndex, JsonTokenType tokenType)
        {
            _currentIndex++;
            return CreateToken(tokenStartIndex, tokenType);
        }

        private JsonToken CreateToken(int startIndex, JsonTokenType tokenType)
        {
            return new JsonToken(_buffer, startIndex, _currentIndex - startIndex, tokenType);
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
                _currentIndex++;
                if (IsEndOfBuffer())
                    break;
                if (_buffer[_currentIndex] != JsonSymbols.StringDelimiter)
                    continue;

                _currentIndex++;
                break;
            }

            return CreateJsonDocumentException(tokenStartIndex, JsonSymbols.String);
        }
    }
}