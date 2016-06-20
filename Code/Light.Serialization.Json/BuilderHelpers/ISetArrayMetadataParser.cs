using System;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction of setting an <see cref="IArrayMetadataParser" /> via property injection.
    /// </summary>
    public interface ISetArrayMetadataParser
    {
        /// <summary>
        ///     Sets the specified <see cref="IArrayMetadataParser" /> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        IArrayMetadataParser MetadataParser { set; }
    }
}