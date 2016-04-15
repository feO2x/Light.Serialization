using System;
using Light.Serialization.Abstractions;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    /// Represents an exception indicating an error in the JSON document.
    /// </summary>
    public class JsonDocumentException : DeserializationException
    {
        /// <summary>
        /// Gets the erroneous JSON token.
        /// </summary>
        public readonly JsonToken ErroneousToken;

        /// <summary>
        /// Creates a new instance of <see cref="JsonDocumentException"/>.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="erroneousToken">The erroneous JSON token.</param>
        /// <param name="innerException">The exception the led to this one.</param>
        public JsonDocumentException(string message, JsonToken erroneousToken, Exception innerException = null)
            : base(message, innerException)
        {
            ErroneousToken = erroneousToken;
        }
    }
}