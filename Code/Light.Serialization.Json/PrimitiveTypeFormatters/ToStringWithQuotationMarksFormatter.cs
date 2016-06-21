using System;
using Light.GuardClauses;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.PrimitiveTypeFormatters
{
    /// <summary>
    ///     Represents a generic <see cref="IPrimitiveTypeFormatter" /> that can be used for all types where the JSON string
    ///     can be created by calling object.ToString and surrounding the result with quotation marks.
    /// </summary>
    /// <typeparam name="T">The type whose values should be converted by calling ToString and surrounding the result with quotation marks.</typeparam>
    public sealed class ToStringWithQuotationMarksFormatter<T> : BasePrimitiveTypeFormatter<T>, IPrimitiveTypeFormatter
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ToStringWithQuotationMarksFormatter{T}" />.
        /// </summary>
        /// <param name="shouldBeNormalizedKey">The value indicating whether the returned JSON string should be normalized by the JSON writer when it is used as a key in a complex JSON object.</param>
        public ToStringWithQuotationMarksFormatter(bool shouldBeNormalizedKey = true) : base(shouldBeNormalizedKey) { }

        /// <summary>
        ///     Serializes the specified .NET value to a JSON string by calling object.ToString on it and surrounding the resulting string with quotation marks.
        /// </summary>
        /// <param name="primitiveValue">The value to be serialized.</param>
        /// <returns>The JSON string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="primitiveValue" /> is null.</exception>
        public string FormatPrimitiveType(object primitiveValue)
        {
            primitiveValue.MustNotBeNull(nameof(primitiveValue));

            return primitiveValue.ToString().SurroundWithQuotationMarks();
        }
    }
}