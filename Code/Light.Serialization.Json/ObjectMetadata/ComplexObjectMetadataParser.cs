using System;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents an object that parses the metadata section of complex JSON objects.
    /// </summary>
    public sealed class ComplexObjectMetadataParser : IMetadataParser
    {
        private string _idSymbol = JsonSymbols.DefaultIdSymbol;
        private string _referenceSymbol = JsonSymbols.DefaultReferenceSymbol;

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

        public MetadataParseResult ParseMetadataSection(ref JsonToken firstLabelToken, JsonDeserializationContext context)
        {
            firstLabelToken.JsonType.MustBe(JsonTokenType.String, $"The {nameof(firstLabelToken)} is not a JSON string.");

            var jsonReader = context.JsonReader;
            var metadataParseResult = new MetadataParseResult(context.RequestedType);
            var firstLabelString = context.DeserializeToken<string>(firstLabelToken);

            // Check if the first label of the complex json object marks a reference to another object
            if (firstLabelString == _referenceSymbol)
            {
                var numberToken = jsonReader.ReadAndExpectPairDelimiterToken()
                                            .ReadAndExpectNumber();
                jsonReader.ReadAndExpectEndOfObject();
                var objectId = context.DeserializeToken<int>(numberToken);
                object returnedObject;
                if (context.DeserializedObjects.TryGetValue(objectId, out returnedObject) == false)
                {
                    // TODO: here, the target object was not deserialized before. Setting this object must be enqueued for later
                    // We should communicate this via the metadata parse result
                    throw new NotImplementedException("The requested object was not deserialized before.");
                }

                metadataParseResult.ObjectFromCache = returnedObject;
                return metadataParseResult;
            }

            // If not, check if the first label is the id symbol
            if (firstLabelString == _idSymbol)
            {
                
            }

            throw new NotImplementedException();
        }
    }
}