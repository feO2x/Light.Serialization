using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.PrimitiveTypeFormatters;

namespace Light.Serialization.Json.WriterInstructors
{
    /// <summary>
    ///     Represents an <see cref="IJsonWriterInstructor" /> that serializes primitive types (such as int, double, bool, string) to
    ///     corresponding JSON values by using primitive type formatters.
    /// </summary>
    public sealed class PrimitiveTypeInstructor : IJsonWriterInstructor
    {
        private readonly IDictionary<Type, IPrimitiveTypeFormatter> _primitiveTypeToFormattersMapping;

        /// <summary>
        ///     Creates a new instance of <see cref="PrimitiveTypeInstructor" />.
        /// </summary>
        /// <param name="primitiveTypeToFormattersMapping">The dictionary containing mappings from type to primitive type formatters which are used to .NET primitives.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="primitiveTypeToFormattersMapping" /> is null.</exception>
        public PrimitiveTypeInstructor(IDictionary<Type, IPrimitiveTypeFormatter> primitiveTypeToFormattersMapping)
        {
            primitiveTypeToFormattersMapping.MustNotBeNull(nameof(primitiveTypeToFormattersMapping));

            _primitiveTypeToFormattersMapping = primitiveTypeToFormattersMapping;
        }

        /// <summary>
        ///     Checks if there is a primitive type mapper for the specified actual type.
        /// </summary>
        public bool AppliesToObject(object @object, Type actualType, Type referencedType)
        {
            return _primitiveTypeToFormattersMapping.ContainsKey(actualType);
        }

        /// <summary>
        ///     Serializes the specified .NET primitive value using a primitive type formatter.
        /// </summary>
        /// <param name="serializationContext">The serialization context of the primitive value.</param>
        /// <exception cref="KeyNotFoundException">Thrown when no primitive type formatter could be found in the internal dictionary using the actual type as key.</exception>
        public void Serialize(JsonSerializationContext serializationContext)
        {
            var typeFormatter = _primitiveTypeToFormattersMapping[serializationContext.ActualType];
            var stringRepresentation = typeFormatter.FormatPrimitiveType(serializationContext.ObjectToBeSerialized);
            serializationContext.Writer.WritePrimitiveValue(stringRepresentation);
        }
    }
}