using System;
using System.Globalization;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.PrimitiveTypeFormatters
{
    // Many thanks to JSON.NET (https://github.com/JamesNK/Newtonsoft.Json). I would have never figured out how to do this without it.

    /// <summary>
    ///     Represents a Primitive Type Formatter that serializes double values to JSON numbers.
    /// </summary>
    public sealed class DoubleFormatter : BasePrimitiveTypeFormatter<double>, IPrimitiveTypeFormatter
    {
        /// <summary>
        ///     Creates a new instance of <see cref="DoubleFormatter" />.
        /// </summary>
        public DoubleFormatter() : base(false) { }

        /// <summary>
        ///     Serializes the specified double value to a JSON number.
        /// </summary>
        /// <param name="primitiveValue">The double value to be serialized.</param>
        /// <returns>The JSON number as a string.</returns>
        /// <exception cref="InvalidCastException">Thrown when <paramref name="primitiveValue" /> is not of type <see cref="double" />.</exception>
        public string FormatPrimitiveType(object primitiveValue)
        {
            var value = (double) primitiveValue;

            return FormatRoundtripDoubleValue(value, value.ToString("R", CultureInfo.InvariantCulture));
        }

        private static string FormatRoundtripDoubleValue(double value, string text)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
                return text.SurroundWithQuotationMarks();

            if (text.IndexOf('.') == -1 && text.IndexOf('E') == -1 && text.IndexOf('e') == -1)
                return text + ".0";

            return text;
        }
    }
}