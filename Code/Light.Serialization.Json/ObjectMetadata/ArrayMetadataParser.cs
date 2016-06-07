using System;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents a metadata parser for parsing JSON array metadata.
    /// </summary>
    public sealed class ArrayMetadataParser : BaseMetadataParser, IMetadataParser
    {
        /// <summary>
        ///     Creates a new instance of ArrayMetadataParser.
        /// </summary>
        /// <param name="nameToTypeMapping">The object that can map JSON type names to .NET types.</param>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="nameToTypeMapping" /> is null.</exception>
        public ArrayMetadataParser(INameToTypeMapping nameToTypeMapping) : base(nameToTypeMapping) { }

        /// <summary>
        ///     Parses the metadata section of a complex JSON object or JSON array given the first token and the deserialization context.
        /// </summary>
        /// <param name="currentToken">The JSON token that represents the first token in a complex JSON object or JSON array.</param>
        /// <param name="context">The deserialization context for the object to be deserialized.</param>
        /// <returns>The metadata parse result.</returns>
        public MetadataParseResult ParseMetadataSection(ref JsonToken currentToken, JsonDeserializationContext context)
        {
            var jsonReader = context.JsonReader;
            var typeToConstruct = context.RequestedType;
            var objectId = -1;

            // Loop through the metadata section of the JSON array
            for (var i = 0; currentToken.JsonType == JsonTokenType.String; i++)
            {
                var tokenString = context.DeserializeToken<string>(currentToken);

                // $ref - refernce to another object in the JSON document
                if (tokenString == _referenceSymbol)
                {
                    if (i > 0)
                        throw new JsonDocumentException($"The {_referenceSymbol} is used in a wrong fashion: it should be the only metadata symbol when a JSON array points to another one.", currentToken);

                    var numberToken = jsonReader.ReadAndExpectValueDelimiterToken()
                                                .ReadAndExpectNumber();
                    jsonReader.ReadAndExpectedEndOfArray("A reference to another collection must be the only value in a JSON array.");
                    objectId = context.DeserializeToken<int>(numberToken);
                    object retrievedObject;
                    if (context.ObjectReferencePreserver.TryGetDeserializedObject(objectId, out retrievedObject))
                        return MetadataParseResult.FromRetrievedObject(objectId, retrievedObject);

                    return MetadataParseResult.FromDeferredReference(objectId);
                }

                // $id - defining an id for this complex JSON object
                if (tokenString == _idSymbol)
                {
                    var numberToken = jsonReader.ReadAndExpectValueDelimiterToken()
                                                .ReadAndExpectNumber();

                    objectId = context.DeserializeToken<int>(numberToken);
                }

                // $type - type information for this complex JSON object
                else if (tokenString == _concreteTypeSymbol)
                {
                    jsonReader.ReadAndExpectValueDelimiterToken();
                    typeToConstruct = ParseType(context);
                }

                // No metadata token found - this means the end of the metadata section in the complex JSON object was reached
                else
                    break;

                // Update currentToken for next loop cycle
                currentToken = jsonReader.ReadNextToken();
                if (currentToken.JsonType == JsonTokenType.EndOfArray)
                    break;

                currentToken.MustBeValueDelimiterInArray();

                currentToken = jsonReader.ReadNextToken();
            }

            var referencePreservationInfo = objectId == -1 ? ReferencePreservationInfo.Empty : ReferencePreservationInfo.FromNewObjectInJsonDocument(objectId);
            return MetadataParseResult.FromMetadata(typeToConstruct, referencePreservationInfo);
        }
    }
}