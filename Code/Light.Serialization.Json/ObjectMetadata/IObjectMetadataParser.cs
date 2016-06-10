using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the abstraction of an object that can parse the metadata section of a complex JSON object.
    /// </summary>
    public interface IObjectMetadataParser
    {
        /// <summary>
        ///     Parses the metadata section of a complex JSON object given the first token and the deserialization context.
        /// </summary>
        /// <param name="currentToken">The JSON token that represents the first token in a complex JSON object.</param>
        /// <param name="context">The deserialization context for the object to be deserialized.</param>
        /// <returns>The metadata parse result.</returns>
        ObjectMetadataParseResult ParseMetadataSection(ref JsonToken currentToken, JsonDeserializationContext context);
    }
}