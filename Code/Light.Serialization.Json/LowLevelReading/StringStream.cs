using System;
using Light.GuardClauses;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents an <see cref="ICharacterStream" /> that provides access to a .NET in memory string / character array.
    /// </summary>
    public sealed class StringStream : ICharacterStream
    {
        private readonly char[] _buffer;
        private int _position;

        /// <summary>
        ///     Creates a new instance of <see cref="StringStream" />.
        /// </summary>
        /// <param name="string">The string that will be used as the internal buffer.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="string" /> is null.</exception>
        public StringStream(string @string)
        {
            @string.MustNotBeNull(nameof(@string));

            _buffer = @string.ToCharArray();
        }

        /// <summary>
        ///     Creates a new instance of <see cref="StringStream" />.
        /// </summary>
        /// <param name="characterArray">The character array that will be used as the internal buffer.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="characterArray" /> is null.</exception>
        public StringStream(char[] characterArray)
        {
            characterArray.MustNotBeNull(nameof(characterArray));

            _buffer = characterArray;
        }

        /// <summary>
        ///     Gets the internal buffer of the <see cref="StringStream" />.
        /// </summary>
        public char[] Buffer => _buffer;

        /// <summary>
        ///     Gets the current character that this stream points to.
        /// </summary>
        public char CurrentCharacter
        {
            get
            {
                IsAtEndOfStream.MustBeFalse(exception: () => new InvalidOperationException("The end of the stream is reached."));
                return _buffer[_position];
            }
        }

        /// <summary>
        ///     Advances the current position to the next character in the underlying buffer.
        /// </summary>
        /// <returns>True if the position could be advanced, else false, if the end of the stream was reached.</returns>
        public bool Advance()
        {
            if (_position == _buffer.Length)
                return false;

            ++_position;

            return _buffer.Length != _position;
        }

        /// <summary>
        ///     Gets the current position that the stream is pointing to.
        /// </summary>
        public int Position => _position;

        /// <summary>
        ///     Does nothing because the whole content of the JSON document is in memory already.
        /// </summary>
        /// <returns>The current position of the stream.</returns>
        public int PinPosition()
        {
            return _position;
        }

        /// <summary>
        ///     Gets the value indicating whether the current position has reached the end of the underlying buffer.
        /// </summary>
        public bool IsAtEndOfStream => _position == _buffer.Length;

        /// <summary>
        ///     Does nothing because no actual stream is referenced.
        /// </summary>
        public void Dispose() { }
    }
}