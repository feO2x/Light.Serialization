namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents the parsing results for a JSON string that should be deserialized
    ///     to a primitive type that is references as a base type.
    /// </summary>
    public struct JsonStringParseResult
    {
        /// <summary>
        ///     Gets the value indicating whether a JSON token was parsed successfully.
        /// </summary>
        public readonly bool WasTokenParsedSuccessfully;

        /// <summary>
        ///     Gets the parsed value or null if the object could not be parsed.
        /// </summary>
        public readonly object ParsedObject;

        /// <summary>
        ///     Creates a new instance of <see cref="JsonStringParseResult" />.
        /// </summary>
        /// <param name="wasTokenParsedSuccessfully">The value indicating whether the JSON token could be parsed successfully.</param>
        /// <param name="parsedObject">The parsed value when parsing was possible.</param>
        public JsonStringParseResult(bool wasTokenParsedSuccessfully, object parsedObject = null)
        {
            WasTokenParsedSuccessfully = wasTokenParsedSuccessfully;
            ParsedObject = parsedObject;
        }
    }
}