namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents an additional abstraction that <see cref="IJsonReader" /> classes should implement to provide
    ///     additional error information when an <see cref="JsonDocumentException" /> was thrown.
    /// </summary>
    public interface IProvideAdditionalErrorInfo
    {
        /// <summary>
        ///     Provides additional error info for the specified erroneous token.
        /// </summary>
        AdditionalErrorInfo GetErrorInfoForToken(JsonToken token);
    }
}