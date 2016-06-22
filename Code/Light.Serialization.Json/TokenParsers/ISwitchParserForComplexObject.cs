using System;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents the abstraction of a parser that the <see cref="ComplexObjectParser" />
    ///     can call when it realizes that the type to be constructed should not be handled
    ///     by it.
    /// </summary>
    public interface ISwitchParserForComplexObject
    {
        /// <summary>
        ///     Checks if the parser implementing this interface should actually deserialize the corresponding
        ///     complex object in the JSON document.
        /// </summary>
        /// <param name="typeToBeConstructed">The type to be constructed that was retrieved from the metadata section of a complex JSON object.</param>
        /// <returns>True if the <see cref="ComplexObjectParser" /> instance should switch to this parser, else false.</returns>
        bool ShouldDeserialize(Type typeToBeConstructed);

        /// <summary>
        ///     Switches from the <see cref="ComplexObjectParser" /> to the target parser that implements this interface.
        /// </summary>
        /// <param name="metadataParseResult">The metadata parse result for the complex JSON object.</param>
        /// <param name="context">The deserialization context for this complex object.</param>
        /// <param name="currentToken">The token that points to the first non-metadata key in the complex JSON object.</param>
        /// <returns>The parse result for the reconstructed instance.</returns>
        ParseResult PerformSwitch(ObjectMetadataParseResult metadataParseResult, JsonDeserializationContext context, JsonToken currentToken);
    }
}