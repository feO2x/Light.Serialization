using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.BuilderHelpers;
using Light.Serialization.Json.ComplexTypeDecomposition;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.WriterInstructors
{
    /// <summary>
    ///     Represents an <see cref="IJsonWriterInstructor" /> that has a fixed set of value readers for a certain complex type.
    ///     Instances of this class are created e.g. when defining custom serialization rules for certain types.
    /// </summary>
    public sealed class CustomRuleInstructor : IJsonWriterInstructor, ISetObjectMetadataInstructor
    {
        private readonly List<IValueReader> _valueReaders;

        /// <summary>
        ///     Gets the type that this instructor is customized for.
        /// </summary>
        public readonly Type TargetType;

        private IMetadataInstructor _metadataInstructor;

        /// <summary>
        ///     Creates a new instance of <see cref="CustomRuleInstructor" />.
        /// </summary>
        /// <param name="targetType">The complex type that this instructor can serialize.</param>
        /// <param name="valueReaders">The set of value readers used to read values from instances of the target type.</param>
        /// <param name="metadataInstructor">The object used to serialize the metadata section of the complex object.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters is null.</exception>
        public CustomRuleInstructor(Type targetType, List<IValueReader> valueReaders, IMetadataInstructor metadataInstructor)
        {
            valueReaders.MustNotBeNull(nameof(valueReaders));
            targetType.MustNotBeNull(nameof(targetType));
            metadataInstructor.MustNotBeNull(nameof(metadataInstructor));

            TargetType = targetType;
            _valueReaders = valueReaders;
            _metadataInstructor = metadataInstructor;
        }

        /// <summary>
        ///     Checks if the actual type is equal to the target type registered with this instructor.
        /// </summary>
        public bool IsSuitableFor(object @object, Type actualType)
        {
            return actualType == TargetType;
        }

        /// <summary>
        ///     Serializes the specified object using the serialization context.
        /// </summary>
        /// <param name="serializationContext">The serialization context for the object to be serialized.</param>
        public void Serialize(JsonSerializationContext serializationContext)
        {
            ComplexObjectHelper.SerializeComplexObject(serializationContext, _valueReaders, _metadataInstructor);
        }

        /// <summary>
        ///     Gets or sets the object used to serialize the metadata section of the complex object.
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
    }
}