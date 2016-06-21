using System;
using System.Text;
using Light.GuardClauses;

namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Represents an <see cref="IStreamWriter" /> that forwards calls to a <see cref="StringBuilder" />.
    /// </summary>
    public sealed class StringBuilderAdapter : IStreamWriter
    {
        private readonly StringBuilder _stringBuilder;

        /// <summary>
        ///     Initializes a new instance of <see cref="StringBuilderAdapter" />.
        /// </summary>
        /// <param name="stringBuilder">The string builder the calls are forwarded to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stringBuilder" /> is null.</exception>
        public StringBuilderAdapter(StringBuilder stringBuilder)
        {
            stringBuilder.MustNotBeNull(nameof(stringBuilder));

            _stringBuilder = stringBuilder;
        }

        /// <summary>
        ///     Forwards the specified character to the string builder.
        /// </summary>
        /// <param name="character">The character to be written.</param>
        /// <returns>The stream writer for method chaining.</returns>
        public IStreamWriter Write(char character)
        {
            _stringBuilder.Append(character);
            return this;
        }

        /// <summary>
        ///     Forwards the specified string to the string builder if it is not null or empty.
        /// </summary>
        /// <param name="string">The string to be written.</param>
        /// <returns>The stream writer for method chaining.</returns>
        public IStreamWriter Write(string @string)
        {
            if (string.IsNullOrEmpty(@string))
                return this;

            _stringBuilder.Append(@string);
            return this;
        }

        /// <summary>
        ///     Not needed for this stream writer.
        /// </summary>
        public void Dispose() { }
    }
}