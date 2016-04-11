using System;
using Light.GuardClauses;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.PrimitiveTypeFormatters
{
    /// <summary>
    ///     Represents a Primitive Type Formatter that serializes .NET characters to JSON strings.
    /// </summary>
    public sealed class CharFormatter : BasePrimitiveTypeFormatter<char>, IPrimitiveTypeFormatter
    {
        private ICharacterEscaper _characterEscaper;

        /// <summary>
        ///     Creates a new instance of <see cref="CharFormatter" />.
        /// </summary>
        /// <param name="characterEscaper">The object that is used to escape special characters.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="characterEscaper" /> is null.</exception>
        public CharFormatter(ICharacterEscaper characterEscaper) : base(true)
        {
            CharacterEscaper = characterEscaper;
        }

        /// <summary>
        ///     Gets or sets the object that is used to escape special characters.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public ICharacterEscaper CharacterEscaper
        {
            get { return _characterEscaper; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _characterEscaper = value;
            }
        }

        /// <summary>
        ///     Serializes the specified .NET character to a JSON string.
        /// </summary>
        /// <param name="primitiveValue">The character to be serialized.</param>
        /// <returns>The JSON string containing the character.</returns>
        public string FormatPrimitiveType(object primitiveValue)
        {
            var value = (char) primitiveValue;

            var characterBuffer = _characterEscaper.Escape(value);
            var jsonRepresenation = characterBuffer != null ? new string(characterBuffer) : value.ToString();

            return jsonRepresenation.SurroundWithQuotationMarks();
        }
    }
}