﻿using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeConstruction;
using Light.Serialization.Json.FrameworkExtensions;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON Token Parser that can deserialize complex JSON objects.
    /// </summary>
    public sealed class ComplexObjectParser : IJsonTokenParser
    {
        private readonly IMetadataParser _metadataParser;
        private readonly IMetaFactory _metaFactory;
        private readonly IInjectableValueNameNormalizer _nameNormalizer;
        private readonly ITypeDescriptionProvider _typeDescriptionProvider;

        /// <summary>
        ///     Creates a new instance of ComplexObjectParser.
        /// </summary>
        /// <param name="metaFactory">The object that can create other objects from type information.</param>
        /// <param name="nameNormalizer">The object used for name normalization between keys in complex JSON objects and .NET member names.</param>
        /// <param name="typeDescriptionProvider">The object that holds creation descriptions for specific types.</param>
        /// <param name="metadataParser">The parser that is used for the metadata section of an complex JSON object.</param>
        public ComplexObjectParser(IMetaFactory metaFactory,
                                   IInjectableValueNameNormalizer nameNormalizer,
                                   ITypeDescriptionProvider typeDescriptionProvider,
                                   IMetadataParser metadataParser)
        {
            metaFactory.MustNotBeNull(nameof(metaFactory));
            nameNormalizer.MustNotBeNull(nameof(nameNormalizer));
            typeDescriptionProvider.MustNotBeNull(nameof(typeDescriptionProvider));
            metadataParser.MustNotBeNull(nameof(metadataParser));

            _metaFactory = metaFactory;
            _nameNormalizer = nameNormalizer;
            _typeDescriptionProvider = typeDescriptionProvider;
            _metadataParser = metadataParser;
        }

        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the specified JSON token is the beginning of a complex JSON object.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            return context.Token.JsonType == JsonTokenType.BeginOfObject;
        }

        /// <summary>
        ///     Parses the specified complex JSON object to a complex .NET type.
        ///     This method must only be called when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            var jsonReader = context.JsonReader;
            var currentToken = jsonReader.ReadNextToken();

            // If the first token is the end of the complex object, then there are no child values
            if (currentToken.JsonType == JsonTokenType.EndOfObject)
                return ParseResult.FromParsedValue(_metaFactory.CreateObject(_typeDescriptionProvider.GetTypeCreationDescription(context.RequestedType), null));

            var metadataParseResult = _metadataParser.ParseMetadataSection(ref currentToken, context);
            if (metadataParseResult.ReferencePreservationInfo.WasObjectRetrieved)
                return ParseResult.FromParsedValue(metadataParseResult.ReferencePreservationInfo.RetrievedObject);
            if (metadataParseResult.ReferencePreservationInfo.IsDeferredReference)
                return ParseResult.FromDeferredReference(metadataParseResult.ReferencePreservationInfo.Id);

            // TODO: if the reconstructed type is a Dictionary, we have to switch to the DictionaryParser here
            if (metadataParseResult.TypeToConstruct.IsDictionaryType())
                throw new NotImplementedException("We have to switch to the DictionaryParser here");

            var typeCreationDescription = _typeDescriptionProvider.GetTypeCreationDescription(metadataParseResult.TypeToConstruct);

            if (currentToken.JsonType == JsonTokenType.EndOfObject)
                return ParseResult.FromParsedValue(_metaFactory.CreateObject(typeCreationDescription, null));

            var deserializedChildValues = new Dictionary<InjectableValueDescription, object>();

            while (true)
            {
                var key = context.DeserializeToken<string>(currentToken);
                var normalizedKey = _nameNormalizer.Normalize(key);

                var injectableValueInfo = typeCreationDescription.GetInjectableValueDescriptionFromNormalizedName(normalizedKey) ??
                                          InjectableValueDescription.FromUnknownValue(normalizedKey, typeof(object));

                currentToken = jsonReader.ReadAndExpectPairDelimiterToken()
                                         .ReadNextToken();
                currentToken.MustBeBeginOfValue();
                var parseResult = context.DeserializeToken(currentToken, injectableValueInfo.Type);
                // TODO: enqueue value if it is a deferred reference
                if (parseResult.IsDeferredReference)
                    throw new NotImplementedException("Here we have to enqueue the value for the object.");

                deserializedChildValues.Add(injectableValueInfo, parseResult.ParsedValue);

                if (jsonReader.ReadAndExpectEndOfObjectOrValueDelimiter() == JsonTokenType.EndOfObject)
                    return ParseResult.FromParsedValue(_metaFactory.CreateObject(typeCreationDescription, deserializedChildValues));

                currentToken = jsonReader.ReadNextToken();
            }
        }
    }
}