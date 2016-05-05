using System;
using Light.GuardClauses;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents a metadata instructor that writes Object IDs and collection type information as the first items of a JSON array.
    /// </summary>
    public sealed class ArrayMetadataInstructor : BaseMetadataInstructor
    {
        private string _arrayLengthSymbol = JsonSymbols.DefaultArrayLengthSymbol;

        /// <summary>
        ///     Creates a new instance of <see cref="ArrayMetadataInstructor" />.
        /// </summary>
        /// <param name="typeToNameMapping">The object that is used to map from .NET types to JSON type names.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeToNameMapping" /> is null.</exception>
        public ArrayMetadataInstructor(ITypeToNameMapping typeToNameMapping) : base(typeToNameMapping) { }

        /// <summary>
        ///     Gets or sets the symbol that is used to mark the JSON number describing the length of a .NET array.
        /// </summary>
        public string ArrayLengthSymbol
        {
            get { return _arrayLengthSymbol; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _arrayLengthSymbol = value;
            }
        }

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
            var writer = serializationContext.Writer;

            writer.WriteString(_concreteTypeSymbol)
                  .WriteDelimiter();

            var actualType = serializationContext.ActualType;
            if (actualType.IsArray)
            {
                writer.BeginObject()
                      .WriteKey(_genericTypeNameSymbol, false)
                      .WriteString(_typeToNameMapping.Map(typeof(Array)))
                      .WriteDelimiter()
                      .WriteKey(_arrayTypeSymbol, false);

                SerializeTypeInfoRecursively(actualType.GetElementType(), writer);

                writer.WriteDelimiter()
                      .WriteKey(_arrayRankSymbol, false)
                      .WritePrimitiveValue(actualType.GetArrayRank().ToString())
                      .WriteDelimiter()
                      .WriteKey(_arrayLengthSymbol, false)
                      .WritePrimitiveValue(GetLengthOfArray(serializationContext.ObjectToBeSerialized))
                      .EndObject();
            }
            else
                SerializeTypeInfoRecursively(serializationContext.ActualType, serializationContext.Writer);

            serializationContext.Writer.WriteDelimiter();
        }

        private static string GetLengthOfArray(object objectToBeSerialized)
        {
            var array = objectToBeSerialized as Array;
            return array == null ? "-1" : array.Length.ToString();
        }
    }
}