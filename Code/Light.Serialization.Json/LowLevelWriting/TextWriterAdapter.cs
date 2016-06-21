using System;
using System.IO;
using Light.GuardClauses;

namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Represents an <see cref="IStreamWriter" /> that forwards the calls to a
    ///     <see cref="TextWriter" /> encapsulating a text stream.
    /// </summary>
    public sealed class TextWriterAdapter : IStreamWriter
    {
        private readonly TextWriter _textWriter;

        /// <summary>
        ///     Initializes a new instance of <see cref="TextWriterAdapter" />.
        /// </summary>
        /// <param name="textWriter">The text writer the calls are forwared to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="textWriter" /> is null.</exception>
        public TextWriterAdapter(TextWriter textWriter)
        {
            textWriter.MustNotBeNull(nameof(textWriter));

            _textWriter = textWriter;
        }

        /// <summary>
        ///     Forwards the character to the text writer.
        /// </summary>
        /// <param name="character">The character to be written.</param>
        /// <returns>The stream writer for method chaining.</returns>
        public IStreamWriter Write(char character)
        {
            _textWriter.Write(character);
            return this;
        }

        /// <summary>
        ///     Forwards the string to the text writer if it is not null or empty.
        /// </summary>
        /// <param name="string">The string to be written.</param>
        /// <returns>The stream writer for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="string" /> is null.</exception>
        public IStreamWriter Write(string @string)
        {
            if (string.IsNullOrEmpty(@string))
                return this;

            _textWriter.Write(@string);
            return this;
        }

        /// <summary>
        ///     Disposes of the text writer.
        /// </summary>
        public void Dispose()
        {
            _textWriter.Dispose();
        }
    }
}