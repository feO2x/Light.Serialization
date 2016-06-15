using System;
using System.IO;
using Light.GuardClauses;
using Light.Serialization.Abstractions;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents an <see cref="ICharacterStream" /> that buffers text input
    ///     from a <see cref="TextReader" />.
    /// </summary>
    public sealed class TextReaderAdapter : ICharacterStream
    {
        /// <summary>
        ///     Gets the default char buffer size of 2048.
        /// </summary>
        public const int DefaultBufferSize = 2048;

        private readonly char[] _buffer;
        private readonly TextReader _textReader;
        private int _currentIndex;
        private bool _isEndOfStream;
        private int _pinnedIndex;
        private int _loadNextContentIndex;
        private int _endIndex = -1;

        /// <summary>
        /// Creates a new instance of <see cref="TextReaderAdapter"/>.
        /// </summary>
        /// <param name="textReader">The text reader that is used to read from the input stream.</param>
        /// <param name="bufferSize">The size of the buffer (optional). Defaults to 2048 characters.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="textReader"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="bufferSize"/> is less than 64.</exception>
        public TextReaderAdapter(TextReader textReader, int bufferSize = DefaultBufferSize)
        {
            textReader.MustNotBeNull(nameof(textReader));
            bufferSize.MustNotBeLessThan(64);

            _pinnedIndex = bufferSize - 1;
            _textReader = textReader;
            _buffer = new char[bufferSize];
            ReadBlockFromTextReader();
        }

        /// <summary>
        /// Gets the buffer that contains the character currently held in memory.
        /// </summary>
        public char[] Buffer => _buffer;

        /// <summary>
        /// Gets the current position in the buffer.
        /// </summary>
        public int CurrentIndex => _currentIndex;

        /// <summary>
        /// Gets the value indicating whether the end of the stream is reached.
        /// </summary>
        public bool IsEndOfStream => _isEndOfStream;

        /// <summary>
        /// Gets the current character of the stream.
        /// </summary>
        public char CurrentCharacter => _buffer[_currentIndex];

        /// <summary>
        /// Pins the current index in the buffer. This way the characters from this position on are not altered when new content is loaded from the text reader.
        /// </summary>
        /// <returns>The current index.</returns>
        public int PinIndex()
        {
            _pinnedIndex = _currentIndex;
            return _pinnedIndex;
        }

        /// <summary>
        /// Advances the position of the current index by one, if possible. 
        /// </summary>
        /// <returns>True if the current position was increased, else false.</returns>
        public bool Advance()
        {
            if (_isEndOfStream)
                return false;

            IncreaseCurrentIndex();

            if (_currentIndex == _endIndex)
            {
                _isEndOfStream = true;
                return false;
            }

            if (_currentIndex == _loadNextContentIndex)
            {
                ReadBlockFromTextReader();
                return !_isEndOfStream;
            }

            if (_currentIndex == _pinnedIndex)
                throw new DeserializationException("The buffer size for deseserializing this JSON document is too small (the pinned index was reached).");

            return true;
        }

        private void ReadBlockFromTextReader()
        {
            var loopIteration = 1;
            while (true)
            {
                var startIndex = _loadNextContentIndex;
                var numberOfCharactersToRead = _loadNextContentIndex < _pinnedIndex ? _pinnedIndex - _loadNextContentIndex :
                                                   _buffer.Length - _loadNextContentIndex;

                var numberOfCharactersRead = _textReader.Read(_buffer, startIndex, numberOfCharactersToRead);
                if (numberOfCharactersRead == 0)
                {
                    if (loopIteration == 1)
                    {
                        _isEndOfStream = true;
                        return;
                    }
                    _endIndex = _loadNextContentIndex;
                    return;
                }

                AdvanceLoadNextContentIndex(numberOfCharactersRead);
                if (_loadNextContentIndex == _pinnedIndex)
                    return;

                ++loopIteration;
            }
        }

        private void IncreaseCurrentIndex()
        {
            ++_currentIndex;

            if (_currentIndex == _buffer.Length)
                _currentIndex = 0;
        }

        private void AdvanceLoadNextContentIndex(int count)
        {
            _loadNextContentIndex += count;
            if (_loadNextContentIndex == _buffer.Length)
                _loadNextContentIndex = 0;
        }
    }
}