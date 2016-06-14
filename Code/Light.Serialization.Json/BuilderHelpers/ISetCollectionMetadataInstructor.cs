using System;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    /// Represents the abstraction to inject a metadata instructor for collections via a property set method.
    /// </summary>
    public interface ISetCollectionMetadataInstructor
    {
        /// <summary>
        ///     Sets the specified IObjectMetadataInstructor.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        IMetadataInstructor MetadataInstructor { set; }
    }
}