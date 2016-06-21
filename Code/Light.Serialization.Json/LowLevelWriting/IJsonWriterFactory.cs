using System;
using System.IO;
using System.Text;

namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Represents the abstraction of an object that is able to create <see cref="IJsonWriter" /> instances that write to string builders or text streams.
    /// </summary>
    public interface IJsonWriterFactory
    {
        /// <summary>
        ///     Creates a new <see cref="IJsonWriter" /> instance for the specified string builder.
        /// </summary>
        /// <param name="stringBuilder">The string builder that the <see cref="IJsonWriter" /> uses to write the document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stringBuilder" /> is null.</exception>
        IJsonWriter CreateFromStringBuilder(StringBuilder stringBuilder);

        /// <summary>
        ///     Creates a new <see cref="IJsonWriter" /> instance for the specified text writer.
        /// </summary>
        /// <param name="writer">The text writer that the <see cref="IJsonWriter" /> uses to write the document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer" /> is null.</exception>
        IJsonWriter CreateFromTextWriter(TextWriter writer);

        /// <summary>
        ///     Creates a new <see cref="IJsonWriter" /> instance for the specified binary writer.
        /// </summary>
        /// <param name="writer">The binary writer that the <see cref="IJsonWriter" /> uses to write the document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer" /> is null.</exception>
        IJsonWriter CreateFromBinaryWriter(BinaryWriter writer);
    }
}