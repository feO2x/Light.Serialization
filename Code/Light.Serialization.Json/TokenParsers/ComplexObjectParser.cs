using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.BuilderHelpers;
using Light.Serialization.Json.ComplexTypeConstruction;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON Token Parser that can deserialize complex JSON objects.
    /// </summary>
    public sealed class ComplexObjectParser : IJsonTokenParser, ISetTypeDescriptionService, ISetObjectMetadataParser, ISetMetaFactory
    {
        private IObjectMetadataParser _metadataParser;
        private IMetaFactory _metaFactory;
        private ITypeDescriptionService _typeDescriptionService;

        /// <summary>
        ///     Creates a new instance of ComplexObjectParser.
        /// </summary>
        /// <param name="metaFactory">The object that can create other objects from type information.</param>
        /// <param name="typeDescriptionService">The object that holds creation descriptions for specific types.</param>
        /// <param name="metadataParser">The parser that is used for the metadata section of an complex JSON object.</param>
        public ComplexObjectParser(IMetaFactory metaFactory,
                                   ITypeDescriptionService typeDescriptionService,
                                   IObjectMetadataParser metadataParser)
        {
            metaFactory.MustNotBeNull(nameof(metaFactory));
            typeDescriptionService.MustNotBeNull(nameof(typeDescriptionService));
            metadataParser.MustNotBeNull(nameof(metadataParser));

            _metaFactory = metaFactory;
            _typeDescriptionService = typeDescriptionService;
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
        ///     Parses the specified complex JSON object to an instance of a complex .NET type.
        ///     This method must only be called when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            var jsonReader = context.JsonReader;
            var currentToken = jsonReader.ReadNextToken();

            // If the first token is the end of the complex object, then there are no child values
            if (currentToken.JsonType == JsonTokenType.EndOfObject)
                return ParseResult.FromParsedValue(_metaFactory.CreateObject(_typeDescriptionService.GetTypeCreationDescription(context.RequestedType), null));

            // Else parse the metadata section
            var metadataParseResult = _metadataParser.ParseMetadataSection(ref currentToken, context);
            // Check if this JSON object is a $ref and the object could be retrieved
            if (metadataParseResult.ReferencePreservationInfo.WasObjectRetrieved)
                return ParseResult.FromParsedValue(metadataParseResult.ReferencePreservationInfo.RetrievedObject);
            // Else check if this object is a deferred reference
            if (metadataParseResult.ReferencePreservationInfo.IsDeferredReference)
                return ParseResult.FromDeferredReference(metadataParseResult.ReferencePreservationInfo.Id);

            // Check if the type to be constructed should actually be handled by another parser
            if (context.RequestedType != metadataParseResult.TypeToBeConstructed)
            {
                var otherParser = context.GetParserCorrespondingToType(metadataParseResult.TypeToBeConstructed);
                if (otherParser != null)
                    return otherParser.PerformSwitch(metadataParseResult, context, currentToken);
            }

            // Get the type creation description for the type that should be constructed.
            var typeCreationDescription = _typeDescriptionService.GetTypeCreationDescription(metadataParseResult.TypeToBeConstructed);

            // Check if there is any data left to deserialized
            if (currentToken.JsonType == JsonTokenType.EndOfObject)
                return ParseResult.FromParsedValue(_metaFactory.CreateObject(typeCreationDescription, null));
            currentToken.MustBeComplexObjectKey();

            var deserializedChildValues = new Dictionary<InjectableValueDescription, InjectableValue>();
            List<DeferredReferenceCandidate> deferredReferences = null;
            // Run through the remaining key-value pairs of the complex JSON object and deserialize them
            while (true)
            {
                var key = context.DeserializeToken<string>(currentToken);
                var normalizedKey = _typeDescriptionService.NormalizeName(key);

                var injectableValueInfo = typeCreationDescription.GetInjectableValueDescriptionFromNormalizedName(normalizedKey) ??
                                          InjectableValueDescription.FromUnknownValue(normalizedKey, typeof(object));

                currentToken = jsonReader.ReadAndExpectPairDelimiterToken()
                                         .ReadNextToken();
                currentToken.MustBeBeginOfValue();
                var parseResult = context.DeserializeToken(currentToken, injectableValueInfo.Type);
                if (parseResult.IsDeferredReference)
                {
                    if (deferredReferences == null)
                        deferredReferences = new List<DeferredReferenceCandidate>();

                    deferredReferences.Add(new DeferredReferenceCandidate(injectableValueInfo, parseResult.ReferenceId));
                }
                else
                    deserializedChildValues.Add(injectableValueInfo, new InjectableValue(parseResult.ParsedValue));

                if (jsonReader.ReadAndExpectEndOfObjectOrValueDelimiter() == JsonTokenType.EndOfObject)
                    break;

                currentToken = jsonReader.ReadNextToken();
                currentToken.MustBeComplexObjectKey();
            }

            // Create the object from the type creation description and the deserialized child values
            var createdObject = _metaFactory.CreateObject(typeCreationDescription, deserializedChildValues);

            // Add this object to the object reference preserver if there was a $id entry in the metadata section
            if (metadataParseResult.ReferencePreservationInfo.IsEmpty == false)
                context.ObjectReferencePreserver.AddDeserializedObject(metadataParseResult.ReferencePreservationInfo.Id, createdObject);

            // Register all deferred references with the Object Reference Preserver if necessary
            if (deferredReferences != null)
            {
                foreach (var deferredReference in deferredReferences)
                {
                    // Check if the object has been deserialized in the recursive algorithm until now
                    object retrievedValue;
                    if (context.ObjectReferencePreserver.TryGetDeserializedObject(deferredReference.ReferenceId, out retrievedValue))
                    {
                        deferredReference.InjectableValueDescription.SetPropertyOrField(createdObject, retrievedValue);
                        continue;
                    }

                    // If not, create a deferred reference
                    context.ObjectReferencePreserver.AddDeferredReference(new DeferredReferenceForObject(deferredReference.ReferenceId, deferredReference.InjectableValueDescription, createdObject));
                }
            }

            // return the deserialized object
            return ParseResult.FromParsedValue(createdObject);
        }

        /// <summary>
        ///     Gets or sets the meta factory used to create complex objects.
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
        ///     Gets or sets the object that is used the metadata section of a complex JSON object.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IObjectMetadataParser MetadataParser
        {
            get { return _metadataParser; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _metadataParser = value;
            }
        }

        /// <summary>
        ///     Sets the specified type description service on the target instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public ITypeDescriptionService TypeDescriptionService
        {
            get { return _typeDescriptionService; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _typeDescriptionService = value;
            }
        }

        private struct DeferredReferenceCandidate
        {
            public readonly InjectableValueDescription InjectableValueDescription;
            public readonly int ReferenceId;

            public DeferredReferenceCandidate(InjectableValueDescription injectableValueDescription, int referenceId)
            {
                InjectableValueDescription = injectableValueDescription;
                ReferenceId = referenceId;
            }
        }
    }
}