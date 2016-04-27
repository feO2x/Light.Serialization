using System;
using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeConstruction;
using Light.Serialization.Json.FrameworkExtensions;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON Token Parser that deserializes JSON arrays to .NET generic collections.
    /// </summary>
    public sealed class ArrayToGenericCollectionParser : IJsonTokenParser
    {
        private readonly IMetadataParser _metadataParser;
        private readonly IMetaFactory _metaFactory;
        private readonly MethodInfo _populateGenericCollectionMethodInfo = typeof(ArrayToGenericCollectionParser).GetTypeInfo().GetDeclaredMethod(nameof(PopulateGenericCollection));

        /// <summary>
        ///     Initializes a new instance of ArrayToGenericCollectionParser.
        /// </summary>
        /// <param name="metaFactory">The factory that is able to create collection instances from type information.</param>
        /// <param name="metadataParser">The object that can parse the metadata section of a JSON array.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metaFactory" /> or <paramref name="metadataParser" /> is null.</exception>
        public ArrayToGenericCollectionParser(IMetaFactory metaFactory, IMetadataParser metadataParser)
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
        public bool IsSuitableFor(JsonToken token, Type requestedType)
        {
            return token.JsonType == JsonTokenType.BeginOfArray;
        }

        /// <summary>
        ///     Parses the given JSON array to a .NET collection.
        ///     This method must only be called when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public object ParseValue(JsonDeserializationContext context)
        {
            
            var firstCollectionToken = context.JsonReader.ReadNextToken();

            if (firstCollectionToken.JsonType == JsonTokenType.EndOfArray)
                return _metaFactory.CreateCollection(context.RequestedType);

            var metadataParseResult = _metadataParser.ParseMetadataSection(ref firstCollectionToken, context);
            if (metadataParseResult.ObjectFromCache != null)
                return metadataParseResult.ObjectFromCache;

            var collection = _metaFactory.CreateCollection(metadataParseResult.TypeToConstruct);

            var specificEnumerableType = context.RequestedType.GetTypeInfo().GetSpecificTypeInfoThatCorrespondsToGenericInterface(typeof(IEnumerable<>).GetTypeInfo());
            var specificPopulateGenericCollectionMethod = _populateGenericCollectionMethodInfo.MakeGenericMethod(specificEnumerableType.GenericTypeArguments);

            var methodParameters = new object[3];
            methodParameters[0] = firstCollectionToken;
            methodParameters[1] = collection;
            methodParameters[2] = context;

            specificPopulateGenericCollectionMethod.Invoke(null, methodParameters);

            return collection;
        }

        private static void PopulateGenericCollection<T>(JsonToken nextToken, ICollection<T> collection, JsonDeserializationContext context)
        {
            while (true)
            {
                nextToken.ExpectBeginOfValue();
                var nextValue = context.DeserializeToken<T>(nextToken);
                collection.Add(nextValue);

                nextToken = context.JsonReader.ReadNextToken();
                switch (nextToken.JsonType)
                {
                    case JsonTokenType.ValueDelimiter:
                        nextToken = context.JsonReader.ReadNextToken();
                        continue;
                    case JsonTokenType.EndOfArray:
                        return;
                    default:
                        throw new JsonDocumentException($"Expected value delimiter or end of array in JSON document, but found {nextToken}.", nextToken);
                }
            }
        }
    }
}