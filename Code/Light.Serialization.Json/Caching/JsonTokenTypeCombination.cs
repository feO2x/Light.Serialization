using System;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.GuardClauses.FrameworkExtensions;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.Caching
{
    /// <summary>
    ///     Represents a combination of JSON token type and .NET type.
    ///     Instances of this type are used to choose an <see cref="IJsonTokenParser" /> from the cache.
    ///     This class behaves like a DDD Value Object.
    /// </summary>
    public struct JsonTokenTypeCombination : IEquatable<JsonTokenTypeCombination>
    {
        private readonly int _hashCode;

        /// <summary>
        ///     Gets the JSON token type of this combination.
        /// </summary>
        public readonly JsonTokenType JsonTokenType;

        /// <summary>
        ///     Gets the .NET type of this combination.
        /// </summary>
        public readonly Type Type;

        /// <summary>
        ///     Creates a new instance of <see cref="JsonTokenTypeCombination" />.
        /// </summary>
        /// <param name="jsonTokenType">The JSON type of the combination.</param>
        /// <param name="type">The .NET type of the combination.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        /// <exception cref="EnumValueNotDefinedException">Thrown when <paramref name="jsonTokenType" /> is not a value of the <see cref="JsonTokenType" /> enum.</exception>
        public JsonTokenTypeCombination(JsonTokenType jsonTokenType, Type type)
        {
            jsonTokenType.MustBeValidEnumValue(nameof(jsonTokenType));
            type.MustNotBeNull(nameof(type));

            JsonTokenType = jsonTokenType;
            Type = type;

            _hashCode = Equality.CreateHashCode(jsonTokenType, type);
        }

        /// <summary>
        ///     Checks if the other <see cref="JsonTokenTypeCombination" /> instance has the same token type and .NET type.
        /// </summary>
        /// <param name="other">The other instance to be compared.</param>
        /// <returns>True if both instances have the same token type and .NET type, else false.</returns>
        public bool Equals(JsonTokenTypeCombination other)
        {
            return JsonTokenType == other.JsonTokenType &&
                   Type == other.Type;
        }

        /// <summary>
        ///     Checks if the other instance is a <see cref="JsonTokenTypeCombination" /> instance, and if it contains the same values as this one.
        /// </summary>
        /// <param name="obj">The object to be compared.</param>
        /// <returns>True if <paramref name="obj" /> is a JsonTokenTypeCombination and both instances have the same token type and .NET type, else false.</returns>
        public override bool Equals(object obj)
        {
            try
            {
                return base.Equals((JsonTokenTypeCombination) obj);
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        /// <summary>
        ///     Gets the hash code of this <see cref="JsonTokenTypeCombination" />.
        /// </summary>
        public override int GetHashCode()
        {
            return _hashCode;
        }
    }
}