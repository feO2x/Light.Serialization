namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the abstraction used to enable or disable object reference preservation.
    /// </summary>
    public interface ISetObjectReferencePreservationStatus
    {
        /// <summary>
        ///     Sets the value indicating whether object ids and references to objects are serialized in the metadata section of a complex JSON object.
        /// </summary>
        bool IsSerializingObjectIds { set; }
    }
}