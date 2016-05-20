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
        ///     Checks that the specified token is a value delimiter, or throw a JsonDocumentException otherwise.
        /// </summary>
        /// <exception cref="JsonDocumentException">Thrown when the JsonType of the token is not a value delimiter.</exception>
        public static void MustBeValueDelimiterInObject(this JsonToken token)
        {
            if (token.JsonType != JsonTokenType.ValueDelimiter)
                throw new JsonDocumentException($"Expected value delimiter or end of complex JSON object, but found {token}.", token);
        }
    }
}