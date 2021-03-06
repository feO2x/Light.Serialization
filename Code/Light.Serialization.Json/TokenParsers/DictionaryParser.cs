﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.BuilderHelpers;
using Light.Serialization.Json.ComplexTypeConstruction;
using Light.Serialization.Json.FrameworkExtensions;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParser" /> that deserializes complex JSON objects to .NET dictionaries.
    /// </summary>
    public sealed class DictionaryParser : IJsonTokenParser, ISwitchParserForComplexObject, ISetObjectMetadataParser, ISetMetaFactory
    {
        private IObjectMetadataParser _metadataParser;
        private IMetaFactory _metaFactory;

        /// <summary>
        ///     Creates a new instance of <see cref="DictionaryParser" />.
        /// </summary>
        /// <param name="metaFactory">The factory that is able to create dictionaries from <see cref="Type" /> information.</param>
        /// <param name="metadataParser">The object that can parse the metadata section of a complex JSON object.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metaFactory" /> or <paramref name="metadataParser" /> is null.</exception>
        public DictionaryParser(IMetaFactory metaFactory, IObjectMetadataParser metadataParser)
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
        ///     Checks if the specified token is a Begin of Object, and if the requested type implements <see cref="IDictionary{TKey,TValue}" /> or <see cref="IDictionary" />.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            return context.Token.JsonType == JsonTokenType.BeginOfObject &&
                   context.RequestedType.IsDictionaryType();
        }

        /// <summary>
        ///     Parses the specified JSON complex object as a .NET dictionary.
        ///     This method must only be called when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            context.Token.JsonType.MustBe(JsonTokenType.BeginOfObject);
            var jsonReader = context.JsonReader;
            var currentToken = jsonReader.ReadNextToken();

            if (currentToken.JsonType == JsonTokenType.EndOfObject)
                return ParseResult.FromParsedValue(_metaFactory.CreateDictionary(context.RequestedType));

            currentToken.MustBeComplexObjectKey();

            var metadataParseResult = _metadataParser.ParseMetadataSection(ref currentToken, context);

            if (metadataParseResult.ReferencePreservationInfo.WasObjectRetrieved)
                return ParseResult.FromParsedValue(metadataParseResult.ReferencePreservationInfo.RetrievedObject);

            if (metadataParseResult.ReferencePreservationInfo.IsDeferredReference)
                return ParseResult.FromDeferredReference(metadataParseResult.ReferencePreservationInfo.Id);

            return ParseDictionary(metadataParseResult, context, currentToken, _metaFactory);
        }

        /// <summary>
        ///     Gets or sets the meta factory used to create dictionary instances.
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

        /// <summary>
        ///     Gets or sets the object used to parse the metadata section of the complex JSON object.
        /// </summary>
        public IObjectMetadataParser MetadataParser
        {
            get { return _metadataParser; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _metadataParser = value;
            }
        }

        bool ISwitchParserForComplexObject.ShouldDeserialize(Type typeToBeConstructed)
        {
            return typeToBeConstructed.IsDictionaryType();
        }

        ParseResult ISwitchParserForComplexObject.PerformSwitch(ObjectMetadataParseResult metadataParseResult, JsonDeserializationContext context, JsonToken currentToken)
        {
            return ParseDictionary(metadataParseResult, context, currentToken, _metaFactory);
        }

        private static ParseResult ParseDictionary(ObjectMetadataParseResult metadataParseResult, JsonDeserializationContext context, JsonToken currentToken, IMetaFactory metaFactory)
        {
            metaFactory.MustNotBeNull(nameof(metaFactory));

            var dictionary = metaFactory.CreateDictionary(metadataParseResult.TypeToBeConstructed);
            if (metadataParseResult.ReferencePreservationInfo.IsEmpty == false)
                context.ObjectReferencePreserver.AddDeserializedObject(metadataParseResult.ReferencePreservationInfo.Id, dictionary);

            var specificDictionaryType = metadataParseResult.TypeToBeConstructed.GetTypeInfo().GetResolvedTypeInfoForGenericInterface(typeof(IDictionary<,>));
            var keyType = specificDictionaryType != null ? specificDictionaryType.GenericTypeArguments[0] : typeof(object);
            var valueType = specificDictionaryType != null ? specificDictionaryType.GenericTypeArguments[1] : typeof(object);

            PopulateDictionary(currentToken, dictionary, context, keyType, valueType);

            return ParseResult.FromParsedValue(dictionary);
        }

        private static void PopulateDictionary(JsonToken currentToken, IDictionary dictionary, JsonDeserializationContext context, Type keyType, Type valueType)
        {
            while (true)
            {
                currentToken.MustBeComplexObjectKey();

                var parseResult = context.DeserializeToken(currentToken, keyType);
                parseResult.IsDeferredReference.MustBeFalse(exception: () => new DeserializationException("The key of a complex JSON object must not be a deferred reference."));
                var key = parseResult.ParsedValue;

                currentToken = context.JsonReader
                                      .ReadAndExpectPairDelimiterToken()
                                      .ReadNextToken();
                currentToken.MustBeBeginOfValue();

                parseResult = context.DeserializeToken(currentToken, valueType);
                if (parseResult.IsDeferredReference)
                    context.ObjectReferencePreserver.AddDeferredReference(new DeferredReferenceForDictionary(parseResult.ReferenceId, key, dictionary));
                else
                    dictionary.Add(key, parseResult.ParsedValue);

                if (context.JsonReader.ReadAndExpectEndOfObjectOrValueDelimiter() == JsonTokenType.EndOfObject)
                    return;

                currentToken = context.JsonReader.ReadNextToken();
            }
        }
    }
}