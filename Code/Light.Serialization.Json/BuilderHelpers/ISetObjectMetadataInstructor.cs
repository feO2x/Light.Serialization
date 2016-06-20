using System;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction of setting an <see cref="IMetadataInstructor" /> for complex objects via property injection.
    /// </summary>
    public interface ISetObjectMetadataInstructor
    {
        /// <summary>
        ///     Sets the specified <see cref="IMetadataInstructor" /> for complex objects.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        IMetadataInstructor MetadataInstructor { set; }
    }
}