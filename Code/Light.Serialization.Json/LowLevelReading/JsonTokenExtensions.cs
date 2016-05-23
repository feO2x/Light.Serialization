namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Contains extension methods for the JsonToken type that simplify and standardize access to tokens.
    /// </summary>
    public static class JsonTokenExtensions
    {
        /// <summary>
        ///     Checks that the specified token is a JSON string, or throws a JsonDocumentException otherwise.
        /// </summary>
        /// <exception cref="JsonDocumentException">Thrown when the JsonType of the token is not String.</exception>
        public static void MustBeComplexObjectKey(this JsonToken token)
        {
            if (token.JsonType != JsonTokenType.String)
                throw new JsonDocumentException($"Expected JSON string or end of complex JSON object, but found {token}.", token);
        }

        /// <summary>
        ///     Checks that the specified token is a value delimiter in a complex JSON object, or throws a JsonDocumentException otherwise.
        /// </summary>
        /// <exception cref="JsonDocumentException">Thrown when the JsonType of the token is not a value delimiter.</exception>
        public static void MustBeValueDelimiterInObject(this JsonToken token)
        {
            if (token.JsonType != JsonTokenType.ValueDelimiter)
                throw new JsonDocumentException($"Expected value delimiter or end of complex JSON object, but found {token}.", token);
        }

        /// <summary>
        ///     Checks that the specified token is a value delimiter in a JSON array, or throws a JsonDocumentException otherwise.
        /// </summary>
        /// <exception cref="JsonDocumentException">Thrown when the JsonType of the token is not a value delimiter.</exception>
        public static void MustBeValueDelimiterInArray(this JsonToken token)
        {
            if (token.JsonType != JsonTokenType.ValueDelimiter)
                throw new JsonDocumentException($"Expected value delimiter or end of JSON array, but found {token}.", token);
        }

        /// <summary>
        ///     Checks that the specified token is a value delimiter, or throws a JsonDocumentException otherwise.
        /// </summary>
        /// <param name="token">The token to be checked.</param>
        /// <param name="message">The message of the JsonDocumentException (optional).</param>
        /// <exception cref="JsonDocumentException">Thrown when the JsonType of the token is not a value delimiter.</exception>
        public static void MustBeValueDelimiter(this JsonToken token, string message = null)
        {
            if (token.JsonType != JsonTokenType.ValueDelimiter)
                throw new JsonDocumentException(message ?? $"Expected value delimiter, but found {token}.", token);
        }
    }
}