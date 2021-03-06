﻿using System;
using System.Globalization;

namespace Light.Serialization.Json.PrimitiveTypeFormatters
{
    // Many thanks to JSON.NET (https://github.com/JamesNK/Newtonsoft.Json). I would have never figured out how to do this without it.

    /// <summary>
    ///     Represents <see cref="IPrimitiveTypeFormatter" /> that serializes .NET <see cref="decimal" /> values to JSON numbers.
    /// </summary>
    public sealed class DecimalFormatter : BasePrimitiveTypeFormatter<decimal>, IPrimitiveTypeFormatter
    {
        /// <summary>
        ///     Initializes a new instance of <see cref="DecimalFormatter" />.
        /// </summary>
        public DecimalFormatter() : base(false) { }

        /// <summary>
        ///     Serializes the specified <see cref="decimal" /> value to a JSON number.
        /// </summary>
        /// <param name="primitiveValue">The decimal to be serialized.</param>
        /// <returns>The JSON number as a string.</returns>
        /// <exception cref="InvalidCastException">Thrown when <paramref name="primitiveValue" /> is not of type decimal.</exception>
        public string FormatPrimitiveType(object primitiveValue)
        {
            var value = (decimal) primitiveValue;

            return FormatDecimal(value.ToString(CultureInfo.InvariantCulture));
        }

        private static string FormatDecimal(string text)
        {
            if (text.IndexOf('.') == -1)
                return text + ".0";
            return text;
        }
    }
}