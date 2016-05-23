using System;
using System.Reflection;
using Light.GuardClauses;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents an object that parses the metadata section of complex JSON objects.
    /// </summary>
    public sealed class ComplexObjectMetadataParser : BaseMetadata, IMetadataParser
    {
        private readonly INameToTypeMapping _nameToTypeMapping;

        /// <summary>
        ///     Creates a new instance of ComplexObjectMetadataParser.
        /// </summary>
        /// <param name="nameToTypeMapping">The object that can map JSON type names to .NET types.</param>
        public ComplexObjectMetadataParser(INameToTypeMapping nameToTypeMapping)
        {
            nameToTypeMapping.MustNotBeNull(nameof(nameToTypeMapping));

            _nameToTypeMapping = nameToTypeMapping;
        }

        /// <summary>
        ///     Parses the metadata section of a complex JSON object given the first label token and the deserialization context.
        /// </summary>
        /// <param name="currentToken">The JSON token that represents the first label in a complex JSON object.</param>
        /// <param name="context">The deserialization context for the complex JSON object.</param>
        /// <returns>The metadata parse result.</returns>
        public MetadataParseResult ParseMetadataSection(ref JsonToken currentToken, JsonDeserializationContext context)
        {
            currentToken.JsonType.MustBe(JsonTokenType.String, $"The {nameof(currentToken)} is not a JSON string.");

            var jsonReader = context.JsonReader;
            var typeToConstruct = context.RequestedType;
            var objectId = -1;

            // Loop through the metadata section of the complex JSON object
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
                    typeToConstruct = ParseType(context);
                }

                // No metadata token found - this means the end of the metadata section in the complex JSON object was reached
                else
                {
                    var referencePreservationInfo = objectId == -1 ? ReferencePreservationInfo.Empty : ReferencePreservationInfo.FromNewObjectInJsonDocument(objectId);
                    return MetadataParseResult.FromMetadata(typeToConstruct, referencePreservationInfo);
                }

                // Update currentToken for next loop cycle
                currentToken = jsonReader.ReadNextToken();
                if (currentToken.JsonType == JsonTokenType.EndOfObject)
                    return MetadataParseResult.FromMetadata(typeToConstruct, ReferencePreservationInfo.FromNewObjectInJsonDocument(objectId));

                currentToken.MustBeValueDelimiterInObject();

                currentToken = jsonReader.ReadNextToken();
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