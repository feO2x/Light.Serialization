using System;
using System.Collections;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeConstruction;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON Token Parser that deserializes JSON arrays to .NET generic collections.
    /// </summary>
    public sealed class CollectionParser : IJsonTokenParser
    {
        private readonly IMetadataParser _metadataParser;
        private readonly IMetaFactory _metaFactory;

        /// <summary>
        ///     Initializes a new instance of ArrayToGenericCollectionParser.
        /// </summary>
        /// <param name="metaFactory">The factory that is able to create collection instances from type information.</param>
        /// <param name="metadataParser">The object that can parse the metadata section of a JSON array.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metaFactory" /> or <paramref name="metadataParser" /> is null.</exception>
        public CollectionParser(IMetaFactory metaFactory, IMetadataParser metadataParser)
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
            var currentToken = context.JsonReader.ReadNextToken();

            if (currentToken.JsonType == JsonTokenType.EndOfArray)
                return ParseResult.FromParsedValue(_metaFactory.CreateCollection(context.RequestedType));

            var metadataParseResult = _metadataParser.ParseMetadataSection(ref currentToken, context);
            if (metadataParseResult.ReferencePreservationInfo.WasObjectRetrieved)
                return ParseResult.FromParsedValue(metadataParseResult.ReferencePreservationInfo.RetrievedObject);
            if (metadataParseResult.ReferencePreservationInfo.IsDeferredReference)
                return ParseResult.FromDeferredReference(metadataParseResult.ReferencePreservationInfo.Id);

            var typeToConstruct = metadataParseResult.TypeToConstruct;
            var collection = _metaFactory.CreateCollection(typeToConstruct);

            // TODO: arrays cannot be populated this way, I have to create another method
            var itemType = typeToConstruct.IsConstructedGenericType ? typeToConstruct.GenericTypeArguments[0] : typeof(object);
            PopulateCollection(currentToken, collection, context, itemType);

            return ParseResult.FromParsedValue(collection);
        }

        private static void PopulateCollection(JsonToken currentToken, IList collection, JsonDeserializationContext context, Type itemType)
        {
            while (true)
            {
                currentToken.MustBeBeginOfValue();
                var nextValue = context.DeserializeToken(currentToken, itemType);
                collection.Add(nextValue);

                currentToken = context.JsonReader.ReadNextToken();
                switch (currentToken.JsonType)
                {
                    case JsonTokenType.ValueDelimiter:
                        currentToken = context.JsonReader.ReadNextToken();
                        continue;
                    case JsonTokenType.EndOfArray:
                        return;
                    default:
                        throw new JsonDocumentException($"Expected value delimiter or end of array in JSON document, but found {currentToken}.", currentToken);
                }
            }
        }
    }
}