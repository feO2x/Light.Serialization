using System;
using Light.Serialization.Json.WriterInstructors;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents an <see cref="IMetadataInstructor"/> that writes Object IDs and collection type information as the first items of a JSON array.
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
                                .WritePrimitive(referenceId.ToString());
        }

        protected override void SerializeObjectId(int documentIdForObject, JsonSerializationContext serializationContext)
        {
            serializationContext.Writer
                                .WriteString(_idSymbol)
                                .WriteDelimiter()
                                .WritePrimitive(documentIdForObject.ToString())
                                .WriteDelimiter();
        }

        protected override void SerializeTypeInfo(JsonSerializationContext serializationContext)
        {
            var writer = serializationContext.Writer;

            writer.WriteString(_concreteTypeSymbol)
                  .WriteDelimiter();

            var actualType = serializationContext.ActualType;
            if (actualType.IsArray)
                SerializeArrayInfo(serializationContext);
            else
                SerializeTypeInfoRecursively(actualType, writer);

            writer.WriteDelimiter();
        }

        private void SerializeArrayInfo(JsonSerializationContext serializationContext)
        {
            var writer = serializationContext.Writer;
            var actualType = serializationContext.ActualType;

            writer.BeginObject()
                  .WriteKey(_typeNameSymbol, false)
                  .WriteString(_typeToNameMapping.Map(typeof(Array)))
                  .WriteDelimiter()
                  .WriteKey(_arrayTypeSymbol, false);

            SerializeTypeInfoRecursively(actualType.GetElementType(), writer);

            var arrayRank = actualType.GetArrayRank();
            var array = (Array) serializationContext.ObjectToBeSerialized;
            if (arrayRank > 1)
            {
                writer.WriteDelimiter()
                      .WriteKey(ArrayRankSymbol, false)
                      .WritePrimitive(arrayRank.ToString())
                      .WriteDelimiter()
                      .WriteKey(_arrayLengthSymbol, false)
                      .BeginArray();

                for (var i = 0; i < arrayRank; i++)
                {
                    writer.WritePrimitive(array.GetLength(i).ToString());
                    if (i < arrayRank - 1)
                        writer.WriteDelimiter();
                    else
                        break;
                }

                writer.EndArray()
                      .EndObject();
            }
            else
            {
                writer.WriteDelimiter()
                      .WriteKey(_arrayLengthSymbol, false)
                      .WritePrimitive(array.Length.ToString())
                      .EndObject();
            }
        }
    }
}