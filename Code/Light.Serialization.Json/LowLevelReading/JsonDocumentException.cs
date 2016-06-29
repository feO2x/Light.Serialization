using System;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents an exception indicating an error in the JSON document.
    /// </summary>
    public class JsonDocumentException : ErroneousTokenException
    {
        /// <summary>
        ///     Creates a new instance of <see cref="JsonDocumentException" />.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="erroneousToken">The erroneous JSON token.</param>
        /// <param name="innerException">The exception the led to this one (optional).</param>
        public JsonDocumentException(string message, JsonToken erroneousToken, Exception innerException = null)
            : base(message, erroneousToken) { }
    }
}