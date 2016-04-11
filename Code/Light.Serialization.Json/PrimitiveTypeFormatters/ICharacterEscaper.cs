namespace Light.Serialization.Json.PrimitiveTypeFormatters
{
    /// <summary>
    ///     Represents the abstraction of an object that creates the JSON escape sequence for a special character.
    /// </summary>
    public interface ICharacterEscaper
    {
        /// <summary>
        ///     Creates the JSON escape sequence for the specified .NET character if it is a special character.
        /// </summary>
        /// <param name="character">The character whose escape sequence should be created.</param>
        /// <returns>The escape sequence if <paramref name="character" /> is a special character, else null.</returns>
        char[] Escape(char character);
    }
}