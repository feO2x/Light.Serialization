using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    /// Represents the abstraction of an object that can parse the metadata section of a JSON array.
    /// </summary>
    public interface IArrayMetadataParser
    {
        /// <summary>
        ///     Parses the metadata section of a JSON array given the first token and the deserialization context.
        /// </summary>
        /// <param name="currentToken">The JSON token that represents the first token in the JSON array.</param>
        /// <param name="context">The deserialization context for the JSON array to be deserialized.</param>
        /// <returns>The metadata parse result.</returns>
        ArrayMetadataParseResult ParseMetadataSection(ref JsonToken currentToken, JsonDeserializationContext context);
    }
}