using System;
using System.Reflection;
using Light.GuardClauses;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the base class for metadata parsers. Provides the <see cref="ParseType" /> method for parsing
    ///     JSON metadata to .NET types.
    /// </summary>
    public abstract class BaseMetadataParser : BaseMetadata
    {
        // ReSharper disable once InconsistentNaming
        protected readonly INameToTypeMapping _nameToTypeMapping;

        /// <summary>
        ///     Initializes a new instance of BaseMetadataParser.
        /// </summary>
        /// <param name="nameToTypeMapping">The object that can map JSON type names to .NET types.</param>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="nameToTypeMapping" /> is null.</exception>
        protected BaseMetadataParser(INameToTypeMapping nameToTypeMapping)
        {
            nameToTypeMapping.MustNotBeNull(nameof(nameToTypeMapping));

            _nameToTypeMapping = nameToTypeMapping;
        }

        /// <summary>
        ///     Parses the part of the metadata section after the $type symbol to deserialize
        ///     the type information used to construct the target object.
        /// </summary>
        /// <param name="context">The deserialization context for the current complex JSON object or JSON array.</param>
        /// <returns>The type that should be instantiated.</returns>
        protected Type ParseType(JsonDeserializationContext context)
        {
            var jsonReader = context.JsonReader;
            var currentToken = jsonReader.ReadNextToken();

            // If it is a JSON string, then the type can simply be resolved by using the name to type mapping
            if (currentToken.JsonType == JsonTokenType.String)
            {
                var jsonTypeName = context.DeserializeToken<string>(currentToken);
                return _nameToTypeMapping.Map(jsonTypeName);
            }

            // If it is not a JSON string, it must be a complex object for a generic type
            if (currentToken.JsonType != JsonTokenType.BeginOfObject)
                throw new JsonDocumentException($"Expected JSON string or begin of complex JSON object in the metadata type description, but found {currentToken}.", currentToken);

            // Read the "name" key of the generic type description
            currentToken = jsonReader.ReadNextToken();
            currentToken.MustBeComplexObjectKey();
            var key = context.DeserializeToken<string>(currentToken);
            if (key != _genericTypeNameSymbol)
                throw new JsonDocumentException($"Expected {_genericTypeNameSymbol} in metadata section of complex JSON object, but found {key}.", currentToken);

            // Read the value of the "name" key, it must be a JSON string
            currentToken = jsonReader.ReadAndExpectPairDelimiterToken()
                                     .ReadNextToken();
            if (currentToken.JsonType != JsonTokenType.String)
                throw new JsonDocumentException($"Expected name of generic type in metadata section of complex JSON object, but found {currentToken}.", currentToken);
            var genericTypeName = context.DeserializeToken<string>(currentToken);
            var genericType = _nameToTypeMapping.Map(genericTypeName);

            // Read the "typeArguments" key of hte generic type description
            currentToken = jsonReader.ReadAndExpectValueDelimiterToken()
                                     .ReadNextToken();
            currentToken.MustBeComplexObjectKey();
            key = context.DeserializeToken<string>(currentToken);
            if (key != _genericTypeArgumentsSymbol)
                throw new JsonDocumentException($"Expected {_genericTypeArgumentsSymbol} in metadata section of complex JSON object, but found {key}.", currentToken);

            // Read the value of the "typeArguments" key, it must be a JSON array
            jsonReader.ReadAndExpectPairDelimiterToken()
                      .ReadAndExpectBeginOfArray();
            // The JSON array must have as much entries as the type has generic parameters
            var genericTypeArguments = new Type[genericType.GetTypeInfo().GenericTypeParameters.Length];
            for (var i = 0; i < genericTypeArguments.Length; i++)
            {
                genericTypeArguments[i] = ParseType(context);
                if (i < genericTypeArguments.Length - 1)
                    jsonReader.ReadAndExpectValueDelimiterToken();
                else
                {
                    jsonReader.ReadAndExpectedEndOfArray();
                    break;
                }
            }
            jsonReader.ReadAndExpectEndOfObject();

            return genericType.MakeGenericType(genericTypeArguments);
        }
    }
}