using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction of setting a <see cref="IObjectMetadataInstructor" /> instance via property injection.
    /// </summary>
    public interface ISetTypeInstructor
    {
        /// <summary>
        ///     Sets the metadata instructor on the target object.
        /// </summary>
        IObjectMetadataInstructor MetadataInstructor { set; }
    }
}