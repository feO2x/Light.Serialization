namespace Light.Serialization.Json
{
    /// <summary>
    /// Represents a JSON character that is escaped with a backslash and a single character.
    /// These are \b, \f, \n, \r, \t, \", \/, and \\ according to http://json.org/.
    /// </summary>
    public struct SingleEscapedCharacter
    {
        /// <summary>
        /// Gets the escaped character in its .NET representation.
        /// </summary>
        public readonly char EscapedCharacter;

        /// <summary>
        /// Gets the character that stands after the backslash in the JSON representation.
        /// </summary>
        public readonly char ValueAfterEscapeCharacter;

        /// <summary>
        /// Creates a new intsance of <see cref="SingleEscapedCharacter"/>.
        /// </summary>
        /// <param name="escapedCharacter">The escaped character in its .NET represenetation.</param>
        /// <param name="valueAfterEscapeCharacter">The character that stands after the backslash in the JSON representation.</param>
        public SingleEscapedCharacter(char escapedCharacter, char valueAfterEscapeCharacter)
        {
            EscapedCharacter = escapedCharacter;
            ValueAfterEscapeCharacter = valueAfterEscapeCharacter;
        }
    }
}