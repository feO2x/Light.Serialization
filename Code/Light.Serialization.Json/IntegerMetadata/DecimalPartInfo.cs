using System;
using Light.GuardClauses;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.IntegerMetadata
{
    /// <summary>
    ///     Represents a structure indicating whether a JSON number has a decimal point and
    ///     if all decimal digits are zeros (so that this number can actually be used as an .NET integer number).
    /// </summary>
    public struct DecimalPartInfo
    {
        /// <summary>
        ///     Gets the index of the decimal point.
        /// </summary>
        public readonly int IndexOfDecimalPoint;

        /// <summary>
        ///     Gets the value indicating whether all decimal digits of a JSON number are zero.
        /// </summary>
        public readonly bool AreTrailingDigitsOnlyZeros;

        /// <summary>
        ///     Creates a new instance of DecimalPartInfo.
        /// </summary>
        /// <param name="indexOfDecimalPoint">The index of the decimal point, or -1 if the JSON number does not have a decimal point.</param>
        /// <param name="areTrailingDigitsOnlyZeros">The value indicating whether all decimal digits are zeros - specify true here if a JSON number does not have a decimal point.</param>
        public DecimalPartInfo(int indexOfDecimalPoint = -1, bool areTrailingDigitsOnlyZeros = true)
        {
            IndexOfDecimalPoint = indexOfDecimalPoint;
            AreTrailingDigitsOnlyZeros = areTrailingDigitsOnlyZeros;
        }

        /// <summary>
        ///     Creates a decimal part info from the specified token representing a JSON floating point number.
        /// </summary>
        /// <param name="token">The token that should be analyzed.</param>
        /// <returns>The decimal part info.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="token" /> is not a FloatingPointNumber.</exception>
        public static DecimalPartInfo FromNumericJsonToken(JsonToken token)
        {
            Check.That(token.JsonType == JsonTokenType.FloatingPointNumber,
                       () => new ArgumentException("The token must be a floating point number."));

            var areTrailingDigitsOnlyZeros = true;
            var indexOfDecimalPoint = -1;
            int i;

            for (i = 0; i < token.Length; i++)
            {
                if (token[i] != JsonSymbols.DecimalPoint) continue;

                indexOfDecimalPoint = i;
                break;
            }

            for (i++; i < token.Length; i++)
            {
                if (token[i] == '0') continue;
                areTrailingDigitsOnlyZeros = false;
                break;
            }

            return new DecimalPartInfo(indexOfDecimalPoint, areTrailingDigitsOnlyZeros);
        }
    }
}