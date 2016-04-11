using System;
using System.Collections;

namespace Light.Serialization.Json.WriterInstructors
{
    /// <summary>
    ///     Represents a JSON Writer Instructor that serializes .NET collections to JSON arrays.
    /// </summary>
    public sealed class CollectionInstructor : IJsonWriterInstructor
    {
        /// <summary>
        ///     Checks if the specified object implements the IEnumerable interface.
        /// </summary>
        /// <returns>True if the specified object is an <see cref="IEnumerable" />, else false.</returns>
        public bool IsSuitableFor(object @object, Type actualType, Type referencedType)
        {
            return @object is IEnumerable;
        }

        /// <summary>
        ///     Serializes the collection using the specified context.
        /// </summary>
        /// <exception cref="InvalidCastException">Thrown when the object to be serialized is not of type <see cref="IEnumerable" />.</exception>
        public void Serialize(JsonSerializationContext serializationContext)
        {
            var enumerable = (IEnumerable) serializationContext.@ObjectToBeSerialized;

            var writer = serializationContext.Writer;
            var enumerator = enumerable.GetEnumerator();
            if (enumerator.MoveNext() == false)
            {
                writer.BeginArray();
                writer.EndArray();
                return;
            }

            writer.BeginArray();
            while (true)
            {
                var currentChildObject = enumerator.Current;
                if (currentChildObject == null)
                    writer.WriteNull();
                else
                {
                    var childType = currentChildObject.GetType();
                    serializationContext.SerializeChild(currentChildObject, childType, childType);
                }
                if (enumerator.MoveNext())
                    writer.WriteDelimiter();
                else
                    break;
            }
            writer.EndArray();
        }
    }
}