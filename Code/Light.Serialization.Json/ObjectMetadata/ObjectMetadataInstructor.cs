using System;
using Light.GuardClauses;
using Light.Serialization.Json.LowLevelWriting;
using Light.Serialization.Json.WriterInstructors;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents an <see cref="IMetadataInstructor" /> that serializes ids and type information in the metadata section of a complex JSON object.
    ///     Additionaly, this instructor can serialize type information, which is needed in the <see cref="TypeAndTypeInfoInstructor" />.
    /// </summary>
    public sealed class ObjectMetadataInstructor : BaseMetadataInstructor, ITypeMetadataInstructor
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ObjectMetadataInstructor" />.
        /// </summary>
        /// <param name="typeToNameMapping">The object that is used to map from .NET types to JSON type names.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeToNameMapping" /> is null.</exception>
        public ObjectMetadataInstructor(ITypeToNameMapping typeToNameMapping) : base(typeToNameMapping) { }

        void ITypeMetadataInstructor.SerializeType(Type type, IJsonWriter writer)
        {
            type.MustNotBeNull(nameof(type));
            writer.MustNotBeNull(nameof(type));

            SerializeTypeInfoRecursively(type, writer);
        }


        protected override void SerializeReferenceId(int referenceId, JsonSerializationContext serializationContext)
        {
            serializationContext.Writer
                                .WriteKey(_referenceSymbol, false)
                                .WritePrimitive(referenceId.ToString());
        }

        protected override void SerializeObjectId(int documentIdForObject, JsonSerializationContext serializationContext)
        {
            serializationContext.Writer
                                .WriteKey(_idSymbol, false)
                                .WritePrimitive(documentIdForObject.ToString())
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