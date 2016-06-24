using System;
using Light.Serialization.Json.LowLevelWriting;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents an <see cref="IMetadataInstructor" /> for complex JSON objects that can also serialize <see cref="Type" /> instances.
    /// </summary>
    public interface ITypeMetadataInstructor : IMetadataInstructor
    {
        /// <summary>
        ///     Serializes the specified <see cref="Type" /> object.
        /// </summary>
        /// <param name="type">The type object to be serialized.</param>
        /// <param name="writer">The object that writes the JSON document.</param>
        /// <exception cref="ArgumentNullException">Thrown when any paramter is null.</exception>
        void SerializeType(Type type, IJsonWriter writer);
    }
}