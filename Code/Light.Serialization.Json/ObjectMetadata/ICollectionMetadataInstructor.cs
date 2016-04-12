namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the abstraction of an object that that serializes the metadata section (e.g. $id, $ref) at the beginning of a JSON array.
    /// </summary>
    public interface ICollectionMetadataInstructor
    {
        /// <summary>
        ///     Serializes the metadata section of the collection with the specified serialization context.
        /// </summary>
        /// <param name="serializationContext">The serialization context for the collection to be serialized.</param>
        /// <returns>The value indicating whether the items of the collection have to be serialized.</returns>
        bool SerializeMetadata(JsonSerializationContext serializationContext);
    }
}