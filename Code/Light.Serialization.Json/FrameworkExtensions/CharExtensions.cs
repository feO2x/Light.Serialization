namespace Light.Serialization.Json.FrameworkExtensions
{
    /// <summary>
    ///     Provides an extension method for characters.
    /// </summary>
    public static class CharExtensions
    {
        /// <summary>
        ///     Checks if the specified character is a hexadecimal character.
        /// </summary>
        /// <param name="character">The character to be checked.</param>
        /// <param name="onlyLowercaseLetters">The value indicating whether the specified character should be normalized to a lowercase character. This is turned off by default.</param>
        /// <returns>True if the specified character is a hexadecimal character (0 - 9 or a - f), else false.</returns>
        public static bool IsHexadecimal(this char character, bool onlyLowercaseLetters = false)
        {
            if (onlyLowercaseLetters == false)
                character = char.ToLowerInvariant(character);

            switch (character)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                    return true;
                default:
                    return false;
            }
        }
    }
}