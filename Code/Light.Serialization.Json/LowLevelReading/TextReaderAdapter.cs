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
        ///     Gets the default character buffer size. This value defaults to 2048.
        /// </summary>
        public const int DefaultBufferSize = 2048;

        /// <summary>
        ///     Gets the minimum char buffer size. This value is 32.
        /// </summary>
        public const int MinimumBufferSize = 32;

        private readonly char[] _buffer;
        private readonly TextReader _textReader;
        private int _endIndex = -1;
        private bool _isAtEndOfStream;
        private int _loadNextContentIndex;
        private int _pinnedIndex = -1;
        private int _bufferPosition;

        /// <summary>
        ///     Creates a new instance of <see cref="TextReaderAdapter" />.
        /// </summary>
        /// <param name="textReader">The text reader that is used to read from the input stream.</param>
        /// <param name="bufferSize">The size of the buffer (optional). Defaults to 2048 characters.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="textReader" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="bufferSize" /> is less than 32.</exception>
        public TextReaderAdapter(TextReader textReader, int bufferSize = DefaultBufferSize)
        {
            textReader.MustNotBeNull(nameof(textReader));
            bufferSize.MustNotBeLessThan(MinimumBufferSize);

            _textReader = textReader;
            _buffer = new char[bufferSize];
            ReadFromTextReaderIntoBuffer();
        }

        /// <summary>
        ///     Gets the buffer that contains the character currently held in memory.
        /// </summary>
        public char[] Buffer => _buffer;

        /// <summary>
        ///     Gets the current position in the buffer.
        /// </summary>
        public int BufferPosition => _bufferPosition;

        /// <summary>
        ///     Gets the value indicating whether the end of the stream is reached.
        /// </summary>
        public bool IsAtEndOfStream => _isAtEndOfStream;

        /// <summary>
        ///     Gets the current character of the stream.
        /// </summary>
        public char CurrentCharacter => _buffer[_bufferPosition];

        /// <summary>
        ///     Pins the current index in the buffer. This way the characters from this position onward are not altered when new content is loaded from the text reader.
        /// </summary>
        /// <returns>The current index.</returns>
        public int PinPosition()
        {
            _pinnedIndex = _bufferPosition;
            return _pinnedIndex;
        }

        /// <summary>
        ///     Advances the position of the current index by one, if possible.
        /// </summary>
        /// <returns>True if the current position was increased, else false.</returns>
        public bool Advance()
        {
            if (_isAtEndOfStream)
                return false;

            AdvanceCurrentIndex();

            if (_bufferPosition == _endIndex)
            {
                _isAtEndOfStream = true;
                return false;
            }

            if (_bufferPosition == _pinnedIndex)
                throw new DeserializationException("The buffer size for deseserializing the JSON document is too small (the pinned index was reached).");

            if (_bufferPosition == _loadNextContentIndex)
            {
                ReadFromTextReaderIntoBuffer();
                return !_isAtEndOfStream;
            }

            return true;
        }

        /// <summary>
        ///     Disposes of the text reader.
        /// </summary>
        public void Dispose()
        {
            _textReader.Dispose();
        }

        private void ReadFromTextReaderIntoBuffer()
        {
            if (_pinnedIndex == -1)
            {
                var numberOfCharactersToRead = _buffer.Length - _loadNextContentIndex;
                var numberOfCharactersRead = _textReader.Read(_buffer, _loadNextContentIndex, numberOfCharactersToRead);
                if (numberOfCharactersRead == 0)
                    _isAtEndOfStream = true;
                else if (numberOfCharactersRead < _buffer.Length)
                    AdvanceLoadNextContentIndex(numberOfCharactersRead);

                return;
            }

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
                        _isAtEndOfStream = true;
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

        private void AdvanceCurrentIndex()
        {
            if (++_bufferPosition == _buffer.Length)
                _bufferPosition = 0;
        }

        private void AdvanceLoadNextContentIndex(int count)
        {
            _loadNextContentIndex += count;
            if (_loadNextContentIndex == _buffer.Length)
                _loadNextContentIndex = 0;
        }
    }
}