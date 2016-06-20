using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.LowLevelWriting;

namespace Light.Serialization.Json.WriterInstructors
{
    /// <summary>
    ///     Represents all information necessary to serialize a single object of a graph.
    /// </summary>
    public struct JsonSerializationContext
    {
        /// <summary>
        ///     Gets the object that should be serialized. This value is never null.
        /// </summary>
        public readonly object ObjectToBeSerialized;

        /// <summary>
        ///     Gets the actual type of the object (similar to <c>ObjectToBeSerialized.GetType()</c>).
        /// </summary>
        public readonly Type ActualType;

        /// <summary>
        ///     Gets the delegate you can use to serialize a child object
        /// </summary>
        public readonly Action<object> SerializeChild;

        /// <summary>
        ///     Gets the writer that creates the actual JSON document.
        /// </summary>
        public readonly IJsonWriter Writer;

        /// <summary>
        ///     Gets the collection containing all objects that already have been serialized.
        /// </summary>
        public readonly List<object> SerializedObjects;

        /// <summary>
        ///     Creates a new instance of <see cref="JsonSerializationContext" />.
        /// </summary>
        /// <param name="objectToBeSerialized">The object to be serialized.</param>
        /// <param name="actualType">The actual type of the object to be serialized.</param>
        /// <param name="serializeChild">The delegate that should be used to serialize child objects.</param>
        /// <param name="writer">The object that writes the actual JSON document.</param>
        /// <param name="serializedObjects">The collection containing all serialized objects if Object Reference Preservation is turned on.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the specified parameters is null.</exception>
        public JsonSerializationContext(object objectToBeSerialized,
                                        Type actualType,
                                        Action<object> serializeChild,
                                        IJsonWriter writer,
                                        List<object> serializedObjects)
        {
            objectToBeSerialized.MustNotBeNull(nameof(objectToBeSerialized));
            actualType.MustNotBeNull(nameof(actualType));
            serializeChild.MustNotBeNull(nameof(serializeChild));
            writer.MustNotBeNull(nameof(writer));
            serializedObjects.MustNotBeNull(nameof(serializedObjects));

            ObjectToBeSerialized = objectToBeSerialized;
            ActualType = actualType;
            SerializeChild = serializeChild;
            Writer = writer;
            SerializedObjects = serializedObjects;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="JsonSerializationContext" /> with the specified type as <see cref="ActualType" />.
        /// </summary>
        /// <param name="type">The new type of the context.</param>
        /// <returns>The new <see cref="JsonSerializationContext" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        public JsonSerializationContext CloneWithNewType(Type type)
        {
            return new JsonSerializationContext(ObjectToBeSerialized, type, SerializeChild, Writer, SerializedObjects);
        }
    }
}