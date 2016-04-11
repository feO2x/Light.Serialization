using System;

namespace Light.Serialization.Json.PrimitiveTypeFormatters
{
    /// <summary>
    ///     Represents a Primitive Type Formatter that serializes .NET boolean values to JSON boolean values.
    /// </summary>
    public sealed class BooleanFormatter : BasePrimitiveTypeFormatter<bool>, IPrimitiveTypeFormatter
    {
        /// <summary>
        ///     Creates a new instance of BooleanFormatter.
        /// </summary>
        public BooleanFormatter() : base(false) { }

        /// <summary>
        ///     Serializes the specified .NET boolean value to a JSON boolean value.
        /// </summary>
        /// <param name="primitiveValue">The boolean value to be serialized.</param>
        /// <returns>"true" if <paramref name="primitiveValue" /> is true, else "false".</returns>
        /// <exception cref="InvalidCastException">Thrown when the specified <paramref name="primitiveValue" /> can not be downcasted to a boolean.</exception>
        public string FormatPrimitiveType(object primitiveValue)
        {
            return (bool) primitiveValue ? JsonSymbols.True : JsonSymbols.False;
        }
    }
}