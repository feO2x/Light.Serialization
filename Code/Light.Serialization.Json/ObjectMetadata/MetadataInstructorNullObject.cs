namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents an IObjectMetadataInstructor that does nothing.
    /// </summary>
    public sealed class MetadataInstructorNullObject : IObjectMetadataInstructor
    {
        /// <summary>
        ///     Serializes no metdata of the given object.
        /// </summary>
        /// <returns>Always true to indicate that the data of the object has to be serialized.</returns>
        public bool SerializeMetadata(JsonSerializationContext serializationContext)
        {
            return true;
        }
    }
}