using System;

namespace Light.Serialization.Abstractions
{
    /// <summary>
    ///     This exception indicates that an object graph could not be deserialized properly.
    /// </summary>
    public class DeserializationException : Exception
    {
        /// <summary>
        ///     Creates a new instance of <see cref="DeserializationException" />.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="innerException">The exception that led to this one (optional).</param>
        public DeserializationException(string message, Exception innerException = null)
            : base(message, innerException) { }
    }
}