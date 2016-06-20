using System;
using System.Reflection;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Json.BuilderHelpers;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParser" /> that can parse <see cref="Type" /> and <see cref="TypeInfo" /> instances.
    /// </summary>
    public sealed class TypeAndTypeInfoParser : IJsonTokenParser, ISwitchParserForComplexObject, ISetTypeParser
    {
        /// <summary>
        ///     Gets the default type key which is "type".
        /// </summary>
        public const string DefaultTypeKey = "type";

        private ITypeParser _metadataParser;

        private string _typeSymbol = DefaultTypeKey;

        /// <summary>
        ///     Creates a new instance of <see cref="TypeAndTypeInfoParser" />.
        /// </summary>
        /// <param name="metadataParser">The object that parses the metadata section of a complex JSON object.</param>
        public TypeAndTypeInfoParser(ITypeParser metadataParser)
        {
            metadataParser.MustNotBeNull(nameof(metadataParser));

            _metadataParser = metadataParser;
        }

        /// <summary>
        ///     Gets or sets the type symbol that is a key in a complex JSON object identifying a <see cref="Type" /> instance.
        ///     This value defaults to "type".
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="value" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="value" /> contains only whitespace.</exception>
        public string TypeSymbol
        {
            get { return _typeSymbol; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _typeSymbol = value;
            }
        }

        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks that the requested type is either <see cref="Type" /> or <see cref="TypeInfo" />
        ///     and that the token is the beginning of a complex JSON object.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            return context.Token.JsonType == JsonTokenType.BeginOfObject &&
                   (context.RequestedType == typeof(Type) || context.RequestedType == typeof(TypeInfo));
        }

        /// <summary>
        ///     Parses the complex JSON object as either a <see cref="Type" /> or a <see cref="TypeInfo" /> instance.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            var reader = context.JsonReader;
            var currentToken = reader.ReadNextToken();
            if (currentToken.JsonType == JsonTokenType.EndOfObject)
                throw new JsonDocumentException("Expected complex JSON object that describes a Type or TypeInfo instance, but found an empty JSON object.", currentToken);

            var metadataParseResult = _metadataParser.ParseMetadataSection(ref currentToken, context);
            if (metadataParseResult.ReferencePreservationInfo.WasObjectRetrieved)
                return ParseResult.FromParsedValue(metadataParseResult.ReferencePreservationInfo.RetrievedObject);
            if (metadataParseResult.ReferencePreservationInfo.IsDeferredReference)
                return ParseResult.FromDeferredReference(metadataParseResult.ReferencePreservationInfo.Id);

            return ParseValue(metadataParseResult, context, currentToken);
        }

        /// <summary>
        ///     Gets or sets the metadata parser.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public ITypeParser MetadataParser
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
            return typeToBeConstructed == typeof(Type);
        }

        ParseResult ISwitchParserForComplexObject.PerformSwitch(ObjectMetadataParseResult metadataParseResult, JsonDeserializationContext context, JsonToken currentToken)
        {
            return ParseValue(metadataParseResult, context, currentToken);
        }

        private ParseResult ParseValue(ObjectMetadataParseResult metadataParseResult, JsonDeserializationContext context, JsonToken currentToken)
        {
            currentToken.MustBeComplexObjectKey();
            var typeKey = context.DeserializeToken<string>(currentToken);
            if (typeKey != _typeSymbol)
                throw new JsonDocumentException($"Expected key \"{_typeSymbol}\" in complex JSON object describing a Type or TypeInfo instance, but found \"{typeKey}\".", currentToken);

            context.JsonReader.ReadAndExpectPairDelimiterToken();
            var type = _metadataParser.ParseType(context);

            object returnValue = type;
            if (context.RequestedType == typeof(TypeInfo))
                returnValue = type.GetTypeInfo();

            if (metadataParseResult.ReferencePreservationInfo.IsEmpty == false)
                context.ObjectReferencePreserver.AddDeserializedObject(metadataParseResult.ReferencePreservationInfo.Id, returnValue);

            return ParseResult.FromParsedValue(returnValue);
        }
    }
}