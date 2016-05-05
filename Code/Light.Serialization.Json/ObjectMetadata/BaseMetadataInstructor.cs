using System;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Json.BuilderInterfaces;
using Light.Serialization.Json.LowLevelWriting;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the base class for metadata instructors that handle object reference preservation and
    ///     type information.
    /// </summary>
    public abstract class BaseMetadataInstructor : IMetadataInstructor, ISetObjectReferencePreservationStatus, ISetTypeInfoSerializationStatus, ISetTypeToNameMapping
    {
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
        ///     Gets or sets the symbol that is used to mark the JSON document ID for a complex object.
        ///     This value defaults to to "$id".
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="value" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="value" /> contains only whitespace.</exception>
        public string IdSymbol
        {
            get { return _idSymbol; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _idSymbol = value;
            }
        }

        /// <summary>
        ///     Gets or sets the symbol that is used to mark a reference to another complex JSON object within the document.
        ///     This value defaults to "$ref".
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="value" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="value" /> contains only whitespace.</exception>
        public string ReferenceSymbol
        {
            get { return _referenceSymbol; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _referenceSymbol = value;
            }
        }

        /// <summary>
        ///     Gets or sets the symbol that is used to mark the type of a complex JSON object.
        ///     This value defaults to "$type".
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="value" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="value" /> contains only whitespace.</exception>
        public string ConcreteTypeSymbol
        {
            get { return _concreteTypeSymbol; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _concreteTypeSymbol = value;
            }
        }

        /// <summary>
        ///     Gets or sets the symbol that is used to mark the name of a generic type.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="value" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="value" /> contains only whitespace.</exception>
        public string GenericTypeNameSymbol
        {
            get { return _genericTypeNameSymbol; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _genericTypeNameSymbol = value;
            }
        }

        /// <summary>
        ///     Gets or sets the symbol that is used to mark the collection of type arguments for a generic type.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="value" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="value" /> contains only whitespace.</exception>
        public string GenericTypeArgumentsSymbol
        {
            get { return _genericTypeArgumentsSymbol; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _genericTypeArgumentsSymbol = value;
            }
        }

        /// <summary>
        ///     Gets or sets the symbol that is used to mark the JSON string containing the actual type of a .NET array.
        /// </summary>
        public string ArrayTypeSymbol
        {
            get { return _arrayTypeSymbol; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _arrayTypeSymbol = value;
            }
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
        ///     complex JSON object if it is a generic type.
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
                WriteGenericTypeInformation(currentType, writer);
                return;
            }

            writer.WriteString(_typeToNameMapping.Map(currentType));
        }

        private void WriteGenericTypeInformation(Type currentType, IJsonWriter writer)
        {
            writer.BeginObject()
                  .WriteKey(_genericTypeNameSymbol, false)
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
                  .WriteKey(_genericTypeNameSymbol, false)
                  .WriteString(_typeToNameMapping.Map(typeof(Array)))
                  .WriteDelimiter()
                  .WriteKey(_arrayTypeSymbol, false);

            SerializeTypeInfoRecursively(arrayType.GetElementType(), writer);
                  
            writer.WriteDelimiter()
                  .WriteKey(_arrayRankSymbol, false)
                  .WritePrimitiveValue(arrayType.GetArrayRank().ToString())
                  .EndObject();
        }

        // ReSharper disable InconsistentNaming
        protected string _concreteTypeSymbol = JsonSymbols.DefaultConcreteTypeSymbol;
        protected string _genericTypeArgumentsSymbol = JsonSymbols.DefaultGenericTypeArgumentsSymbol;
        protected string _genericTypeNameSymbol = JsonSymbols.DefaultGenericTypeNameSymbol;
        protected string _idSymbol = JsonSymbols.DefaultIdSymbol;
        private bool _isSerializingObjectIds = true;
        private bool _isSerializingTypeInfo = true;
        protected string _referenceSymbol = JsonSymbols.DefaultReferenceSymbol;
        protected string _arrayTypeSymbol = JsonSymbols.DefaultArrayTypeSymbol;
        protected string _arrayRankSymbol = JsonSymbols.DefaultArrayRankSymbol;
        protected ITypeToNameMapping _typeToNameMapping;
        // ReSharper restore InconsistentNaming
    }
}