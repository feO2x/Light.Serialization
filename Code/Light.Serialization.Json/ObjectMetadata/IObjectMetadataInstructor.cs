namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the abstraction of an object that serializes the metadata section ($key, $id, $ref) at the beginning of a complex JSON object.
    /// </summary>
    public interface IObjectMetadataInstructor
    {
        /// <summary>
        ///     Instructs the JSON writer to serialize the metadata section of the specified object.
        /// </summary>
        /// <param name="serializationContext">The serialization context for the object to be serialized.</param>
        /// <returns>The value indicating whether the values of the object have to be serialized.</returns>
        bool SerializeMetadata(JsonSerializationContext serializationContext);
    }
}