using System;
using System.Collections;
using Light.GuardClauses;
using Light.Serialization.Json.BuilderHelpers;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.WriterInstructors
{
    /// <summary>
    ///     Represents an <see cref="IJsonWriterInstructor" /> that serializes .NET collections to JSON arrays.
    /// </summary>
    public sealed class CollectionInstructor : IJsonWriterInstructor, ISetCollectionMetadataInstructor
    {
        private IMetadataInstructor _metadataInstructor;

        /// <summary>
        ///     Creates a new instance of <see cref="CollectionInstructor" />.
        /// </summary>
        /// <param name="metadataInstructor">The object that serializes the metadata section of the collection.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadataInstructor" /> is null.</exception>
        public CollectionInstructor(IMetadataInstructor metadataInstructor)
        {
            metadataInstructor.MustNotBeNull(nameof(metadataInstructor));

            _metadataInstructor = metadataInstructor;
        }

        /// <summary>
        ///     Checks if the specified object implements the <see cref="IEnumerable" /> interface.
        /// </summary>
        /// <returns>True if the specified object is an <see cref="IEnumerable" />, else false.</returns>
        public bool IsSuitableFor(object @object, Type actualType)
        {
            return @object is IEnumerable;
        }

        /// <summary>
        ///     Serializes the collection using the specified context.
        /// </summary>
        /// <exception cref="InvalidCastException">Thrown when the object to be serialized is not of type <see cref="IEnumerable" />.</exception>
        public void Serialize(JsonSerializationContext serializationContext)
        {
            var enumerable = (IEnumerable) serializationContext.ObjectToBeSerialized;

            var writer = serializationContext.Writer;
            writer.BeginArray();
            var shouldSerializeItems = _metadataInstructor.SerializeMetadata(serializationContext);

            if (shouldSerializeItems == false)
            {
                writer.EndArray();
                return;
            }

            var enumerator = enumerable.GetEnumerator();
            if (enumerator.MoveNext() == false)
            {
                writer.EndArray();
                return;
            }

            while (true)
            {
                var currentChildObject = enumerator.Current;
                if (currentChildObject == null)
                    writer.WriteNull();
                else
                    serializationContext.SerializeChild(currentChildObject);
                if (enumerator.MoveNext())
                    writer.WriteDelimiter();
                else
                    break;
            }
            writer.EndArray();
        }

        /// <summary>
        ///     Gets or sets the object used to serialize the metadata section of the collection.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IMetadataInstructor MetadataInstructor
        {
            get { return _metadataInstructor; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _metadataInstructor = value;
            }
        }
    }
}