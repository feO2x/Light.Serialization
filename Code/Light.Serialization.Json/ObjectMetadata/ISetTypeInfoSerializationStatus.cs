namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the abstraction used to enable or disable type information
    /// </summary>
    public interface ISetTypeInfoSerializationStatus
    {
        /// <summary>
        ///     Sets the value indicating whether type information is serialized in the metadata section of a complex JSON object.
        /// </summary>
        bool IsSerializingTypeInfo { set; }
    }
}