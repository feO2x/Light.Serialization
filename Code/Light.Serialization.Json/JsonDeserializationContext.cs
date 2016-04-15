using System;
using Light.GuardClauses;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json
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

        /// <summary>
        ///     Gets the delegate that can be used to deserialize a JSON token.
        /// </summary>
        private readonly Func<JsonToken, Type, object> _deserializeToken;

        /// <summary>
        ///     Creates a new instance of <see cref="JsonDeserializationContext" />.
        /// </summary>
        /// <param name="token">The token to be deserialized.</param>
        /// <param name="requestedType">The requested type of the token.</param>
        /// <param name="jsonReader">The object that is able to read single tokens from a JSON document.</param>
        /// <param name="deserializeToken">The delegate that can be used to deserialize a JSON token.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public JsonDeserializationContext(JsonToken token,
                                          Type requestedType,
                                          IJsonReader jsonReader,
                                          Func<JsonToken, Type, object> deserializeToken)
        {
            requestedType.MustNotBeNull(nameof(requestedType));
            jsonReader.MustNotBeNull(nameof(jsonReader));
            deserializeToken.MustNotBeNull(nameof(deserializeToken));

            Token = token;
            RequestedType = requestedType;
            JsonReader = jsonReader;
            _deserializeToken = deserializeToken;
        }

        /// <summary>
        ///     Deserializes the specified token with as type of T.
        /// </summary>
        /// <typeparam name="T">The requested .NET type of the JSON token.</typeparam>
        /// <param name="token">The token to be deserialized.</param>
        public T DeserializeToken<T>(JsonToken token)
        {
            return (T) _deserializeToken(token, typeof(T));
        }

        /// <summary>
        ///     Deserializes the JSON token as the requested type.
        /// </summary>
        /// <param name="token">The token to be deserialized.</param>
        /// <param name="requestedType">The .NET type the token should be deserialized to.</param>
        public object DeserializeToken(JsonToken token, Type requestedType)
        {
            return _deserializeToken(token, requestedType);
        }
    }
}