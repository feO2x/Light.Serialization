using System;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Json.BuilderInterfaces;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents a metadata instructor that writes Object IDs in a JSON string as the first item of a JSON array.
    /// </summary>
    public sealed class CollectionReferenceMetadataInstructor : IMetadataInstructor, ISetObjectReferencePreservationStatus
    {
        private string _idSymbol = JsonSymbols.DefaultIdSymbol;
        private bool _isSerializingObjectIds = true;
        private string _referenceSymbol = JsonSymbols.DefaultReferenceSymbol;

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
        ///     Serializes the JSON object ID of the specified collection.
        /// </summary>
        /// <param name="serializationContext">The serialization context of the collection.</param>
        /// <returns>True if the items of the collection should be serialized after the metadata section, else false.</returns>
        public bool SerializeMetadata(JsonSerializationContext serializationContext)
        {
            var writer = serializationContext.Writer;
            if (_isSerializingObjectIds == false)
                return true;

            var indexOfObject = serializationContext.SerializedObjects.GetIndexOfSame(serializationContext.ObjectToBeSerialized);
            if (indexOfObject != -1)
            {
                writer.WritePrimitiveValue($"\"{_referenceSymbol}:{indexOfObject}\"");
                return false;
            }

            serializationContext.SerializedObjects.Add(serializationContext.ObjectToBeSerialized);
            writer.WritePrimitiveValue($"\"{_idSymbol}:{serializationContext.SerializedObjects.Count - 1}\"");
            writer.WriteDelimiter();
            return true;
        }

        /// <summary>
        ///     Gets or sets value indicating whether object ids and references to objects are serialized in the metadata section.
        /// </summary>
        public bool IsSerializingObjectIds
        {
            get { return _isSerializingObjectIds; }
            set { _isSerializingObjectIds = value; }
        }
    }
}