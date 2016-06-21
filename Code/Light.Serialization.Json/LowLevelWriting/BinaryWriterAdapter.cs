using System;
using System.IO;
using System.Text;
using Light.GuardClauses;

namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Represents an <see cref="IStreamWriter" /> that creates a string
    ///     and forwards this JSON document to the binary writer when it is disposed of.
    /// </summary>
    public sealed class BinaryWriterAdapter : IStreamWriter
    {
        private readonly BinaryWriter _binaryWriter;
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        /// <summary>
        ///     Creates a new instance of <see cref="BinaryWriterAdapter" />.
        /// </summary>
        /// <param name="binaryWriter">The binary writer the calls are forwarded to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="binaryWriter" /> is null.</exception>
        public BinaryWriterAdapter(BinaryWriter binaryWriter)
        {
            binaryWriter.MustNotBeNull(nameof(binaryWriter));

            _binaryWriter = binaryWriter;
        }

        /// <summary>
        ///     Forwards the specified character to the binary writer.
        /// </summary>
        /// <param name="character">The character to be written.</param>
        /// <returns>The stream writer for method chaining.</returns>
        public IStreamWriter Write(char character)
        {
            _stringBuilder.Append(character);
            return this;
        }

        /// <summary>
        ///     Forwards the specified string to the binary writer if it is not null or empty.
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
        ///     Creates a string from the internal string builder and
        ///     writes this string to the binary writer.
        /// </summary>
        public void Dispose()
        {
            var json = _stringBuilder.ToString();
            _binaryWriter.Write(json);
            _binaryWriter.Dispose();
        }
    }
}