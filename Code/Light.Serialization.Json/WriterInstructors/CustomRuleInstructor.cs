using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.BuilderInjection;
using Light.Serialization.Json.ComplexTypeDecomposition;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.WriterInstructors
{
    /// <summary>
    ///     Represents a JSON Writer Instructor that has a fixed set of value providers for a certain complex type.
    ///     Instances of this class are created i.a. when defining custom serialization rules for certain types.
    /// </summary>
    public sealed class CustomRuleInstructor : IJsonWriterInstructor, ISetObjectMetadataInstructor
    {
        private readonly IList<IValueProvider> _valueProviders;

        /// <summary>
        ///     Gets the type that this instructor is customized for.
        /// </summary>
        public readonly Type TargetType;

        private IObjectMetadataInstructor _metadataInstructor;

        /// <summary>
        ///     Creates a new instance of <see cref="CustomRuleInstructor" />.
        /// </summary>
        /// <param name="targetType">The complex type that this instructor can serialize.</param>
        /// <param name="valueProviders">The set of value providers used to read values from instances of the target type.</param>
        /// <param name="metadataInstructor">The object used to serialize the metadata section of the complex object.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters is null.</exception>
        public CustomRuleInstructor(Type targetType, IList<IValueProvider> valueProviders, IObjectMetadataInstructor metadataInstructor)
        {
            valueProviders.MustNotBeNull(nameof(valueProviders));
            targetType.MustNotBeNull(nameof(targetType));
            metadataInstructor.MustNotBeNull(nameof(metadataInstructor));

            TargetType = targetType;
            _valueProviders = valueProviders;
            _metadataInstructor = metadataInstructor;
        }

        /// <summary>
        ///     Checks if the actual type is equal to the target type registered with this instructor.
        /// </summary>
        public bool IsSuitableFor(object @object, Type actualType, Type referencedType)
        {
            return actualType == TargetType;
        }

        /// <summary>
        ///     Serializes the specified object using the serialization context.
        /// </summary>
        /// <param name="serializationContext">The serialization context for the object to be serialized.</param>
        public void Serialize(JsonSerializationContext serializationContext)
        {
            ComplexObjectHelper.SerializeComplexObject(serializationContext, _valueProviders, _metadataInstructor);
        }

        /// <summary>
        ///     Gets or sets the object used to serialize the metadata section of the complex object.
        /// </summary>
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