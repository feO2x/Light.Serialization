using System;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents a metadata instructor that writes Object IDs and collection type information as the first items of a JSON array.
    /// </summary>
    public sealed class ArrayMetadataInstructor : BaseMetadataInstructor
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ArrayMetadataInstructor" />.
        /// </summary>
        /// <param name="typeToNameMapping">The object that is used to map from .NET types to JSON type names.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeToNameMapping" /> is null.</exception>
        public ArrayMetadataInstructor(ITypeToNameMapping typeToNameMapping) : base(typeToNameMapping) { }

        protected override void SerializeReferenceId(int referenceId, JsonSerializationContext serializationContext)
        {
            serializationContext.Writer
                                .WriteString(_referenceSymbol)
                                .WriteDelimiter()
                                .WritePrimitiveValue(referenceId.ToString());
        }

        protected override void SerializeObjectId(int documentIdForObject, JsonSerializationContext serializationContext)
        {
            serializationContext.Writer
                                .WriteString(_idSymbol)
                                .WriteDelimiter()
                                .WritePrimitiveValue(documentIdForObject.ToString())
                                .WriteDelimiter();
        }

        protected override void SerializeTypeInfo(JsonSerializationContext serializationContext)
        {
            serializationContext.Writer
                                .WriteString(_concreteTypeSymbol)
                                .WriteDelimiter();

            SerializeTypeInfoRecursively(serializationContext.ActualType, serializationContext.Writer);
            serializationContext.Writer.WriteDelimiter();
        }
    }
}