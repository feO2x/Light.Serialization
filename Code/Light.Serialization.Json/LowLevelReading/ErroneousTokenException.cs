using Light.GuardClauses;
using Light.Serialization.Abstractions;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents an exception indicating that a JSON token is malformed, or could not be deserialized properly with the given information.
    /// </summary>
    public class ErroneousTokenException : DeserializationException, IExchangeExceptionMessage
    {
        /// <summary>
        ///     Gets the erroneous JSON token.
        /// </summary>
        public readonly JsonToken ErroneousToken;

        private string _message;


        /// <summary>
        ///     Creates a new instance of <see cref="ErroneousTokenException" />.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="erroneousToken">The erroneous JSON token.</param>
        public ErroneousTokenException(string message, JsonToken erroneousToken) : base(message)
        {
            message.MustNotBeNullOrWhiteSpace(nameof(message));

            _message = message;
            ErroneousToken = erroneousToken;
        }

        public override string Message => _message;

        void IExchangeExceptionMessage.ExchangeMessage(string message)
        {
            message.MustNotBeNullOrWhiteSpace(nameof(message));

            _message = message;
        }
    }
}