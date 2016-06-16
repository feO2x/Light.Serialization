using System;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction of setting an <see cref="IObjectMetadataParser" /> on a target object.
    /// </summary>
    public interface ISetObjectMetadataParser
    {
        /// <summary>
        ///     Sets the <see cref="IObjectMetadataParser" /> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        IObjectMetadataParser MetadataParser { set; }
    }
}