using System;

namespace Light.Serialization.Json.PrimitiveTypeFormatters
{
    /// <summary>
    ///     Provides <see cref="TargetType" /> and <see cref="ShouldBeNormalizedKey" /> members for classes that want to implement the <see cref="IPrimitiveTypeFormatter" /> interface.
    /// </summary>
    /// <typeparam name="T">The primitive type that the formatter serializes.</typeparam>
    public abstract class BasePrimitiveTypeFormatter<T>
    {
        /// <summary>
        ///     Initializes <see cref="ShouldBeNormalizedKey" /> with the specified value.
        /// </summary>
        /// <param name="shouldBeNormalizedKey">The value that will be assigned to <see cref="ShouldBeNormalizedKey" />.</param>
        protected BasePrimitiveTypeFormatter(bool shouldBeNormalizedKey)
        {
            ShouldBeNormalizedKey = shouldBeNormalizedKey;
        }

        /// <summary>
        ///     Gets the type that this formatter can serialize. Is set to <c>typeof(T)</c>.
        /// </summary>
        public Type TargetType { get; } = typeof (T);

        /// <summary>
        ///     Gets the value indicating whether the returned JSON string should be normalized by the JSON writer when it is used as a key in a complex JSON object.
        /// </summary>
        public bool ShouldBeNormalizedKey { get; }
    }
}