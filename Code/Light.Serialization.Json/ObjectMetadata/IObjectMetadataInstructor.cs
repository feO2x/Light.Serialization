using System;
using Light.Serialization.Json.LowLevelWriting;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents a metadata instructor that can also serialize type information.
    /// </summary>
    public interface IObjectMetadataInstructor : IMetadataInstructor
    {
        /// <summary>
        ///     Serializes the specified type object.
        /// </summary>
        /// <param name="type">The type object to be serialized.</param>
        /// <param name="writer">The object that writes the JSON document.</param>
        /// <exception cref="ArgumentNullException">Thrown when any paramter is null.</exception>
        void SerializeType(Type type, IJsonWriter writer);
    }
}