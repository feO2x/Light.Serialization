using Light.GuardClauses;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents additional error info for the specified token, e.g. line number and position of the error
    ///     and optional surrounding characters.
    /// </summary>
    public struct AdditionalErrorInfo
    {
        /// <summary>
        ///     Gets the line number where the erroneous token resides.
        /// </summary>
        public readonly int LineNumber;

        /// <summary>
        ///     Gets the position within the line where the erroneous token resides.
        /// </summary>
        public readonly int Position;

        /// <summary>
        ///     Gets the characters after the erroneous token. This value could be null.
        /// </summary>
        public readonly string CharactersAfterErrouneousToken;

        public AdditionalErrorInfo(int lineNumber, int position, string charactersAfterErrouneousToken)
        {
            lineNumber.MustNotBeLessThan(0);
            position.MustNotBeLessThan(0);

            LineNumber = lineNumber;
            Position = position;
            CharactersAfterErrouneousToken = charactersAfterErrouneousToken;
        }
    }
}