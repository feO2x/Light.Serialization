using System;
using System.Reflection;
using Light.GuardClauses;
using Light.Serialization.Json.BuilderHelpers;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the base class for metadata parsers. Provides the <see cref="ParseType" /> method for parsing
    ///     JSON metadata to .NET types.
    /// </summary>
    public abstract class BaseMetadataParser : BaseMetadata, ISetNameToTypeMapping, ISetIgnoreMetadataTypeInformation
    {
        /// <summary>
        ///     Initializes a new instance of <see cref="BaseMetadataParser" />.
        /// </summary>
        /// <param name="nameToTypeMapping">The object that can map JSON type names to .NET types.</param>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="nameToTypeMapping" /> is null.</exception>
        protected BaseMetadataParser(INameToTypeMapping nameToTypeMapping)
        {
            nameToTypeMapping.MustNotBeNull(nameof(nameToTypeMapping));

            _nameToTypeMapping = nameToTypeMapping;
        }

        /// <summary>
        ///     Gets or sets the value indicating whether type information should be ignored in the metadata section. This value defaults to false.
        /// </summary>
        public bool IsIgnoringMetadataTypeInformation
        {
            get { return _isIgnoringMetadataTypeInformation; }
            set { _isIgnoringMetadataTypeInformation = value; }
        }

        /// <summary>
        ///     Gets or sets the specified name to type mapping.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public INameToTypeMapping NameToTypeMapping
        {
            get { return _nameToTypeMapping; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _nameToTypeMapping = value;
            }
        }

        /// <summary>
        ///     Parses the part of the metadata section after the $type symbol to deserialize
        ///     the type information used to construct the target object.
        /// </summary>
        /// <param name="context">The deserialization context for the current complex JSON object or JSON array.</param>
        /// <returns>The type that should be instantiated.</returns>
        protected Type ParseType(JsonDeserializationContext context)
        {
            var reader = context.JsonReader;
            var currentToken = reader.ReadNextToken();

            // If it is a JSON string, then the type can simply be resolved by using the name to type mapping
            if (currentToken.JsonType == JsonTokenType.String)
            {
                var jsonTypeName = context.DeserializeToken<string>(currentToken);
                return _nameToTypeMapping.Map(jsonTypeName);
            }

            // If it is not a JSON string, it must be a complex object for a generic type or array type
            if (currentToken.JsonType != JsonTokenType.BeginOfObject)
                throw new JsonDocumentException($"Expected JSON string or begin of complex JSON object describing a type in the metadata section, but found {currentToken}.", currentToken);

            // Read the "name" key of the type description
            currentToken = reader.ReadNextToken();
            currentToken.MustBeComplexObjectKey();
            var key = context.DeserializeToken<string>(currentToken);
            if (key != _typeNameSymbol)
                throw new JsonDocumentException($"Expected {_typeNameSymbol} in type description of metadata section, but found {key}.", currentToken);

            // Read the value of the "name" key, it must be a JSON string
            currentToken = reader.ReadAndExpectPairDelimiterToken()
                                 .ReadNextToken();
            if (currentToken.JsonType != JsonTokenType.String)
                throw new JsonDocumentException($"Expected type name in metadata section, but found {currentToken}.", currentToken);
            var targetTypeName = context.DeserializeToken<string>(currentToken);
            var targetType = _nameToTypeMapping.Map(targetTypeName);
            reader.ReadAndExpectValueDelimiterToken();

            return targetType == typeof(Array) ? ParseArrayType(context) : ParseResolvedGenericType(targetType, context);
        }

        private Type ParseArrayType(JsonDeserializationContext context)
        {
            var reader = context.JsonReader;
            var currentToken = reader.ReadNextToken();

            // Read the "arrayType" symbol
            currentToken.MustBeComplexObjectKey();
            var key = context.DeserializeToken<string>(currentToken);
            if (key != _arrayTypeSymbol)
                throw new JsonDocumentException($"Expected {_arrayTypeSymbol} in type description of metadata section, but found {key}.", currentToken);

            // Read the name of the "arrayType"
            currentToken = reader.ReadAndExpectPairDelimiterToken()
                                 .ReadNextToken();
            if (currentToken.JsonType != JsonTokenType.String)
                throw new JsonDocumentException($"Expected type name in metadata section, but found {currentToken}.", currentToken);
            var elementTypeName = context.DeserializeToken<string>(currentToken);
            var elementType = _nameToTypeMapping.Map(elementTypeName);

            // Read the optional "arrayRank" symbol for multidimensional arrays
            if (reader.ReadAndExpectEndOfObjectOrValueDelimiter() == JsonTokenType.EndOfObject)
                return elementType.MakeArrayType();

            currentToken = reader.ReadNextToken();
            currentToken.MustBeComplexObjectKey();
            key = context.DeserializeToken<string>(currentToken);
            if (key != _arrayRankSymbol)
                throw new JsonDocumentException($"Expected {_arrayRankSymbol} in type description of metadata section, but found {key}.", currentToken);

            // Read the number of dimensions of the multidimensional array
            currentToken = reader.ReadAndExpectPairDelimiterToken()
                                 .ReadNextToken();
            if (currentToken.JsonType != JsonTokenType.IntegerNumber)
                throw new JsonDocumentException($"Expected JSON number describing the number of dimensions for an array type, but found {currentToken}.", currentToken);

            var numberOfDimensions = context.DeserializeToken<int>(currentToken);
            reader.ReadAndExpectEndOfObject();
            return elementType.MakeArrayType(numberOfDimensions);
        }

        protected Type ParseResolvedGenericType(Type genericType, JsonDeserializationContext context)
        {
            // Read the "typeArguments" key of hte generic type description
            var reader = context.JsonReader;
            var currentToken = reader.ReadNextToken();

            currentToken.MustBeComplexObjectKey();
            var key = context.DeserializeToken<string>(currentToken);
            if (key != _genericTypeArgumentsSymbol)
                throw new JsonDocumentException($"Expected {_genericTypeArgumentsSymbol} in metadata section of complex JSON object, but found {key}.", currentToken);

            // Read the value of the "typeArguments" key, it must be a JSON array
            reader.ReadAndExpectPairDelimiterToken()
                  .ReadAndExpectBeginOfArray();
            // The JSON array must have as much entries as the type has generic parameters
            var genericTypeArguments = new Type[genericType.GetTypeInfo().GenericTypeParameters.Length];
            for (var i = 0; i < genericTypeArguments.Length; i++)
            {
                genericTypeArguments[i] = ParseType(context);
                if (i < genericTypeArguments.Length - 1)
                    reader.ReadAndExpectValueDelimiterToken();
                else
                {
                    reader.ReadAndExpectedEndOfArray();
                    break;
                }
            }
            reader.ReadAndExpectEndOfObject();

            return genericType.MakeGenericType(genericTypeArguments);
        }

        // ReSharper disable InconsistentNaming
        protected INameToTypeMapping _nameToTypeMapping;
        protected bool _isIgnoringMetadataTypeInformation;
        // ReSharper restore InconsistentNaming
    }
}