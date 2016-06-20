using System;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction of setting an <see cref="ITypeParser" /> instance via property injection.
    /// </summary>
    public interface ISetTypeParser
    {
        /// <summary>
        ///     Sets the metadata parser.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        ITypeParser MetadataParser { set; }
    }
}