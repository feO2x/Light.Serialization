using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeConstruction;
using Light.Serialization.Json.FrameworkExtensions;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON token parser that deserializes complex JSON objects to .NET dictionaries.
    /// </summary>
    public sealed class DictionaryParser : IJsonTokenParser
    {
        private readonly IMetadataParser _metadataParser;
        private readonly IMetaFactory _metaFactory;

        /// <summary>
        ///     Creates a new instance of <see cref="DictionaryParser" />.
        /// </summary>
        /// <param name="metaFactory">The factory that is able to create dictionaries from type information.</param>
        /// <param name="metadataParser">The object that can parse the metadata section of a complex JSON object.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metaFactory" /> or <paramref name="metadataParser" /> is null.</exception>
        public DictionaryParser(IMetaFactory metaFactory, IMetadataParser metadataParser)
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
        ///     Checks if the specified token is a Begin of Object, and if the requested type implements IDictionary of T.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            return context.Token.JsonType == JsonTokenType.BeginOfObject &&
                   context.RequestedType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IDictionary));
        }

        /// <summary>
        ///     Parses the specified JSON complex object as a .NET dictionary.
        ///     This method must only be called when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            var jsonReader = context.JsonReader;
            var currentToken = jsonReader.ReadNextToken();

            // Check if it is an empty JSON object
            if (currentToken.JsonType == JsonTokenType.EndOfObject)
                return ParseResult.FromParsedValue(_metaFactory.CreateDictionary(context.RequestedType));

            // If not, then there must be a JSON string as the first key of the object
            if (currentToken.JsonType != JsonTokenType.String)
                throw new JsonDocumentException($"Expected JSON string or end of complex JSON object, but found {currentToken}", currentToken);

            var metadataParseResult = _metadataParser.ParseMetadataSection(ref currentToken, context);

            if (metadataParseResult.ReferencePreservationInfo.WasObjectRetrieved)
                return ParseResult.FromParsedValue(metadataParseResult.ReferencePreservationInfo.RetrievedObject);

            if (metadataParseResult.ReferencePreservationInfo.IsDeferredReference)
                return ParseResult.FromDeferredReference(metadataParseResult.ReferencePreservationInfo.Id);

            var dictionary = _metaFactory.CreateDictionary(metadataParseResult.TypeToConstruct);

            var specificDictionaryType = metadataParseResult.TypeToConstruct.GetTypeInfo().GetResolvedTypeInfoForGenericInterface(typeof(IDictionary<,>));
            var keyType = specificDictionaryType != null ? specificDictionaryType.GenericTypeArguments[0] : typeof(object);
            var valueType = specificDictionaryType != null ? specificDictionaryType.GenericTypeArguments[1] : typeof(object);

            PopulateDictionary(currentToken, dictionary, context, keyType, valueType);

            return ParseResult.FromParsedValue(dictionary);
        }

        private static void PopulateDictionary(JsonToken currentToken, IDictionary dictionary, JsonDeserializationContext context, Type keyType, Type valueType)
        {
            while (true)
            {
                if (currentToken.JsonType != JsonTokenType.String)
                    throw new JsonDocumentException($"Expected key in complex JSON object, but found {currentToken}.", currentToken);

                var key = context.DeserializeToken(currentToken, keyType);

                context.JsonReader.ReadAndExpectPairDelimiterToken();

                currentToken = context.JsonReader.ReadNextToken();
                currentToken.ExpectBeginOfValue();

                var value = context.DeserializeToken(currentToken, valueType);

                dictionary.Add(key, value);

                if (context.JsonReader.ReadAndExpectEndOfObjectOrValueDelimiter() == JsonTokenType.EndOfObject)
                    return;

                currentToken = context.JsonReader.ReadNextToken();
            }
        }
    }
}