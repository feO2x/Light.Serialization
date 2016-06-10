using System;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents a metadata parser for parsing JSON array metadata.
    /// </summary>
    public sealed class ArrayMetadataParser : BaseMetadataParser, IArrayMetadataParser
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
        public ArrayMetadataParseResult ParseMetadataSection(ref JsonToken currentToken, JsonDeserializationContext context)
        {
            var jsonReader = context.JsonReader;
            var objectId = -1;
            var collectionTypeInfo = new CollectionTypeInfo(context.RequestedType);

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
                        return ArrayMetadataParseResult.FromRetrievedObject(objectId, retrievedObject);

                    return ArrayMetadataParseResult.FromDeferredReference(objectId);
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
                    collectionTypeInfo = ParseCollectionType(context);
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
            return collectionTypeInfo.ArrayLengths != null ?
                       ArrayMetadataParseResult.FromArrayType(collectionTypeInfo.CollectionType, referencePreservationInfo, collectionTypeInfo.ArrayLengths) :
                       ArrayMetadataParseResult.FromCollectionType(collectionTypeInfo.CollectionType, referencePreservationInfo);
        }

        private CollectionTypeInfo ParseCollectionType(JsonDeserializationContext context)
        {
            var reader = context.JsonReader;
            var currentToken = reader.ReadNextToken();

            // If it is a JSON string, then the type can simply be resolved by using the name to type mapping
            if (currentToken.JsonType == JsonTokenType.String)
            {
                var jsonTypeName = context.DeserializeToken<string>(currentToken);
                return new CollectionTypeInfo(_nameToTypeMapping.Map(jsonTypeName));
            }

            // If it is not a JSON string, it must be a complex object for a generic collection or array type
            if (currentToken.JsonType != JsonTokenType.BeginOfObject)
                throw new JsonDocumentException($"Expected JSON string or begin of complex JSON object in the metadata type description, but found {currentToken}.", currentToken);

            // Read the "name" key of the generic type description
            currentToken = reader.ReadNextToken();
            currentToken.MustBeComplexObjectKey();
            var key = context.DeserializeToken<string>(currentToken);
            if (key != _genericTypeNameSymbol)
                throw new JsonDocumentException($"Expected {_genericTypeNameSymbol} in metadata section, but found {key}.", currentToken);

            // Read the value of the "name" key, it must be a JSON string
            currentToken = reader.ReadAndExpectPairDelimiterToken()
                                 .ReadNextToken();
            if (currentToken.JsonType != JsonTokenType.String)
                throw new JsonDocumentException($"Expected name of collection type in metadata section of JSON array, but found {currentToken}.", currentToken);
            var collectionTypeName = context.DeserializeToken<string>(currentToken);
            var collectionType = _nameToTypeMapping.Map(collectionTypeName);

            reader.ReadAndExpectValueDelimiterToken();

            // If the type is an array, then the rest of the metadata section has to be parsed differently
            if (collectionType == typeof(Array))
                return ParseArrayMetadataSection(context);

            // Else call the base class implementation for resolving a "normal" type metadata section
            var resolvedGenericCollectionType = ParseResolvedGenericType(collectionType, context);
            return new CollectionTypeInfo(resolvedGenericCollectionType);
        }

        private CollectionTypeInfo ParseArrayMetadataSection(JsonDeserializationContext context)
        {
            var reader = context.JsonReader;
            var currentToken = reader.ReadNextToken();

            // The next element must be the arrayType symbol
            currentToken.MustBeComplexObjectKey();
            var currentKey = context.DeserializeToken<string>(currentToken);
            if (currentKey != _arrayTypeSymbol)
                throw new JsonDocumentException($"Expected {_arrayTypeSymbol} in metadata section of JSON array, but found {currentToken}.", currentToken);

            reader.ReadAndExpectPairDelimiterToken();
            var elementType = ParseType(context);

            // The next token is either the optional arrayRank symbol describing the dimensions of the target array...
            currentToken = reader.ReadAndExpectValueDelimiterToken()
                                 .ReadNextToken();
            currentToken.MustBeComplexObjectKey();
            currentKey = context.DeserializeToken<string>(currentToken);

            var numberOfDimensions = 1;

            if (currentKey == _arrayRankSymbol)
            {
                currentToken = reader.ReadAndExpectPairDelimiterToken()
                                     .ReadNextToken();
                if (currentToken.JsonType != JsonTokenType.IntegerNumber)
                    throw new JsonDocumentException($"Expected JSON number describing the number of dimensions for the target array, but found {currentToken}.", currentToken);

                numberOfDimensions = context.DeserializeToken<int>(currentToken);
                if (numberOfDimensions < 1)
                    throw new JsonDocumentException($"{_arrayRankSymbol} describes the number of dimensions of a multidimensional array and must be greated than 0, but you specified {numberOfDimensions}.", currentToken);

                currentToken = reader.ReadAndExpectValueDelimiterToken()
                                     .ReadNextToken();
                currentToken.MustBeComplexObjectKey();
                currentKey = context.DeserializeToken<string>(currentToken);
            }

            // ...or the arrayLenght symbol. Note that this symbol is mandatory.
            if (currentKey != _arrayLengthSymbol)
                throw new JsonDocumentException($"Expected {_arrayLengthSymbol} in metadata section of JSON array, but found {currentToken}.", currentToken);

            currentToken = reader.ReadAndExpectPairDelimiterToken()
                                 .ReadNextToken();

            // If the target array is not multidimensional, arrayLenght must be a simple integer number
            if (numberOfDimensions == 1)
            {
                if (currentToken.JsonType != JsonTokenType.IntegerNumber)
                    throw new JsonDocumentException($"Expected JSON number describing the length of the target array, but found {currentToken}.", currentToken);
                reader.ReadAndExpectEndOfObject();
                return new CollectionTypeInfo(elementType.MakeArrayType(), new[] { context.DeserializeToken<int>(currentToken) });
            }

            // Else the value is a JSON array containing the lengths of the different dimensions for the target array
            if (currentToken.JsonType != JsonTokenType.BeginOfArray)
                throw new JsonDocumentException($"Expected JSON array describing the lengths of the different dimensions of the target array, but found {currentToken}.", currentToken);

            var arraySizes = new int[numberOfDimensions];
            for (var i = 0;; i++)
            {
                currentToken = reader.ReadNextToken();
                if (currentToken.JsonType != JsonTokenType.IntegerNumber)
                    throw new JsonDocumentException($"Expected JSON number that describes dimension {i} of the target array, but found {currentToken}.", currentToken);
                arraySizes[i] = context.DeserializeToken<int>(currentToken);
                if (i < numberOfDimensions - 1)
                    reader.ReadAndExpectValueDelimiterToken();
                else
                {
                    reader.ReadAndExpectedEndOfArray();
                    break;
                }
            }
            reader.ReadAndExpectEndOfObject();
            return new CollectionTypeInfo(elementType.MakeArrayType(numberOfDimensions), arraySizes);
        }

        private struct CollectionTypeInfo
        {
            public readonly Type CollectionType;
            public readonly int[] ArrayLengths;

            public CollectionTypeInfo(Type collectionType)
            {
                CollectionType = collectionType;
                ArrayLengths = null;
            }

            public CollectionTypeInfo(Type collectionType, int[] arrayLengths)
            {
                CollectionType = collectionType;
                ArrayLengths = arrayLengths;
            }
        }
    }
}