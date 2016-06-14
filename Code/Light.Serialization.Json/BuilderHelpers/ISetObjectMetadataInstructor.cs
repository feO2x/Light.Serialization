using System;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction to inject a metadata instructor for complex objects via a property set method.
    /// </summary>
    public interface ISetObjectMetadataInstructor
    {
        /// <summary>
        ///     Sets the specified IMetadataInstructor.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        IMetadataInstructor MetadataInstructor { set; }
    }
}