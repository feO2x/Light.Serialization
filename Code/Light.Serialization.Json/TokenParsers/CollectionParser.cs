﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.BuilderHelpers;
using Light.Serialization.Json.ComplexTypeConstruction;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParser" /> that deserializes JSON arrays to .NET collections.
    /// </summary>
    public sealed class CollectionParser : IJsonTokenParser, ISetArrayMetadataParser, ISetMetaFactory
    {
        private readonly MethodInfo _populateArrayWithUnknownLengthMethod = typeof(CollectionParser).GetTypeInfo().GetDeclaredMethod(nameof(PopulateArrayWithUnknownLength));
        private IArrayMetadataParser _metadataParser;
        private IMetaFactory _metaFactory;

        /// <summary>
        ///     Initializes a new instance of <see cref="CollectionParser" />.
        /// </summary>
        /// <param name="metaFactory">The factory that is able to create collection instances from type information.</param>
        /// <param name="metadataParser">The object that can parse the metadata section of a JSON array.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metaFactory" /> or <paramref name="metadataParser" /> is null.</exception>
        public CollectionParser(IMetaFactory metaFactory, IArrayMetadataParser metadataParser)
        {
            metaFactory.MustNotBeNull(nameof(metaFactory));
            metadataParser.MustNotBeNull(nameof(metadataParser));

            _metaFactory = metaFactory;
            _metadataParser = metadataParser;
        }

        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the JSON token is a begin-of-array token.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            return context.Token.JsonType == JsonTokenType.BeginOfArray;
        }

        /// <summary>
        ///     Parses the given JSON array to a .NET collection.
        ///     This method must only be called when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            context.Token.JsonType.MustBe(JsonTokenType.BeginOfArray);
            var currentToken = context.JsonReader.ReadNextToken();

            // Check if the JSON array is empty - in this case, there is no metadata section, and therefore the collection must not be added to the ObjectReferencePreserver
            if (currentToken.JsonType == JsonTokenType.EndOfArray)
                return ParseResult.FromParsedValue(context.RequestedType.IsArray ?
                                                       Array.CreateInstance(context.RequestedType.GetElementType(), new int[context.RequestedType.GetArrayRank()]) :
                                                       _metaFactory.CreateCollection(context.RequestedType));

            // Parse the metadata section and check if the collection refers to another one
            var metadataParseResult = _metadataParser.ParseMetadataSection(ref currentToken, context);
            if (metadataParseResult.ReferencePreservationInfo.WasObjectRetrieved)
                return ParseResult.FromParsedValue(metadataParseResult.ReferencePreservationInfo.RetrievedObject);
            if (metadataParseResult.ReferencePreservationInfo.IsDeferredReference)
                return ParseResult.FromDeferredReference(metadataParseResult.ReferencePreservationInfo.Id);

            var typeToConstruct = metadataParseResult.CollectionTypeToConstruct;
            // Check if the collection is describing an array, because they cannot be populated as collections can be (IList.Add does not work in this case)
            if (metadataParseResult.IsDescribingArrayType)
            {
                Array array;

                // Check if the array length was present in the metadata section; if not, we don't know how long the resulting array must be
                if (metadataParseResult.ArrayLenghts == null)
                {
                    if (typeToConstruct.GetArrayRank() > 1)
                        throw new DeserializationException("Light.GuardClauses does not support the deserialization of multidimensional arrays without the use of a metadata sections in JSON arrays because we cannot know how large the different dimensions of the .NET array are.");
                    array = PopulateArrayWithUnknownLenght(typeToConstruct.GetElementType(), currentToken, context);
                    if (metadataParseResult.ReferencePreservationInfo.IsEmpty == false)
                        context.ObjectReferencePreserver.AddDeserializedObject(metadataParseResult.ReferencePreservationInfo.Id, array);
                    return ParseResult.FromParsedValue(array);
                }

                // If yes, we can create the array directly and populate it
                array = Array.CreateInstance(typeToConstruct.GetElementType(), metadataParseResult.ArrayLenghts);
                if (metadataParseResult.ReferencePreservationInfo.IsEmpty == false)
                    context.ObjectReferencePreserver.AddDeserializedObject(metadataParseResult.ReferencePreservationInfo.Id, array);

                PopulateArray(array, typeToConstruct.GetElementType(), currentToken, context, metadataParseResult.ArrayLenghts);
                return ParseResult.FromParsedValue(array);
            }

            // If it is not an array, then create the collection via the meta factory and populate it
            var collection = _metaFactory.CreateCollection(typeToConstruct);

            if (metadataParseResult.ReferencePreservationInfo.IsEmpty == false)
                context.ObjectReferencePreserver.AddDeserializedObject(metadataParseResult.ReferencePreservationInfo.Id, collection);

            var itemType = typeToConstruct.IsConstructedGenericType ? typeToConstruct.GenericTypeArguments[0] : typeof(object);
            PopulateCollection(collection, itemType, currentToken, context);

            return ParseResult.FromParsedValue(collection);
        }

        /// <summary>
        ///     Gets or sets the object used to parse the metadata section of a JSON array.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IArrayMetadataParser MetadataParser
        {
            get { return _metadataParser; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _metadataParser = value;
            }
        }

        /// <summary>
        ///     Gets or sets the meta factory used to create collection instances.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IMetaFactory MetaFactory
        {
            get { return _metaFactory; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _metaFactory = value;
            }
        }

        private static void PopulateCollection(IList collection, Type itemType, JsonToken currentToken, JsonDeserializationContext context)
        {
            var currentIndex = 0;
            while (true)
            {
                currentToken.MustBeBeginOfValue();
                var parseResult = context.DeserializeToken(currentToken, itemType);

                if (parseResult.IsDeferredReference)
                    context.ObjectReferencePreserver.AddDeferredReference(new DeferredReferenceForCollection(parseResult.ReferenceId, currentIndex, collection));
                else
                    collection.Add(parseResult.ParsedValue);

                currentToken = context.JsonReader.ReadNextToken();
                switch (currentToken.JsonType)
                {
                    case JsonTokenType.ValueDelimiter:
                        currentToken = context.JsonReader.ReadNextToken();
                        currentIndex++;
                        continue;
                    case JsonTokenType.EndOfArray:
                        return;
                    default:
                        throw new JsonDocumentException($"Expected value delimiter or end of array in JSON document, but found {currentToken}.", currentToken);
                }
            }
        }

        private static void PopulateArray(Array array, Type itemType, JsonToken currentToken, JsonDeserializationContext context, int[] arrayLenghts)
        {
            var currentIndices = new int[arrayLenghts.Length];

            while (true)
            {
                currentToken.MustBeBeginOfValue();
                var parseResult = context.DeserializeToken(currentToken, itemType);

                if (parseResult.IsDeferredReference)
                    context.ObjectReferencePreserver.AddDeferredReference(new DeferredReferenceForMultidimensionalArray(parseResult.ReferenceId, currentIndices, array));
                else
                    array.SetValue(parseResult.ParsedValue, currentIndices);

                currentToken = context.JsonReader.ReadNextToken();
                switch (currentToken.JsonType)
                {
                    case JsonTokenType.ValueDelimiter:
                        currentToken = context.JsonReader.ReadNextToken();
                        AdjustCurrentIndices(currentIndices, arrayLenghts);
                        continue;
                    case JsonTokenType.EndOfArray:
                        return;
                    default:
                        throw new JsonDocumentException($"Expected value delimiter or end of array in JSON array, but found {currentToken}.", currentToken);
                }
            }
        }

        private static void AdjustCurrentIndices(int[] currentIndices, int[] arrayLenghts)
        {
            for (var i = arrayLenghts.Length - 1; i >= 0; --i)
            {
                if (currentIndices[i] + 1 < arrayLenghts[i])
                {
                    ++currentIndices[i];
                    break;
                }

                currentIndices[i] = 0;
            }
        }

        private Array PopulateArrayWithUnknownLenght(Type elementType, JsonToken currentToken, JsonDeserializationContext context)
        {
            var resolvedMethod = _populateArrayWithUnknownLengthMethod.MakeGenericMethod(elementType);
            return (Array) resolvedMethod.Invoke(null, new object[] { currentToken, context });
        }

        private static T[] PopulateArrayWithUnknownLength<T>(JsonToken currentToken, JsonDeserializationContext context)
        {
            var temporaryList = new List<T>();
            List<DeferredReferenceCandidate> deferredReferences = null;

            var currentIndex = 0;
            while (true)
            {
                currentToken.MustBeBeginOfValue();
                var parseResult = context.DeserializeToken(currentToken, typeof(T));

                if (parseResult.IsDeferredReference)
                {
                    temporaryList.Add(default(T));
                    if (deferredReferences == null)
                        deferredReferences = new List<DeferredReferenceCandidate>();

                    deferredReferences.Add(new DeferredReferenceCandidate(parseResult.ReferenceId, currentIndex));
                }
                else
                    temporaryList.Add((T) parseResult.ParsedValue);

                currentToken = context.JsonReader.ReadNextToken();
                if (currentToken.JsonType == JsonTokenType.ValueDelimiter)
                {
                    currentToken = context.JsonReader.ReadNextToken();
                    currentIndex++;
                    continue;
                }
                if (currentToken.JsonType == JsonTokenType.EndOfArray)
                    break;
                throw new JsonDocumentException($"Expected value delimiter or end of array in JSON document, but found {currentToken}.", currentToken);
            }

            var array = temporaryList.ToArray();

            if (deferredReferences != null)
            {
                foreach (var candidate in deferredReferences)
                {
                    context.ObjectReferencePreserver.AddDeferredReference(new DeferredReferenceForArray(candidate.ReferenceId, candidate.TargetIndex, array));
                }
            }

            return array;
        }

        private struct DeferredReferenceCandidate
        {
            public readonly int ReferenceId;
            public readonly int TargetIndex;

            public DeferredReferenceCandidate(int referenceId, int targetIndex)
            {
                ReferenceId = referenceId;
                TargetIndex = targetIndex;
            }
        }
    }
}