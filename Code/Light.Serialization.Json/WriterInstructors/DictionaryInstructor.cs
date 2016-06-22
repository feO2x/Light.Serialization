using System;
using System.Collections;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.BuilderHelpers;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.PrimitiveTypeFormatters;

namespace Light.Serialization.Json.WriterInstructors
{
    /// <summary>
    ///     Represents an <see cref="IJsonWriterInstructor" /> that serializes .NET dictionaries to complex JSON objects.
    /// </summary>
    public sealed class DictionaryInstructor : IJsonWriterInstructor, ISetObjectMetadataInstructor, ISetPrimitiveTypeFormatters
    {
        private IMetadataInstructor _metadataInstructor;
        private IDictionary<Type, IPrimitiveTypeFormatter> _primitiveTypeFormattersMapping;

        /// <summary>
        ///     Creates a new instance of <see cref="DictionaryInstructor" />.
        /// </summary>
        /// <param name="primitiveTypeFormattersMapping">The dictionary containing mappings from type to primitive type formatters which are used to serialize keys.</param>
        /// <param name="metadataInstructor">The object that is used to serialize the metadata section of the complex object.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="primitiveTypeFormattersMapping" /> or <paramref name="metadataInstructor" /> is null.</exception>
        public DictionaryInstructor(IDictionary<Type, IPrimitiveTypeFormatter> primitiveTypeFormattersMapping, IMetadataInstructor metadataInstructor)
        {
            primitiveTypeFormattersMapping.MustNotBeNull(nameof(primitiveTypeFormattersMapping));
            metadataInstructor.MustNotBeNull(nameof(metadataInstructor));

            _primitiveTypeFormattersMapping = primitiveTypeFormattersMapping;
            _metadataInstructor = metadataInstructor;
        }

        /// <summary>
        ///     Checks if the specified object is an <see cref="IDictionary" /> instance.
        /// </summary>
        public bool IsSuitableFor(object @object, Type actualType)
        {
            return @object is IDictionary;
        }

        /// <summary>
        ///     Serializes the specified dictionary using the serialization context.
        /// </summary>
        /// <param name="serializationContext">The serialization context for the dictionary.</param>
        /// <exception cref="InvalidCastException">Thrown when the object to be serialized is not of type <see cref="IDictionary" />.</exception>
        public void Serialize(JsonSerializationContext serializationContext)
        {
            var dictionary = (IDictionary) serializationContext.ObjectToBeSerialized;

            var writer = serializationContext.Writer;
            writer.BeginObject();

            var shouldSerializeData = _metadataInstructor.SerializeMetadata(serializationContext);

            if (shouldSerializeData == false || dictionary.Count == 0)
            {
                writer.EndObject();
                return;
            }

            var dicitionaryEnumerator = dictionary.GetEnumerator();
            dicitionaryEnumerator.MoveNext();
            while (true)
            {
                var key = dicitionaryEnumerator.Key;

                // Keys in a (.NET) dictionary can never be null, thus there is no check here.

                var keyType = key.GetType();
                if (_primitiveTypeFormattersMapping.ContainsKey(keyType))
                {
                    var typeFormatter = _primitiveTypeFormattersMapping[keyType];
                    writer.WriteKey(typeFormatter.FormatPrimitiveType(key), typeFormatter.ShouldBeNormalizedKey);
                }
                else
                    throw new SerializationException($"The value \"{key}\" can not be transformed to a JSON string and therefore cannot be used as a key in a complex JSON object.");

                var value = dicitionaryEnumerator.Value;
                if (value == null)
                    writer.WriteNull();
                else
                    serializationContext.SerializeChild(value);

                if (dicitionaryEnumerator.MoveNext())
                    writer.WriteDelimiter();
                else
                    break;
            }
            writer.EndObject();
        }

        /// <summary>
        ///     Gets or sets the object used to serialize the metadata section of the dictionary.
        /// </summary>
        public IMetadataInstructor MetadataInstructor
        {
            get { return _metadataInstructor; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _metadataInstructor = value;
            }
        }

        /// <summary>
        ///     Gets or sets the dictionary containing the mappings from types to primitive formatters.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IDictionary<Type, IPrimitiveTypeFormatter> PrimitiveTypeFormattersMapping
        {
            get { return _primitiveTypeFormattersMapping; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _primitiveTypeFormattersMapping = value;
            }
        }
    }
}