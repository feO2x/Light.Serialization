namespace Light.Serialization.Json.BuilderInterfaces
{
    /// <summary>
    ///     Represents the abstraction used to enable or disable object reference preservation.
    /// </summary>
    public interface ISetObjectReferencePreservationStatus
    {
        /// <summary>
        ///     Sets the value indicating whether object ids and references to objects are serialized in the metadata section.
        /// </summary>
        bool IsSerializingObjectIds { set; }
    }
}