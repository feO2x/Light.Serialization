using System;
using Light.GuardClauses;
using Light.Serialization.Json.BuilderInterfaces;
using Light.Serialization.Json.ComplexTypeDecomposition;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.WriterInstructors
{
    /// <summary>
    ///     Represents a JSON Writer Instructor that serializes non-primitive .NET objects (no collections, no dictionaries) to complex JSON objects.
    /// </summary>
    public sealed class ComplexObjectInstructor : IJsonWriterInstructor, ISetObjectMetadataInstructor, ISetTypeAnalyzer
    {
        private IObjectMetadataInstructor _metadataInstructor;
        private IReadableValuesTypeAnalyzer _typeAnalyzer;

        /// <summary>
        ///     Creates a new instance of <see cref="ComplexObjectInstructor" />.
        /// </summary>
        /// <param name="typeAnalyzer">The object that is used to analyze types for readable members (usually properties and fields).</param>
        /// <param name="metadataInstructor">The object that serializes the metadata section of the complex object.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeAnalyzer" /> or <paramref name="metadataInstructor" /> is null.</exception>
        public ComplexObjectInstructor(IReadableValuesTypeAnalyzer typeAnalyzer, IObjectMetadataInstructor metadataInstructor)
        {
            TypeAnalyzer = typeAnalyzer;
            MetadataInstructor = metadataInstructor;
        }

        /// <summary>
        ///     Gets or sets the object that is used to analyze types for readable members (usually properties and fields).
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IReadableValuesTypeAnalyzer TypeAnalyzer
        {
            get { return _typeAnalyzer; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _typeAnalyzer = value;
            }
        }

        /// <summary>
        ///     Gets or sets the metadata instructor used to serialize the metadata section of the complex object.
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

        /// <summary>
        ///     Checks if the specified object is no delegate. Important: the complex object instructor should be the last one in the collection of writer instructors
        ///     so that it is ruled out that this instructor serializes a dictionary (or other complex .NET types that should not be treated by it).
        /// </summary>
        /// <returns>True if the specified object is no delegate, else false.</returns>
        public bool IsSuitableFor(object @object, Type actualType)
        {
            return @object is Delegate == false;
        }

        /// <summary>
        ///     Serializes the specified complex .NET object to a complex JSON object using the specified context.
        /// </summary>
        /// <param name="serializationContext">The serialization context for the object to be serialized.</param>
        public void Serialize(JsonSerializationContext serializationContext)
        {
            var valueProviders = _typeAnalyzer.AnalyzeType(serializationContext.ActualType);
            ComplexObjectHelper.SerializeComplexObject(serializationContext, valueProviders, _metadataInstructor);
        }
    }
}