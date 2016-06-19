using System;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents the interface for the deserializer that the <see cref="JsonDeserializationContext" />
    ///     uses to access it.
    /// </summary>
    public interface IRecursiveDeserializer
    {
        /// <summary>
        ///     Parses the given token with the specified requested type.
        /// </summary>
        /// <param name="token">The token to be deserialized.</param>
        /// <param name="requestedType">The type used to reference the deserialized instance.</param>
        /// <returns>The parse result for the operation.</returns>
        ParseResult DeserializeToken(JsonToken token, Type requestedType);

        /// <summary>
        ///     Checks if the <see cref="ComplexObjectParser" /> should switch to another parser
        ///     when the type to be constructed was retrieved from the metadata section of a
        ///     complex JSON object.
        /// </summary>
        /// <param name="typeToBeConstructed">The type to be constructed.</param>
        /// <returns>
        ///     An instance of a parser implementing the <see cref="ISwitchParserForComplexObject" /> interface if
        ///     a switch should be performed, else null.
        /// </returns>
        ISwitchParserForComplexObject FindParserForType(Type typeToBeConstructed);
    }
}