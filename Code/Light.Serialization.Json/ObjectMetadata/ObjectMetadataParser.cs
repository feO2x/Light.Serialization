using System;
using Light.GuardClauses;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents an object that parses the metadata section of complex JSON objects.
    /// </summary>
    public sealed class ObjectMetadataParser : BaseMetadataParser, IObjectMetadataParser
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ObjectMetadataParser"/>.
        /// </summary>
        /// <param name="nameToTypeMapping">The object that can map JSON type names to .NET types.</param>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="nameToTypeMapping" /> is null.</exception>
        public ObjectMetadataParser(INameToTypeMapping nameToTypeMapping)
            : base(nameToTypeMapping) { }

        /// <summary>
        ///     Parses the metadata section of a complex JSON object given the first label token and the deserialization context.
        /// </summary>
        /// <param name="currentToken">The JSON token that represents the first label in a complex JSON object.</param>
        /// <param name="context">The deserialization context for the complex JSON object.</param>
        /// <returns>The metadata parse result.</returns>
        public ObjectMetadataParseResult ParseMetadataSection(ref JsonToken currentToken, JsonDeserializationContext context)
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
                    if (context.ObjectReferencePreserver.TryGetDeserializedObject(objectId, out retrievedObject))
                        return ObjectMetadataParseResult.FromRetrievedObject(objectId, retrievedObject);

                    return ObjectMetadataParseResult.FromDeferredReference(objectId);
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
                    var parsedType = ParseType(context);
                    if (_isIgnoringMetadataTypeInformation == false)
                        typeToConstruct = parsedType;
                }

                // No metadata token found - this means the end of the metadata section in the complex JSON object was reached
                else
                {
                    var referencePreservationInfo = objectId == -1 ? ReferencePreservationInfo.Empty : ReferencePreservationInfo.FromNewObjectInJsonDocument(objectId);
                    return ObjectMetadataParseResult.FromMetadata(typeToConstruct, referencePreservationInfo);
                }

                // Update currentToken for next loop cycle
                currentToken = jsonReader.ReadNextToken();
                if (currentToken.JsonType == JsonTokenType.EndOfObject)
                    return ObjectMetadataParseResult.FromMetadata(typeToConstruct, ReferencePreservationInfo.FromNewObjectInJsonDocument(objectId));

                currentToken.MustBeValueDelimiterInObject();

                currentToken = jsonReader.ReadNextToken();
            }
        }
    }
}