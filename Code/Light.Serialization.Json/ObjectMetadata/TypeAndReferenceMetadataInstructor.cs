using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Json.LowLevelWriting;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents an IObjectMetadataInstructor that adds the type
    /// </summary>
    public sealed class TypeAndReferenceMetadataInstructor : IObjectMetadataInstructor, ISetObjectReferencePreservationStatus, ISetTypeInfoSerializationStatus, IClearSerializedObjectsCache
    {
        private readonly List<object> _serializedObjects = new List<object>();
        private readonly ITypeToNameMapping _typeToNameMapping;
        private string _concreteTypeSymbol = JsonSymbols.DefaultConcreteTypeSymbol;
        private string _genericTypeArgumentsSymbol = JsonSymbols.DefaultGenericTypeArgumentsSymbol;
        private string _genericTypeNameSymbol = JsonSymbols.DefaultGenericTypeNameSymbol;
        private string _idSymbol = JsonSymbols.DefaultIdSymbol;
        private string _referenceSymbol = JsonSymbols.DefaultReferenceSymbol;

        /// <summary>
        ///     Creates a new instance of <see cref="TypeAndReferenceMetadataInstructor" />.
        /// </summary>
        /// <param name="typeToNameMapping">The object that is used to map from .NET types to JSON type names.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeToNameMapping" /> is null.</exception>
        public TypeAndReferenceMetadataInstructor(ITypeToNameMapping typeToNameMapping)
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
        ///     Clears the list of serialized objects - this should be done when a new object graph is serialized.
        /// </summary>
        public void ClearSerializedObjects()
        {
            _serializedObjects.Clear();
        }

        /// <summary>
        ///     Serializes the JSON document ID and the type name of the specified object.
        /// </summary>
        /// <param name="serializationContext">The serialization context for the object to be serialized.</param>
        /// <returns>True if the object was not serialized before, else false.</returns>
        public bool SerializeMetadata(JsonSerializationContext serializationContext)
        {
            var writer = serializationContext.Writer;

            if (IsSerializingObjectIds)
            {
                var indexOfObject = _serializedObjects.IndexOf(serializationContext.ObjectToBeSerialized);

                if (indexOfObject != -1)
                {
                    writer.WriteKey(_referenceSymbol);
                    writer.WritePrimitiveValue(indexOfObject.ToString());
                    return false;
                }

                _serializedObjects.Add(serializationContext.ObjectToBeSerialized);
                writer.WriteKey(_idSymbol);
                writer.WritePrimitiveValue((_serializedObjects.Count - 1).ToString());
                writer.WriteDelimiter();
            }

            if (IsSerializingTypeInfo)
            {
                writer.WriteKey(_concreteTypeSymbol);
                SerializeTypeInfo(serializationContext.ActualType, writer);
                writer.WriteDelimiter();
            }
            return true;
        }

        /// <summary>
        ///     Gets or sets the value indicating wheter object ids and references to objects are serialized in the metadata section of a complex JSON object.
        /// </summary>
        public bool IsSerializingObjectIds { get; set; } = true;

        /// <summary>
        ///     Gets or sets the value indicating type information is serialized in the metadata section of a complex JSON object.
        /// </summary>
        public bool IsSerializingTypeInfo { get; set; } = true;

        private void SerializeTypeInfo(Type currentType, IJsonWriter writer)
        {
            if (currentType.IsConstructedGenericType == false)
            {
                var typeName = _typeToNameMapping.Map(currentType);
                writer.WritePrimitiveValue(typeName);
                return;
            }

            writer.BeginObject();

            writer.WriteKey(_genericTypeNameSymbol);
            var genericTypeName = _typeToNameMapping.Map(currentType);
            writer.WritePrimitiveValue(genericTypeName);
            writer.WriteDelimiter();

            writer.WriteKey(_genericTypeArgumentsSymbol);
            writer.BeginArray();

            var genericTypeArguments = currentType.GenericTypeArguments;
            for (var i = 0; i < genericTypeArguments.Length; i++)
            {
                SerializeTypeInfo(genericTypeArguments[i], writer);
                if (i < genericTypeArguments.Length - 1)
                    writer.WriteDelimiter();
                else
                    writer.EndArray();
            }

            writer.EndObject();
        }
    }
}