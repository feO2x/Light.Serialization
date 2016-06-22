using System;
using System.Diagnostics;
using Light.GuardClauses;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents the context that is necessary to deserialize a single token in a JSON document.
    /// </summary>
    public struct JsonDeserializationContext
    {
        /// <summary>
        ///     Gets the JSON token to be deserialized.
        /// </summary>
        public readonly JsonToken Token;

        /// <summary>
        ///     Gets the type that is requested by the .NET object graph for the object to be serialized.
        /// </summary>
        public readonly Type RequestedType;

        /// <summary>
        ///     Gets the object that is able to read single tokens from a JSON document.
        /// </summary>
        public readonly IJsonReader JsonReader;

        private readonly IRecursiveDeserializer _deserializer;

        /// <summary>
        ///     Gets the object reference preserver that handles already deserialized objects and deferred references.
        /// </summary>
        public ObjectReferencePreserver ObjectReferencePreserver;

        /// <summary>
        ///     Creates a new instance of <see cref="JsonDeserializationContext" />.
        /// </summary>
        /// <param name="token">The token to be deserialized.</param>
        /// <param name="requestedType">The requested type of the token.</param>
        /// <param name="jsonReader">The forward-only reader accessing the JSON document.</param>
        /// <param name="deserializer">The deserializer that can be used to deserialize a child JSON token.</param>
        /// <param name="objectReferencePreserver">The object handling Object Reference Preservation.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public JsonDeserializationContext(JsonToken token,
                                          Type requestedType,
                                          IJsonReader jsonReader,
                                          IRecursiveDeserializer deserializer,
                                          ObjectReferencePreserver objectReferencePreserver)
        {
            requestedType.MustNotBeNull(nameof(requestedType));
            jsonReader.MustNotBeNull(nameof(jsonReader));
            deserializer.MustNotBeNull(nameof(deserializer));
            objectReferencePreserver.MustNotBeNull(nameof(objectReferencePreserver));

            Token = token;
            RequestedType = requestedType;
            JsonReader = jsonReader;
            _deserializer = deserializer;
            ObjectReferencePreserver = objectReferencePreserver;
        }

        /// <summary>
        ///     Deserializes the specified token as type of <see cref="T" />.
        /// </summary>
        /// <typeparam name="T">The requested .NET type of the JSON token.</typeparam>
        /// <param name="token">The token to be deserialized.</param>
        /// <exception cref="InvalidOperationException">Thrown when the deserialized value is a deferred reference.</exception>
        public T DeserializeToken<T>(JsonToken token)
        {
            var parseResult = _deserializer.DeserializeToken(token, typeof(T));
            CheckForDeferredReference(token, parseResult);

            return (T) parseResult.ParsedValue;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        // ReSharper disable once UnusedParameter.Local
        private static void CheckForDeferredReference(JsonToken token, ParseResult parseResult)
        {
            if (parseResult.ParsedValue == null)
                throw new InvalidOperationException($"The token \"{token}\" could not be completely deserialized because it is a deferred reference.");
        }

        /// <summary>
        ///     Deserializes the JSON token as the requested type.
        /// </summary>
        /// <param name="token">The token to be deserialized.</param>
        /// <param name="requestedType">The .NET type the token should be deserialized to.</param>
        public ParseResult DeserializeToken(JsonToken token, Type requestedType)
        {
            return _deserializer.DeserializeToken(token, requestedType);
        }

        /// <summary>
        ///     Checks if the <paramref name="typeToBeConstructed" /> should not be handled by the <see cref="ComplexObjectParser" />, but by another one.
        ///     This method should only be called by the <see cref="ComplexObjectParser" /> when it deserialized the actual type of the object to be deserialized
        ///     from the metadata section of a complex JSON object.
        /// </summary>
        /// <param name="typeToBeConstructed">The actual type of the object to be deserialized.</param>
        /// <returns>An instance of <see cref="ISwitchParserForComplexObject" /> that can be used to perform the parser switch if it is necessary, else null.</returns>
        public ISwitchParserForComplexObject GetParserCorrespondingToType(Type typeToBeConstructed)
        {
            return _deserializer.FindParserForType(typeToBeConstructed);
        }
    }
}