using System;
using Light.GuardClauses;

namespace Light.Serialization.Json.LowLevelReading
{
    public sealed class StringStream : ICharacterStream
    {
        private readonly char[] _buffer;
        private int _currentIndex;

        public StringStream(string @string) : this(@string.ToCharArray())
        {
        }

        public StringStream(char[] characterArray)
        {
            characterArray.MustNotBeNull(nameof(characterArray));

            _buffer = characterArray;
        }

        public char[] Buffer => _buffer;

        public char CurrentCharacter
        {
            get
            {
                IsEndOfStream.MustBeFalse(exception: () => new InvalidOperationException("The end of the stream is reached"));
                return _buffer[_currentIndex];
            }
        }


        public bool Advance()
        {
            if (_currentIndex == _buffer.Length)
                return false;

            ++_currentIndex;

            return _buffer.Length != _currentIndex;
        }

        public int CurrentIndex => _currentIndex;

        public bool IsEndOfStream => _currentIndex == _buffer.Length;
    }
}