using System;
using Light.GuardClauses;
using Light.Serialization.Json.LowLevelWriting;
using Light.Serialization.Json.WriterInstructors;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents an Metadata Instructor that serializes ids and type information in the metadata section of a complex JSON object.
    /// </summary>
    public sealed class ObjectMetadataInstructor : BaseMetadataInstructor, IObjectMetadataInstructor
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ObjectMetadataInstructor" />.
        /// </summary>
        /// <param name="typeToNameMapping">The object that is used to map from .NET types to JSON type names.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeToNameMapping" /> is null.</exception>
        public ObjectMetadataInstructor(ITypeToNameMapping typeToNameMapping) : base(typeToNameMapping) { }


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

        void IObjectMetadataInstructor.SerializeType(Type type, IJsonWriter writer)
        {
            type.MustNotBeNull(nameof(type));
            writer.MustNotBeNull(nameof(type));

            SerializeTypeInfoRecursively(type, writer);
        }
    }
}