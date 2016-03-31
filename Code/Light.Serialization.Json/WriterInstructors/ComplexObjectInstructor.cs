using System;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeDecomposition;

namespace Light.Serialization.Json.WriterInstructors
{
    /// <summary>
    ///     Represents an <see cref="IJsonWriterInstructor" /> that serializes non-primitive .NET objects (no collections, no dictionaries) to complex JSON objects.
    /// </summary>
    public sealed class ComplexObjectInstructor : IJsonWriterInstructor
    {
        private IReadableValuesTypeAnalyzer _typeAnalyzer;

        /// <summary>
        ///     Creates a new instance of <see cref="ComplexObjectInstructor" />.
        /// </summary>
        /// <param name="typeAnalyzer">The object that is used to analyze types for readable members (usually properties and fields).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeAnalyzer" /> is null.</exception>
        public ComplexObjectInstructor(IReadableValuesTypeAnalyzer typeAnalyzer)
        {
            TypeAnalyzer = typeAnalyzer;
        }

        /// <summary>
        ///     Gets or sets the object that is used to analyze types for readable members (usually properties and fields).
        /// </summary>
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
        ///     Checks if the specified object is no delegate. Important: the complex object instructor should be the last one in the collection of writer instructors
        ///     so that it is ruled out that this instructor serializes a dictionary.
        /// </summary>
        /// <returns>True if the specified object is no delegate, else false.</returns>
        public bool AppliesToObject(object @object, Type actualType, Type referencedType)
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
            ComplexObjectWriting.WriteValues(serializationContext, valueProviders);
        }
    }
}