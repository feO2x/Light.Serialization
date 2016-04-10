using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeDecomposition;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.WriterInstructors
{
    /// <summary>
    ///     Represents an <see cref="IJsonWriterInstructor" /> that has a fixed set of value providers for a certain complex type.
    ///     Instances of this class are created i.a. when defining custom rules for certain types.
    /// </summary>
    public sealed class CustomRuleInstructor : IJsonWriterInstructor
    {
        private readonly IObjectMetadataInstructor _metadataInstructor;
        private readonly Type _targetType;
        private readonly IList<IValueProvider> _valueProviders;

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

            _targetType = targetType;
            _valueProviders = valueProviders;
            _metadataInstructor = metadataInstructor;
        }

        /// <summary>
        ///     Checks if the actual type is equal to the target type registered with this instructor.
        /// </summary>
        public bool AppliesToObject(object @object, Type actualType, Type referencedType)
        {
            return actualType == _targetType;
        }

        /// <summary>
        ///     Serializes the specified object using the serialization context.
        /// </summary>
        /// <param name="serializationContext">The serialization context for the object to be serialized.</param>
        public void Serialize(JsonSerializationContext serializationContext)
        {
            ComplexObjectHelper.SerializeComplexObject(serializationContext, _valueProviders, _metadataInstructor);
        }
    }
}