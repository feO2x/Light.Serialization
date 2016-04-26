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
    ///     Represents a JSON token parser that deserializes complex JSON objects to generic dictionaries.
    /// </summary>
    public sealed class GenericDictionaryParser : IJsonTokenParser
    {
        private readonly IMetadataParser _metadataParser;
        private readonly IMetaFactory _metaFactory;
        private readonly MethodInfo _populateGenericDictionaryInfo = typeof(GenericDictionaryParser).GetTypeInfo().GetDeclaredMethod(nameof(PopulateGenericDictionary));

        /// <summary>
        ///     Creates a new instance of <see cref="GenericDictionaryParser" />.
        /// </summary>
        /// <param name="metaFactory">The factory that is able to create dictionaries from type information.</param>
        /// <param name="metadataParser">The object that can parse the metadata section of a complex JSON object.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metaFactory" /> or <paramref name="metadataParser" /> is null.</exception>
        public GenericDictionaryParser(IMetaFactory metaFactory,
                                       IMetadataParser metadataParser)
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
        public bool IsSuitableFor(JsonToken token, Type requestedType)
        {
            return token.JsonType == JsonTokenType.BeginOfObject &&
                   requestedType.GetTypeInfo().ImplementsGenericInterface(typeof(IDictionary<,>).GetTypeInfo());
        }

        /// <summary>
        ///     Parses the specified JSON complex object as a generic .NET dictionary.
        ///     This method must only be called when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public object ParseValue(JsonDeserializationContext context)
        {
            var jsonReader = context.JsonReader;
            var currentToken = jsonReader.ReadNextToken();

            // Check if it is an empty JSON object
            if (currentToken.JsonType == JsonTokenType.EndOfObject)
                return _metaFactory.CreateDictionary(context.RequestedType);

            // If not, then there must be a JSON string as the first key of the object
            if (currentToken.JsonType != JsonTokenType.String)
                throw new JsonDocumentException($"Expected JSON string or end of complex JSON object, but found {currentToken}", currentToken);

            var metadataParseResult = _metadataParser.ParseMetadataSection(ref currentToken, context);

            if (metadataParseResult.ObjectFromCache != null)
                return metadataParseResult.ObjectFromCache;

            var dictionary = _metaFactory.CreateDictionary(metadataParseResult.TypeToConstruct);

            var specificDictionaryType = metadataParseResult.TypeToConstruct.GetTypeInfo().GetSpecificTypeInfoThatCorrespondsToGenericInterface(typeof(IDictionary<,>).GetTypeInfo());
            var specificPopulateGenericDictionaryMethod = _populateGenericDictionaryInfo.MakeGenericMethod(specificDictionaryType.GenericTypeArguments);

            var methodParameters = new object[3];
            methodParameters[0] = currentToken;
            methodParameters[1] = dictionary;
            methodParameters[2] = context;

            specificPopulateGenericDictionaryMethod.Invoke(null, methodParameters);

            return dictionary;
        }

        private static void PopulateGenericDictionary<TKey, TValue>(JsonToken currentToken, IDictionary<TKey, TValue> dictionary, JsonDeserializationContext context)
        {
            while (true)
            {
                if (currentToken.JsonType != JsonTokenType.String)
                    throw new JsonDocumentException($"Expected key in complex JSON object, but found {currentToken}", currentToken);

                var key = context.DeserializeToken<TKey>(currentToken);

                context.JsonReader.ReadAndExpectPairDelimiterToken();

                currentToken = context.JsonReader.ReadNextToken();
                currentToken.ExpectBeginOfValue();

                var value = context.DeserializeToken<TValue>(currentToken);

                dictionary.Add(key, value);

                if (context.JsonReader.ReadAndExpectEndOfObjectOrValueDelimiter() == JsonTokenType.EndOfObject)
                    return;

                currentToken = context.JsonReader.ReadNextToken();
            }
        }
    }
}