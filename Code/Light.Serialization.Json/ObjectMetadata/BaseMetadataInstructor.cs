using System;
using Light.GuardClauses;
using Light.Serialization.Json.BuilderHelpers;
using Light.Serialization.Json.FrameworkExtensions;
using Light.Serialization.Json.LowLevelWriting;
using Light.Serialization.Json.WriterInstructors;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the base class for metadata instructors that handle object reference preservation and type information.
    /// </summary>
    public abstract class BaseMetadataInstructor : BaseMetadata, IMetadataInstructor, ISetObjectReferencePreservationStatus, ISetTypeInfoSerializationStatus, ISetTypeToNameMapping
    {
        private bool _isSerializingObjectIds = true;
        private bool _isSerializingTypeInfo = true;

        // ReSharper disable once InconsistentNaming
        protected ITypeToNameMapping _typeToNameMapping;

        /// <summary>
        ///     Creates a new instance of <see cref="BaseMetadataInstructor" />.
        /// </summary>
        /// <param name="typeToNameMapping">The object that is used to map from .NET types to JSON type names.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeToNameMapping" /> is null.</exception>
        protected BaseMetadataInstructor(ITypeToNameMapping typeToNameMapping)
        {
            typeToNameMapping.MustNotBeNull(nameof(typeToNameMapping));

            _typeToNameMapping = typeToNameMapping;
        }

        /// <summary>
        ///     Serializes the JSON object ID and the type name of the specified object.
        /// </summary>
        /// <param name="serializationContext">The serialization context for the object to be serialized.</param>
        /// <returns>True if the object was not serialized before, else false.</returns>
        public bool SerializeMetadata(JsonSerializationContext serializationContext)
        {
            if (_isSerializingObjectIds)
            {
                var indexOfObject = serializationContext.SerializedObjects.GetIndexOfSame(serializationContext.ObjectToBeSerialized);

                if (indexOfObject != -1)
                {
                    SerializeReferenceId(indexOfObject, serializationContext);
                    return false;
                }

                serializationContext.SerializedObjects.Add(serializationContext.ObjectToBeSerialized);
                SerializeObjectId(serializationContext.SerializedObjects.Count - 1, serializationContext);
            }

            if (_isSerializingTypeInfo)
                SerializeTypeInfo(serializationContext);

            return true;
        }

        /// <summary>
        ///     Gets or sets the value indicating wheter object ids and references to objects are serialized in the metadata section of a complex JSON object.
        /// </summary>
        public bool IsSerializingObjectIds
        {
            get { return _isSerializingObjectIds; }
            set { _isSerializingObjectIds = value; }
        }

        /// <summary>
        ///     Gets or sets the value indicating type information is serialized in the metadata section of a complex JSON object.
        /// </summary>
        public bool IsSerializingTypeInfo
        {
            get { return _isSerializingTypeInfo; }
            set { _isSerializingTypeInfo = value; }
        }

        /// <summary>
        ///     Gets or sets the object used to map from .NET types to JSON names.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public ITypeToNameMapping TypeToNameMapping
        {
            get { return _typeToNameMapping; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _typeToNameMapping = value;
            }
        }

        /// <summary>
        ///     Writes the reference id ($ref) in the metadata section of a complex JSON object or JSON array.
        /// </summary>
        /// <param name="referenceId">The id pointing to another object in the JSON document.</param>
        /// <param name="serializationContext">The serialization context.</param>
        protected abstract void SerializeReferenceId(int referenceId, JsonSerializationContext serializationContext);

        /// <summary>
        ///     Writes the object id ($id) in the metadata section of a complex JSON object or JSON array.
        /// </summary>
        /// <param name="documentIdForObject">The JSON document id for the object.</param>
        /// <param name="serializationContext">The serialization context.</param>
        protected abstract void SerializeObjectId(int documentIdForObject, JsonSerializationContext serializationContext);

        /// <summary>
        ///     Writes information about the type to the metadata section of a complex JSON object or JSON array.
        /// </summary>
        /// <param name="serializationContext">The serialization context.</param>
        protected abstract void SerializeTypeInfo(JsonSerializationContext serializationContext);

        /// <summary>
        ///     Serializes the specified type as a JSON string if it is a non-generic type, or as a
        ///     complex JSON object if it is a generic type without a simple name.
        /// </summary>
        /// <param name="currentType">The type to be written to the metadata section.</param>
        /// <param name="writer">The object that writes the JSON document.</param>
        protected void SerializeTypeInfoRecursively(Type currentType, IJsonWriter writer)
        {
            if (currentType.IsArray)
            {
                WriteArrayTypeInformation(currentType, writer);
                return;
            }

            if (currentType.IsConstructedGenericType)
            {
                var jsonNameForResolvedGenericType = _typeToNameMapping.TryMap(currentType);
                if (jsonNameForResolvedGenericType != null)
                {
                    writer.WriteString(jsonNameForResolvedGenericType);
                    return;
                }

                WriteGenericTypeInformation(currentType, writer);
                return;
            }

            writer.WriteString(_typeToNameMapping.Map(currentType));
        }

        private void WriteGenericTypeInformation(Type currentType, IJsonWriter writer)
        {
            writer.BeginObject()
                  .WriteKey(_typeNameSymbol, false)
                  .WriteString(_typeToNameMapping.Map(currentType.GetGenericTypeDefinition()))
                  .WriteDelimiter()
                  .WriteKey(_genericTypeArgumentsSymbol, false)
                  .BeginArray();

            var genericTypeArguments = currentType.GenericTypeArguments;
            for (var i = 0; i < genericTypeArguments.Length; i++)
            {
                SerializeTypeInfoRecursively(genericTypeArguments[i], writer);
                if (i < genericTypeArguments.Length - 1)
                    writer.WriteDelimiter();
                else
                {
                    writer.EndArray();
                    break;
                }
            }

            writer.EndObject();
        }

        private void WriteArrayTypeInformation(Type arrayType, IJsonWriter writer)
        {
            writer.BeginObject()
                  .WriteKey(_typeNameSymbol, false)
                  .WriteString(_typeToNameMapping.Map(typeof(Array)))
                  .WriteDelimiter()
                  .WriteKey(_arrayTypeSymbol, false);

            SerializeTypeInfoRecursively(arrayType.GetElementType(), writer);

            var arrayRank = arrayType.GetArrayRank();
            if (arrayRank > 1)
            {
                writer.WriteDelimiter()
                      .WriteKey(ArrayRankSymbol, false)
                      .WritePrimitive(arrayType.GetArrayRank().ToString());
            }

            writer.EndObject();
        }
    }
}