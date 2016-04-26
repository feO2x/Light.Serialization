using System;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a struct that can parse JSON strings in ISO 8601 duration format to .NET TimeSpan values.
    /// </summary>
    public struct Iso8601DurationToTimeSpanParser
    {
        private int _days;
        private int _hours;
        private int _minutes;
        private int _seconds;
        private int _milliseconds;
        private int _currentIndex;
        private bool _wasTDesignatorHit;
        private int _indexOfDot;

        /// <summary>
        /// Parses the specified JSON token as a .NET TimeSpan value.
        /// </summary>
        public TimeSpan ParseToken(JsonToken token)
        {
            _currentIndex = 1;
            _wasTDesignatorHit = false;
            _days = _hours = _minutes = _seconds = _milliseconds = _indexOfDot = 0;

            ExpectCharacter('P', token);

            while (_currentIndex < token.Length - 1)
            {
                if (_wasTDesignatorHit == false)
                    CheckForTDesignator(token);

                var startIndex = _currentIndex;
                var designator = ReadUntilDesignator(token);
                var numberOfDigitsToParse = _currentIndex - startIndex - 1;

                if (_wasTDesignatorHit == false ||
                    designator != 'S' ||
                    DoesNumberContainDot(startIndex, numberOfDigitsToParse, token) == false)
                {
                    var number = ReadNumber(numberOfDigitsToParse, token, startIndex);
                    AssignNumberAccordingToDesignator(number, designator, token);
                }
                else
                {
                    _seconds = ReadNumber(_indexOfDot - startIndex, token, startIndex);
                    _milliseconds = ReadNumber(3, token, _indexOfDot + 1);
                }
            }

            try
            {
                return new TimeSpan(_days, _hours, _minutes, _seconds, _milliseconds);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw CreateException(token, ex);
            }
        }

        private bool DoesNumberContainDot(int startIndex, int numberOfDigitsToParse, JsonToken token)
        {
            for (var i = 0; i < numberOfDigitsToParse; i++)
            {
                var currentCharacter = token[startIndex];
                if (currentCharacter == '.')
                {
                    _indexOfDot = startIndex;
                    return true;
                }

                startIndex++;
            }
            return false;
        }

        private void AssignNumberAccordingToDesignator(int number, char designator, JsonToken token)
        {
            switch (designator)
            {
                case 'D':
                    _days = number;
                    break;
                case 'H':
                    if (_wasTDesignatorHit == false)
                        throw CreateException(token);
                    _hours = number;
                    break;
                case 'M':
                    if (_wasTDesignatorHit == false)
                        throw CreateException(token);
                    _minutes = number;
                    break;
                case 'S':
                    if (_wasTDesignatorHit == false)
                        throw CreateException(token);
                    _seconds = number;
                    break;
                default:
                    throw CreateException(token);
            }
        }

        private static int ReadNumber(int expectedNumberOfDigits, JsonToken token, int startIndex)
        {
            var result = 0;
            for (var base10Position = expectedNumberOfDigits; base10Position > 0; base10Position--, startIndex++)
            {
                var digit = GetDigit(token, startIndex);
                result += digit * CalculateBase(base10Position);
            }
            return result;
        }

        private static int CalculateBase(int base10Position)
        {
            if (base10Position == 1)
                return 1;

            var result = 10;
            for (var i = 2; i < base10Position; i++)
            {
                result *= 10;
            }
            return result;
        }

        private char ReadUntilDesignator(JsonToken token)
        {
            while (true)
            {
                if (_currentIndex == token.Length - 1)
                    throw CreateException(token);

                var currentCharacter = token[_currentIndex++];
                if (char.IsDigit(currentCharacter))
                    continue;
                if (IsDesignator(currentCharacter))
                    return currentCharacter;
            }
        }

        private static int GetDigit(JsonToken token, int index)
        {
            var character = token[index];
            if (char.IsDigit(character) == false)
                throw CreateException(token);
            return character - '0';
        }

        private void CheckForTDesignator(JsonToken token)
        {
            var character = token[_currentIndex];
            if (character != 'T')
                return;

            _currentIndex++;
            _wasTDesignatorHit = true;
        }

        private static bool IsDesignator(char character)
        {
            return character == 'D' ||
                   character == 'H' ||
                   character == 'M' ||
                   character == 'S' ||
                   character == 'T';
        }

        private void ExpectCharacter(char character, JsonToken token)
        {
            if (token[_currentIndex++] != character)
                throw CreateException(token);
        }

        private static JsonDocumentException CreateException(JsonToken token, Exception innerException = null)
        {
            return new JsonDocumentException($"The specified token {token} does not represent a valid time span.", token, innerException);
        }
    }
}