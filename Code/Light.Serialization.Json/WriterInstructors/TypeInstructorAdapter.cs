using System;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Json.BuilderHelpers;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.WriterInstructors
{
    /// <summary>
    ///     Represents an <see cref="IJsonWriterInstructor" /> that can serialize <see cref="Type" /> instances.
    /// </summary>
    public sealed class TypeInstructorAdapter : IJsonWriterInstructor, ISetTypeInstructor
    {
        /// <summary>
        ///     Gets the default type key which is "type".
        /// </summary>
        public const string DefaultTypeKey = "\"type\"";

        private IObjectMetadataInstructor _metadataInstructor;
        private string _typeKey = DefaultTypeKey;

        /// <summary>
        ///     Creates a new instance of <see cref="TypeInstructorAdapter" />.
        /// </summary>
        /// <param name="metadataInstructor">The metadata instructor that can write the metadata section of a complex JSON object.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadataInstructor" /> is null.</exception>
        public TypeInstructorAdapter(IObjectMetadataInstructor metadataInstructor)
        {
            metadataInstructor.MustNotBeNull(nameof(metadataInstructor));

            _metadataInstructor = metadataInstructor;
        }

        /// <summary>
        ///     Gets the string that is used as the JSON key for the serialized type.
        ///     This value defaults to "type".
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="value" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="value" /> contains only whitespace.</exception>
        public string TypeKey
        {
            get { return _typeKey; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _typeKey = value;
            }
        }

        /// <summary>
        ///     Checks if the <paramref name="actualType" /> is of type <see cref="Type" />.
        /// </summary>
        public bool IsSuitableFor(object @object, Type actualType)
        {
            return @object is Type;
        }

        /// <summary>
        ///     Serializes the given type object.
        ///     Please note that you must only call this method when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        /// <param name="serializationContext">The serialization context for the type instance.</param>
        public void Serialize(JsonSerializationContext serializationContext)
        {
            var writer = serializationContext.Writer;

            writer.BeginObject();

            var shouldSerializeData = _metadataInstructor.SerializeMetadata(serializationContext);
            if (shouldSerializeData == false)
            {
                writer.EndObject();
                return;
            }

            writer.WriteKey(_typeKey);

            _metadataInstructor.SerializeType((Type) serializationContext.ObjectToBeSerialized, serializationContext.Writer);
            writer.EndObject();
        }

        /// <summary>
        ///     Gets or sets the metadata instructor.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IObjectMetadataInstructor MetadataInstructor
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