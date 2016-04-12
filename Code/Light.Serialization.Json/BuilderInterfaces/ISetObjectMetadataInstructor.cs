using System;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.BuilderInterfaces
{
    /// <summary>
    ///     Represents the abstraction to inject a metadata instructor via a property set method.
    /// </summary>
    public interface ISetObjectMetadataInstructor
    {
        /// <summary>
        ///     Sets the specified IObjectMetadataInstructor.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        IObjectMetadataInstructor MetadataInstructor { set; }
    }
}