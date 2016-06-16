using System;
using System.IO;
using System.Text;

namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Represents the abstraction of an object that is able to create <see cref="IJsonWriter" /> instances that write to string builders or streams.
    /// </summary>
    public interface IJsonWriterFactory
    {
        /// <summary>
        ///     Creates a new <see cref="IJsonWriter" /> instance for the specified string builder.
        /// </summary>
        /// <param name="builder">The string builder that the JSON writer uses to write the document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> is null.</exception>
        IJsonWriter CreateFromStringBuilder(StringBuilder builder);

        /// <summary>
        ///     Creates a new <see cref="IJsonWriter" /> instance of the specified text writer.
        /// </summary>
        /// <param name="writer">The text writer that the JSON writer uses to write the document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer" /> is null.</exception>
        IJsonWriter CreateFromTextWriter(TextWriter writer);
    }
}