using System;
using System.Reflection;
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
        private readonly INameToTypeMapping _nameToTypeMapping;
        private string _concreteTypeSymbol = JsonSymbols.DefaultConcreteTypeSymbol;
        private string _genericTypeArgumentsSymbol = JsonSymbols.DefaultGenericTypeArgumentsSymbol;
        private string _genericTypeNameSymbol = JsonSymbols.DefaultGenericTypeNameSymbol;
        private string _idSymbol = JsonSymbols.DefaultIdSymbol;
        private string _referenceSymbol = JsonSymbols.DefaultReferenceSymbol;


        public ComplexObjectMetadataParser(INameToTypeMapping nameToTypeMapping)
        {
            nameToTypeMapping.MustNotBeNull(nameof(nameToTypeMapping));

            _nameToTypeMapping = nameToTypeMapping;
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

        public MetadataParseResult ParseMetadataSection(ref JsonToken currentToken, JsonDeserializationContext context)
        {
            currentToken.JsonType.MustBe(JsonTokenType.String, $"The {nameof(currentToken)} is not a JSON string.");

            var jsonReader = context.JsonReader;
            var targetType = context.RequestedType;
            var objectId = -1;

            for (var i = 0;; i++)
            {
                currentToken.MustBeComplexObjectKey();
                var tokenString = context.DeserializeToken<string>(currentToken);

                // $ref - refernce to another object in the JSON document
                if (tokenString == _referenceSymbol)
                {
                    if (i > 0)
                        throw new JsonDocumentException($"The {_referenceSymbol} is used in a wrong fashion: it should be the only metadata symbol when an object points to another one.", currentToken);

                    var numberToken = jsonReader.ReadAndExpectPairDelimiterToken()
                                                .ReadAndExpectNumber();
                    jsonReader.ReadAndExpectEndOfObject("A reference to another object must be the only Key-Value-Pair in a complex JSON object.");
                    objectId = context.DeserializeToken<int>(numberToken);
                    object retrievedObject;
                    if (context.DeserializedObjects.TryGetValue(objectId, out retrievedObject))
                        return MetadataParseResult.FromRetrievedObject(objectId, retrievedObject);

                    return MetadataParseResult.FromDeferredReference(objectId);
                }

                // $id - defining an id for this complex JSON object
                if (tokenString == _idSymbol)
                {
                    var numberToken = jsonReader.ReadAndExpectPairDelimiterToken()
                                                .ReadAndExpectNumber();

                    objectId = context.DeserializeToken<int>(numberToken);
                }

                // $type - type information for this complex JSON object
                else if (tokenString == _concreteTypeSymbol)
                {
                    jsonReader.ReadAndExpectPairDelimiterToken();
                    targetType = ParseType(context);
                }

                // TODO: update currentToken for next loop cycle
                currentToken = jsonReader.ReadNextToken();
                if (currentToken.JsonType == JsonTokenType.EndOfObject)
                    return MetadataParseResult.FromMetadata(targetType, ReferencePreservationInfo.FromNewObjectInJsonDocument(objectId));

                currentToken.MustBeValueDelimiterInObject();
            }
        }

        private Type ParseType(JsonDeserializationContext context)
        {
            var jsonReader = context.JsonReader;

            var currentToken = jsonReader.ReadNextToken();

            if (currentToken.JsonType == JsonTokenType.String)
            {
                var jsonTypeName = context.DeserializeToken<string>(currentToken);
                return _nameToTypeMapping.Map(jsonTypeName);
            }

            if (currentToken.JsonType != JsonTokenType.BeginOfObject)
                throw new JsonDocumentException($"Expected JSON string or begin of complex JSON object in the metadata type description, but found {currentToken}.", currentToken);

            currentToken = jsonReader.ReadNextToken();
            currentToken.MustBeComplexObjectKey();
            var label = context.DeserializeToken<string>(currentToken);
            if (label != _genericTypeNameSymbol)
                throw new JsonDocumentException($"Expected {_genericTypeNameSymbol} in metadata section of complex JSON object, but found {label}.", currentToken);

            currentToken = jsonReader.ReadAndExpectPairDelimiterToken()
                                     .ReadNextToken();

            if (currentToken.JsonType != JsonTokenType.String)
                throw new JsonDocumentException($"Expected name of generic type in metadata section of complex JSON object, but found {currentToken}.", currentToken);

            var genericTypeName = context.DeserializeToken<string>(currentToken);
            var genericType = _nameToTypeMapping.Map(genericTypeName);

            currentToken = jsonReader.ReadAndExpectValueDelimiterToken()
                                     .ReadNextToken();

            currentToken.MustBeComplexObjectKey();
            label = context.DeserializeToken<string>(currentToken);
            if (label != _genericTypeArgumentsSymbol)
                throw new JsonDocumentException($"Expected {_genericTypeArgumentsSymbol} in metadata section of complex JSON object, but found {label}.", currentToken);

            jsonReader.ReadAndExpectPairDelimiterToken()
                      .ReadAndExpectBeginOfArray();

            var genericTypeArguments = new Type[genericType.GetTypeInfo().GenericTypeParameters.Length];
            for (var i = 0; i < genericTypeArguments.Length; i++)
            {
                genericTypeArguments[i] = ParseType(context);
                if (i < genericTypeArguments.Length - 1)
                    jsonReader.ReadAndExpectValueDelimiterToken();
                else
                {
                    jsonReader.ReadAndExpectedEndOfArray();
                    break;
                }
            }
            jsonReader.ReadAndExpectEndOfObject();

            return genericType.MakeGenericType(genericTypeArguments);
        }
    }
}