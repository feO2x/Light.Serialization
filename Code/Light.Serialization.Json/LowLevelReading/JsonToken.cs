using System;
using Light.GuardClauses;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents a single token from a JSON document.
    /// </summary>
    public struct JsonToken
    {
        private readonly char[] _buffer;
        private readonly int _startIndex;

        /// <summary>
        ///     Gets the length of the JSON token.
        /// </summary>
        public readonly int Length;

        /// <summary>
        ///     Gets the type of the JSON token.
        /// </summary>
        public readonly JsonTokenType JsonType;

        private readonly bool _isCrossingBufferBoundary;

        /// <summary>
        ///     Creates a new instance of <see cref="JsonToken" />.
        /// </summary>
        /// <param name="buffer">The buffer that holds the token.</param>
        /// <param name="startIndex">The beginning of the token.</param>
        /// <param name="length">The length of the token.</param>
        /// <param name="jsonType">The type of the token.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="buffer" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="startIndex" /> is less than zero or greater than the Length of <paramref name="buffer" />,
        ///     or <paramref name="length" /> is less than zero or greater than the length of <paramref name="buffer" />.
        /// </exception>
        public JsonToken(char[] buffer, int startIndex, int length, JsonTokenType jsonType)
        {
            buffer.MustNotBeNull(nameof(buffer));
            startIndex.MustNotBeLessThan(0, nameof(startIndex));
            startIndex.MustNotBeGreaterThanOrEqualTo(buffer.Length, nameof(startIndex));
            length.MustNotBeLessThan(0, nameof(length));
            length.MustNotBeGreaterThan(buffer.Length, nameof(length));

            _buffer = buffer;
            _startIndex = startIndex;
            Length = length;
            JsonType = jsonType;
            _isCrossingBufferBoundary = startIndex + length > buffer.Length;
        }

        /// <summary>
        ///     Gets the character at the specified index of the JSON token.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when index is less than zero or greater than <see cref="Length" />.</exception>
        public char this[int index]
        {
            get
            {
                index.MustNotBeLessThan(0, nameof(index));
                index.MustNotBeGreaterThanOrEqualTo(Length, nameof(index));

                return _buffer[(_startIndex + index) % _buffer.Length];
            }
        }

        /// <summary>
        ///     Return the string representation of the JSON token.
        /// </summary>
        public override string ToString()
        {
            if (_isCrossingBufferBoundary == false)
                return new string(_buffer, _startIndex, Length);

            var characterArray = new char[Length];
            for (var i = 0; i < Length; i++)
            {
                characterArray[i] = this[i];
            }
            return new string(characterArray);
        }

        /// <summary>
        ///     Returns the string representation of the JSON token within the specified boundaries.
        /// </summary>
        /// <param name="startIndex">The start index of the returned string.</param>
        /// <param name="numberOfCharacters">The number of characters of the returned string.</param>
        /// <returns></returns>
        public string ToString(int startIndex, int numberOfCharacters)
        {
            startIndex.MustNotBeLessThan(0, nameof(startIndex));
            numberOfCharacters.MustNotBeLessThan(1, nameof(numberOfCharacters));
            var numberOfCharactersLeft = Length - startIndex;
            numberOfCharacters.MustNotBeGreaterThan(numberOfCharactersLeft, nameof(numberOfCharacters));

            if (_isCrossingBufferBoundary == false)
                return new string(_buffer, _startIndex + startIndex, numberOfCharacters);

            var characterArray = new char[numberOfCharacters];
            for (var i = 0; i < numberOfCharacters; i++)
            {
                characterArray[i] = this[i];
            }
            return new string(characterArray);
        }

        /// <summary>
        ///     Returns the content if the token is a JSON string.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="JsonType" /> is not a JSON string.</exception>
        public string ToStringWithoutQuotationMarks()
        {
            JsonType.MustBe(JsonTokenType.String,
                            exception: () => new InvalidOperationException("ToStringWithoutQuotationMarks should only be called when the JsonType of this token is String."));

            return ToString(1, Length - 2);
        }

        /// <summary>
        ///     Checks if the token represents the beginning of a JSON value.
        /// </summary>
        public bool IsBeginOfValue => JsonType == JsonTokenType.String ||
                                      JsonType == JsonTokenType.True ||
                                      JsonType == JsonTokenType.False ||
                                      JsonType == JsonTokenType.FloatingPointNumber ||
                                      JsonType == JsonTokenType.IntegerNumber ||
                                      JsonType == JsonTokenType.Null ||
                                      JsonType == JsonTokenType.BeginOfArray ||
                                      JsonType == JsonTokenType.BeginOfObject;

        /// <summary>
        ///     Checks if this token is the beginning of a JSON value, else throws a JsonDocumentException.
        /// </summary>
        public void MustBeBeginOfValue()
        {
            if (IsBeginOfValue == false)
                throw new JsonDocumentException($"Expected begin of JSON value, but found {this}.", this);
        }

        /// <summary>
        ///     Returns a new JsonToken that does not contain the outer quotation marks of this JSON string.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when this token is not a JSON string.</exception>
        public JsonToken RemoveOuterQuotationMarks()
        {
            JsonType.MustBe(JsonTokenType.String,
                            exception: () => new InvalidOperationException("RemoveOuterQuotationMarks should only be called when the JsonType of this token is String."));

            return new JsonToken(_buffer, _startIndex + 1, Length - 2, JsonTokenType.String);
        }
    }
}