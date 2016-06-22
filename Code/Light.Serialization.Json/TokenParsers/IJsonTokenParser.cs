namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents the abstraction of a parser that can deserialize certain types of JSON tokens.
    /// </summary>
    public interface IJsonTokenParser
    {
        /// <summary>
        ///     Gets the value indicating if this parser can be cached for a certain JSON token type - .NET type combination.
        /// </summary>
        bool CanBeCached { get; }

        /// <summary>
        ///     Checks if this parser is suitable for the specified token with the requested .NET type.
        /// </summary>
        /// <param name="context">The deserialization context for the object to be parsed.</param>
        /// <returns>True if the parser can deserialized the JSON token as the requested type, else false.</returns>
        bool IsSuitableFor(JsonDeserializationContext context);

        /// <summary>
        ///     Parses the JSON token using the specified deserialization context.
        ///     This method must only be called when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        /// <param name="context">The deserialization context of the object to be parsed.</param>
        /// <returns>The info about the parsed value.</returns>
        ParseResult ParseValue(JsonDeserializationContext context);
    }
}