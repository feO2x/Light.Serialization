using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents an IObjectMetadataInstructor that adds the type
    /// </summary>
    public sealed class TypeAndReferenceMetadataInstructor : IObjectMetadataInstructor
    {
        private readonly string _referenceSymbol = JsonSymbols.DefaultReferenceSymbol;
        private readonly List<object> _serializedObjects = new List<object>();
        private readonly ITypeToNameMapping _typeToNameMapping;
        private string _concreteTypeSymbol = JsonSymbols.DefaultConcreteTypeSymbol;
        private string _idSymbol = JsonSymbols.DefaultIdSymbol;

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
                _idSymbol = value;
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
        ///     Serializes the JSON document ID and the type name of the specified object.
        /// </summary>
        /// <param name="serializationContext">The serialization context for the object to be serialized.</param>
        /// <returns>True if the object was not serialized before, else false.</returns>
        public bool SerializeMetadata(JsonSerializationContext serializationContext)
        {
            var writer = serializationContext.Writer;
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

            writer.WriteKey(_concreteTypeSymbol);
            var jsonTypeName = _typeToNameMapping.Map(serializationContext.ActualType);
            writer.WritePrimitiveValue(jsonTypeName);
            writer.WriteDelimiter();

            return true;
        }
    }
}