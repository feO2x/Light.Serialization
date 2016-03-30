using System;

namespace Light.Serialization.Abstractions
{
    /// <summary>
    ///     This exception indicates that an object graph could not be serialized properly.
    /// </summary>
    public class SerializationException : Exception
    {
        /// <summary>
        ///     Creates a new instance of <see cref="SerializationException" />
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="innerException">The exception that led to this one (optional).</param>
        public SerializationException(string message, Exception innerException = null)
            : base(message, innerException) { }
    }
}