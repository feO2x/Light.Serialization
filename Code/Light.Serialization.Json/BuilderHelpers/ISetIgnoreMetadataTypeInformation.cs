namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction of setting the information that type information in metadata sections of the JSON document should be ignored.
    /// </summary>
    public interface ISetIgnoreMetadataTypeInformation
    {
        /// <summary>
        ///     Sets the value indicating whether type information in the metadata section should be ignored.
        /// </summary>
        bool IsIgnoringMetadataTypeInformation { set; }
    }
}