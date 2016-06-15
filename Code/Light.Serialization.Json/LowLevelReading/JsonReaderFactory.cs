using System;
using System.IO;
using Light.GuardClauses;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents a factory that creates <see cref="JsonReader" /> instances from the specified JSON document.
    /// </summary>
    public sealed class JsonReaderFactory : IJsonReaderFactory
    {
        private int _bufferSizeForStreaming = TextReaderAdapter.DefaultBufferSize;

        /// <summary>
        ///     Gets or sets the size that is used for the character buffer when the JSON document is a stream.
        ///     Defaults to 2048. Must be greater than or equal to 32.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value" /> is less than 32.</exception>
        public int BufferSizeForStreaming
        {
            get { return _bufferSizeForStreaming; }
            set
            {
                _bufferSizeForStreaming.MustNotBeLessThan(TextReaderAdapter.MinimumBufferSize, nameof(value));
                _bufferSizeForStreaming = value;
            }
        }

        /// <summary>
        ///     Creates a <see cref="JsonReader" /> instance from the specified JSON string.
        /// </summary>
        /// <param name="json">The JSON document as a string.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json" /> is null.</exception>
        public IJsonReader CreateFromString(string json)
        {
            return new JsonReader(new StringStream(json));
        }

        /// <summary>
        ///     Creates a JSON reader instance from the specified JSON stream.
        /// </summary>
        /// <param name="textReader">The text reader encapsulating the stream that contains the JSON document.</param>
        /// <returns>The created JSON reader.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="textReader" /> is null.</exception>
        public IJsonReader CreateFromTextReader(TextReader textReader)
        {
            return new JsonReader(new TextReaderAdapter(textReader, _bufferSizeForStreaming));
        }
    }
}