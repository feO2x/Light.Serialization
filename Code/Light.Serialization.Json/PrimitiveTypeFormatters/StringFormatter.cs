using System;
using System.Text;
using Light.GuardClauses;
using Light.Serialization.Json.BuilderHelpers;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.PrimitiveTypeFormatters
{
    /// <summary>
    ///     Represents a Primitive Type Formatter that serializes .NET strings to JSON strings.
    /// </summary>
    public sealed class StringFormatter : BasePrimitiveTypeFormatter<string>, IPrimitiveTypeFormatter, ISetCharacterEscaper
    {
        private static double _stringBuilderCapacityCoefficient = 2.0;
        private ICharacterEscaper _characterEscaper;

        /// <summary>
        ///     Creates a new instance of <see cref="StringFormatter" />.
        /// </summary>
        /// <param name="characterEscaper">The object that is used to escape special characters.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="characterEscaper" /> is null.</exception>
        public StringFormatter(ICharacterEscaper characterEscaper) : base(false)
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
        ///     Gets or sets the coefficient value that is used to calculate the initial capacity of the string builder that is used when
        ///     the specified string for <see cref="FormatPrimitiveType" /> contains special characters that have to be escaped.
        ///     The formula to calculate the string builder capacity is: capacity = (int) (string.Lenght * StringBuilderCapacityCoefficient);
        ///     The coefficient defaults to 2.0.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value" /> is less than 1.0.</exception>
        public static double StringBuilderCapacityCoefficient
        {
            get { return _stringBuilderCapacityCoefficient; }
            set
            {
                value.MustNotBeLessThan(1.0,
                                        message: "The coefficient must at least be 1.0.");
                _stringBuilderCapacityCoefficient = value;
            }
        }

        /// <summary>
        ///     Serializes the specified .NET string to a JSON string. Special characters are escaped.
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
            var stringBuilder = new StringBuilder((int) (@string.Length * _stringBuilderCapacityCoefficient));
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
    }
}