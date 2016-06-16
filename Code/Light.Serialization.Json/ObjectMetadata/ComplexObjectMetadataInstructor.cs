using System;
using Light.Serialization.Json.WriterInstructors;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents an Metadata Instructor that serializes ids and type information in the metadata section of a complex JSON object.
    /// </summary>
    public sealed class ComplexObjectMetadataInstructor : BaseMetadataInstructor
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ComplexObjectMetadataInstructor" />.
        /// </summary>
        /// <param name="typeToNameMapping">The object that is used to map from .NET types to JSON type names.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeToNameMapping" /> is null.</exception>
        public ComplexObjectMetadataInstructor(ITypeToNameMapping typeToNameMapping) : base(typeToNameMapping) { }


        protected override void SerializeReferenceId(int referenceId, JsonSerializationContext serializationContext)
        {
            serializationContext.Writer
                                .WriteKey(_referenceSymbol, false)
                                .WritePrimitiveValue(referenceId.ToString());
        }

        protected override void SerializeObjectId(int documentIdForObject, JsonSerializationContext serializationContext)
        {
            serializationContext.Writer
                                .WriteKey(_idSymbol, false)
                                .WritePrimitiveValue(documentIdForObject.ToString())
                                .WriteDelimiter();
        }

        protected override void SerializeTypeInfo(JsonSerializationContext serializationContext)
        {
            serializationContext.Writer.WriteKey(_concreteTypeSymbol, false);
            SerializeTypeInfoRecursively(serializationContext.ActualType, serializationContext.Writer);
            serializationContext.Writer.WriteDelimiter();
        }
    }
}