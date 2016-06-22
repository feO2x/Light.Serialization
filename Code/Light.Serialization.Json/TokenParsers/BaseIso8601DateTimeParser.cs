using System;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents the base class for <see cref="IJsonTokenParser" /> classes that deserialize ISO 8601 date time values
    ///     (<see cref="DateTimeParser" /> and <see cref="DateTimeOffsetParser" />).
    /// </summary>
    /// <typeparam name="T">The .NET type that corresponds to the ISO 8601 date time.</typeparam>
    public abstract class BaseIso8601DateTimeParser<T> : BaseJsonStringToPrimitiveParser<T>
    {
        /// <summary>
        ///     Checks if the specified token of the context is a JSON string and the requested type is <see cref="T" />.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            return context.Token.JsonType == JsonTokenType.String && context.RequestedType == typeof(T);
        }

        /// <summary>
        ///     Advances the <paramref name="currentIndex" /> by one position and checks that the specified token character is
        ///     the samme as <paramref name="expected" />.
        /// </summary>
        /// <exception cref="JsonDocumentException">Thrown when the character is not equal to <paramref name="expected" />.</exception>
        protected static void ExpectCharacter(char expected, JsonToken token, ref int currentIndex)
        {
            if (token[currentIndex++] != expected)
                throw CreateException(token);
        }

        /// <summary>
        ///     Checks if the <paramref name="tokenLength" /> is equal to the <paramref name="currentIndex" />.
        /// </summary>
        protected static bool IsEndOfToken(int currentIndex, int tokenLength)
        {
            return currentIndex == tokenLength - 1;
        }

        /// <summary>
        ///     Checks if the character at the <paramref name="currentIndex" /> of the <paramref name="token" />
        ///     is a indicator for a ISO 8601 time zone (i.e. a "Z", "+", or "-" character).
        /// </summary>
        protected static bool IsTimeZoneIndicator(JsonToken token, ref int currentIndex)
        {
            var character = token[currentIndex];
            return character == 'Z' || character == '+' || character == '-';
        }

        /// <summary>
        ///     Reads the specified number of characters from the <paramref name="token" /> and expects all of them
        ///     to be digits. The <paramref name="currentIndex" /> is advanced accordingly.
        /// </summary>
        /// <exception cref="JsonDocumentException">Thrown when any of the characters is no digit.</exception>
        protected static int ReadNumber(int expectedNumberOfDigits, JsonToken token, ref int currentIndex)
        {
            var result = 0;
            for (var base10Position = expectedNumberOfDigits; base10Position > 0; base10Position--, currentIndex++)
            {
                var digit = GetDigit(token, currentIndex);
                result += digit * CalculateBase(base10Position);
            }
            return result;
        }

        /// <summary>
        ///     Calculates the position of a digit in a whole base10 number.
        /// </summary>
        protected static int CalculateBase(int base10Position)
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

        /// <summary>
        ///     Gets a single digit from the token at the current index.
        /// </summary>
        /// <exception cref="JsonDocumentException">Thrown when the character is no digit.</exception>
        protected static int GetDigit(JsonToken token, int currentIndex)
        {
            var character = token[currentIndex];
            if (char.IsDigit(character) == false)
                throw CreateException(token);

            return character - '0';
        }

        protected static JsonDocumentException CreateException(JsonToken token, Exception innerException = null)
        {
            return new JsonDocumentException($"The specified token {token} does not represent a valid date time.", token, innerException);
        }
    }
}