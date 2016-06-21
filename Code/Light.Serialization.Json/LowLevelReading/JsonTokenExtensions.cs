namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Contains extension methods for the <see cref="JsonToken" /> type that simplify and standardize access to tokens.
    /// </summary>
    public static class JsonTokenExtensions
    {
        /// <summary>
        ///     Checks that the specified token is a JSON string, or throws a <see cref="JsonDocumentException" /> otherwise.
        /// </summary>
        /// <param name="token">The token to be checked.</param>
        /// <param name="message">The message that will be injected into the <see cref="JsonDocumentException" /> (optional).</param>
        /// <exception cref="JsonDocumentException">Thrown when the JsonType of the token is not String.</exception>
        public static void MustBeComplexObjectKey(this JsonToken token, string message = null)
        {
            if (token.JsonType != JsonTokenType.String)
                throw new JsonDocumentException(message ?? $"Expected JSON string or end of complex JSON object, but found {token}.", token);
        }

        /// <summary>
        ///     Checks that the specified token is a value delimiter in a complex JSON object, or throws a <see cref="JsonDocumentException" /> otherwise.
        /// </summary>
        /// <param name="token">The token to be checked.</param>
        /// <param name="message">The message that will be injected into the <see cref="JsonDocumentException" /> (optional).</param>
        /// <exception cref="JsonDocumentException">Thrown when the JsonType of the token is not a value delimiter.</exception>
        public static void MustBeValueDelimiterInObject(this JsonToken token, string message = null)
        {
            if (token.JsonType != JsonTokenType.ValueDelimiter)
                throw new JsonDocumentException(message ?? $"Expected value delimiter or end of complex JSON object, but found {token}.", token);
        }

        /// <summary>
        ///     Checks that the specified token is a value delimiter in a JSON array, or throws a <see cref="JsonDocumentException" /> otherwise.
        /// </summary>
        /// <param name="token">The token to be checked.</param>
        /// <param name="message">The message that will be injected into the <see cref="JsonDocumentException" /> (optional).</param>
        /// <exception cref="JsonDocumentException">Thrown when the JsonType of the token is not a value delimiter.</exception>
        public static void MustBeValueDelimiterInArray(this JsonToken token, string message = null)
        {
            if (token.JsonType != JsonTokenType.ValueDelimiter)
                throw new JsonDocumentException(message ?? $"Expected value delimiter or end of JSON array, but found {token}.", token);
        }

        /// <summary>
        ///     Checks that the specified token is a value delimiter, or throws a <see cref="JsonDocumentException" /> otherwise.
        /// </summary>
        /// <param name="token">The token to be checked.</param>
        /// <param name="message">The message that will be injected into the <see cref="JsonDocumentException" /> (optional).</param>
        /// <exception cref="JsonDocumentException">Thrown when the JsonType of the token is not a value delimiter.</exception>
        public static void MustBeValueDelimiter(this JsonToken token, string message = null)
        {
            if (token.JsonType != JsonTokenType.ValueDelimiter)
                throw new JsonDocumentException(message ?? $"Expected value delimiter, but found {token}.", token);
        }
    }
}