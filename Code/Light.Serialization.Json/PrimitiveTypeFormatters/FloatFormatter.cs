using System.Globalization;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.PrimitiveTypeFormatters
{
    // Many thanks to JSON.NET (https://github.com/JamesNK/Newtonsoft.Json). I would have never figured out how to do this without it.

    /// <summary>
    ///     Represents a Primitive Type Formatter that serializes float values to JSON numbers.
    /// </summary>
    public sealed class FloatFormatter : BasePrimitiveTypeFormatter<float>, IPrimitiveTypeFormatter
    {
        /// <summary>
        ///     Creates a new instance of <see cref="FloatFormatter" />.
        /// </summary>
        public FloatFormatter() : base(false) { }

        /// <summary>
        ///     Serializes the specified float value to a JSON number.
        /// </summary>
        /// <param name="primitiveValue">The float value to be serialized.</param>
        /// <returns>The JSON number as a string.</returns>
        public string FormatPrimitiveType(object primitiveValue)
        {
            var value = (float) primitiveValue;

            return FormatRoundTripFloatValue(value, value.ToString("R", CultureInfo.InvariantCulture));
        }

        private static string FormatRoundTripFloatValue(float value, string text)
        {
            if (float.IsInfinity(value) || float.IsNaN(value))
                return text.SurroundWithQuotationMarks();

            if (text.IndexOf('.') == -1 && text.IndexOf('E') == -1 && text.IndexOf('e') == -1)
                return text + ".0";

            return text;
        }
    }
}