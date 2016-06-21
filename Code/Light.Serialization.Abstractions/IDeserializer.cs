using System;
using System.IO;

namespace Light.Serialization.Abstractions
{
    /// <summary>
    ///     Represents an object that can deserialize a string to an object graph.
    /// </summary>
    public interface IDeserializer
    {
        /// <summary>
        ///     Deserializes the specified string to an object graph.
        /// </summary>
        /// <typeparam name="T">The type used to reference the root of the deserialized object graph.</typeparam>
        /// <param name="serializedString">The string to be deserialized.</param>
        /// <returns>The deserialized object graph.</returns>
        /// <exception cref="DeserializationException">Thrown when the specified string is malformed or some part of it could not be deserialized correctly.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serializedString" /> is null.</exception>
        T Deserialize<T>(string serializedString);

        /// <summary>
        ///     Deserializes the specified string to an object graph.
        /// </summary>
        /// <param name="serializedString">The string to be deserialized.</param>
        /// <param name="requestedType">The type used to reference the root of the deserialized object.</param>
        /// <returns>The deserialized object graph.</returns>
        /// <exception cref="DeserializationException">Thrown when the specified string is malformed or some part of it could not be deserialized correctly.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serializedString" /> or <paramref name="requestedType" /> is null.</exception>
        object Deserialize(string serializedString, Type requestedType);

        /// <summary>
        ///     Deserializes the specified text stream to an object graph. The text reader is disposed of after the deserialization process.
        /// </summary>
        /// <typeparam name="T">The type used to reference the root of the deserialized object graph.</typeparam>
        /// <param name="textReader">The text reader encapsulating the stream that contains the document.</param>
        /// <returns>The deserialized object graph.</returns>
        /// <exception cref="DeserializationException">Thrown when the specified stream is malformed or some part of it could not be deserialized correctly.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="textReader" /> is null.</exception>
        T Deserialize<T>(TextReader textReader);

        /// <summary>
        ///     Deserializes the specified text stream to an object graph. The text reader is disposed of after the deserialization process.
        /// </summary>
        /// <param name="textReader">The text reader encapsulating the stream that contains the document.</param>
        /// <param name="requestedType">The type used to reference the root of the deserialized object.</param>
        /// <returns>The deserialized object graph.</returns>
        /// <exception cref="DeserializationException">Thrown when the specified stream is malformed or some part of it could not be deserialized correctly.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="textReader" /> or <paramref name="requestedType" /> is null.</exception>
        object Deserialize(TextReader textReader, Type requestedType);

        /// <summary>
        ///     Deserializes the specified binary stream to an object graph. The binary reader is disposed of after the deserialization process.
        /// </summary>
        /// <typeparam name="T">The type used to reference the root of the deserialized object graph.</typeparam>
        /// <param name="binaryReader">The binary reader encapsulating the stream that contains the document.</param>
        /// <returns>The deserialized object graph.</returns>
        /// <exception cref="DeserializationException">Thrown when the specified stream is malformed or some part of it could not be deserialized correctly.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="binaryReader" /> is null.</exception>
        T Deserialize<T>(BinaryReader binaryReader);

        /// <summary>
        ///     Deserializes the specified binary stream to an object graph. The binary reader is disposed of after the deserialization process.
        /// </summary>
        /// <param name="binaryReader">The binary reader encapsulating the stream that contains the document.</param>
        /// <param name="requestedType">The type used to reference the root of the deserialized object.</param>
        /// <returns>The deserialized object graph.</returns>
        /// <exception cref="DeserializationException">Thrown when the specified stream is malformed or some part of it could not be deserialized correctly.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="binaryReader" /> or <paramref name="requestedType" /> is null.</exception>
        object Deserialize(BinaryReader binaryReader, Type requestedType);
    }
}