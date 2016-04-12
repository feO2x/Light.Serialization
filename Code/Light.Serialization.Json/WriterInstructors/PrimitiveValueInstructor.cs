using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.BuilderInterfaces;
using Light.Serialization.Json.PrimitiveTypeFormatters;

namespace Light.Serialization.Json.WriterInstructors
{
    /// <summary>
    ///     Represents an <see cref="IJsonWriterInstructor" /> that serializes primitive types (such as int, double, bool, string) to
    ///     corresponding JSON values by using primitive type formatters.
    /// </summary>
    public sealed class PrimitiveValueInstructor : IJsonWriterInstructor, ISetPrimitiveTypeFormatters
    {
        private IDictionary<Type, IPrimitiveTypeFormatter> _primitiveTypeFormattersMapping;

        /// <summary>
        ///     Creates a new instance of <see cref="PrimitiveValueInstructor" />.
        /// </summary>
        /// <param name="primitiveTypeFormattersMapping">The dictionary containing mappings from type to primitive type formatters which are used to .NET primitives.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="primitiveTypeFormattersMapping" /> is null.</exception>
        public PrimitiveValueInstructor(IDictionary<Type, IPrimitiveTypeFormatter> primitiveTypeFormattersMapping)
        {
            primitiveTypeFormattersMapping.MustNotBeNull(nameof(primitiveTypeFormattersMapping));

            _primitiveTypeFormattersMapping = primitiveTypeFormattersMapping;
        }

        /// <summary>
        ///     Checks if there is a primitive type mapper for the specified actual type.
        /// </summary>
        public bool IsSuitableFor(object @object, Type actualType)
        {
            return _primitiveTypeFormattersMapping.ContainsKey(actualType);
        }

        /// <summary>
        ///     Serializes the specified .NET primitive value using a primitive type formatter.
        /// </summary>
        /// <param name="serializationContext">The serialization context of the primitive value.</param>
        /// <exception cref="KeyNotFoundException">Thrown when no primitive type formatter could be found in the internal dictionary using the actual type as key.</exception>
        public void Serialize(JsonSerializationContext serializationContext)
        {
            var typeFormatter = _primitiveTypeFormattersMapping[serializationContext.ActualType];
            var stringRepresentation = typeFormatter.FormatPrimitiveType(serializationContext.ObjectToBeSerialized);
            serializationContext.Writer.WritePrimitiveValue(stringRepresentation);
        }

        /// <summary>
        ///     Gets or sets the dictionary containing mappings from types to primitive formatters.
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