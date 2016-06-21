using System;
using System.Text;
using Light.GuardClauses;
using Light.Serialization.Json.BuilderHelpers;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.PrimitiveTypeFormatters
{
    /// <summary>
    ///     Represents an <see cref="IPrimitiveTypeFormatter" /> that serializes .NET <see cref="string" /> instances to JSON strings.
    /// </summary>
    public sealed class StringFormatter : BasePrimitiveTypeFormatter<string>, IPrimitiveTypeFormatter, ISetCharacterEscaper
    {
        private ICharacterEscaper _characterEscaper;

        /// <summary>
        ///     Creates a new instance of <see cref="StringFormatter" />.
        /// </summary>
        /// <param name="characterEscaper">The object that is used to escape special characters.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="characterEscaper" /> is null.</exception>
        public StringFormatter(ICharacterEscaper characterEscaper) : base(false)
        {
            characterEscaper.MustNotBeNull(nameof(characterEscaper));

            CharacterEscaper = characterEscaper;
        }

        /// <summary>
        ///     Serializes the specified .NET <see cref="string" /> to a JSON string. Special characters are escaped.
        /// </summary>
        /// <param name="primitiveValue">The string to be serialized.</param>
        /// <returns>The JSON string.</returns>
        /// <exception cref="InvalidCastException">Thrown when <paramref name="primitiveValue" /> is not of type <see cref="string" />.</exception>
        public string FormatPrimitiveType(object primitiveValue)
        {
            var @string = (string) primitiveValue;

            char[] characterBuffer;
            var i = 0;
            for (; i < @string.Length; i++)
            {
                characterBuffer = _characterEscaper.Escape(@string[i]);
                if (characterBuffer != null)
                    goto EscapeStringContent;
            }
            return @string.SurroundWithQuotationMarks();

            EscapeStringContent:
            var stringBuilder = new StringBuilder();
            stringBuilder.Append('"');
            stringBuilder.Append(@string.Substring(0, i));
            stringBuilder.Append(characterBuffer);
            i++;
            for (; i < @string.Length; i++)
            {
                var character = @string[i];
                characterBuffer = _characterEscaper.Escape(character);
                if (characterBuffer == null)
                {
                    stringBuilder.Append(character);
                    continue;
                }

                stringBuilder.Append(characterBuffer);
            }
            return stringBuilder.CompleteJsonStringWithQuotationMark();
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
    }
}