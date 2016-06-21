using System;
using Light.GuardClauses;

namespace Light.Serialization.Json.PrimitiveTypeFormatters
{
    /// <summary>
    ///     Represents a generic <see cref="IPrimitiveTypeFormatter" /> that can be used for all types where the JSON value can be created by calling object.ToString.
    /// </summary>
    /// <typeparam name="T">The type whose values should be converted by calling ToString.</typeparam>
    public sealed class ToStringFormatter<T> : BasePrimitiveTypeFormatter<T>, IPrimitiveTypeFormatter
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ToStringFormatter{T}" />.
        /// </summary>
        /// <param name="shouldBeNormalizedKey">The value indicating whether the returned JSON string should be normalized by the JSON writer when it is used as a key in a complex JSON object.</param>
        public ToStringFormatter(bool shouldBeNormalizedKey = true) : base(shouldBeNormalizedKey) { }

        /// <summary>
        ///     Serializes the specified .NET value by calling object.ToString on it.
        /// </summary>
        /// <param name="primitiveValue">The .NET value to be serialized.</param>
        /// <returns>The JSON value as a string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="primitiveValue" /> is null.</exception>
        public string FormatPrimitiveType(object primitiveValue)
        {
            primitiveValue.MustNotBeNull(nameof(primitiveValue));

            return primitiveValue.ToString();
        }
    }
}