using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.IntegerMetadata;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParser" /> that can deserialize JSON numbers to .NET unsigned integer types.
    /// </summary>
    public sealed class UnsignedIntegerParser : IJsonTokenParser
    {
        private Dictionary<Type, UnsignedIntegerTypeInfo> _unsignedIntegerTypes = UnsignedIntegerTypeInfo.CreateDefaultUnsignedIntegerTypes();

        /// <summary>
        ///     Gets or sets the mapping from .NET unsigned integer types to <see cref="UnsignedIntegerTypeInfo" /> objects.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public Dictionary<Type, UnsignedIntegerTypeInfo> UnsignedIntegerTypes
        {
            get { return _unsignedIntegerTypes; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _unsignedIntegerTypes = value;
            }
        }

        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the requested type is a .NET unsigned integer type and if the given token is a JSON number or string.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            var token = context.Token;
            return (token.JsonType == JsonTokenType.IntegerNumber || token.JsonType == JsonTokenType.FloatingPointNumber || token.JsonType == JsonTokenType.String) &&
                   _unsignedIntegerTypes.ContainsKey(context.RequestedType);
        }

        /// <summary>
        ///     Parses the specified JSON token to a .NET unsigned integer type.
        ///     This method must only be called when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            var token = context.Token;
            if (token.JsonType == JsonTokenType.String)
                token = token.RemoveOuterQuotationMarks();

            var currentIndex = 0;
            var digitsLeftToRead = token.Length;

            if (token[0] == JsonSymbols.Minus)
            {
                if (token[1] != '0')
                    throw new ErroneousTokenException($"Could not deserialize value {token} because it produces an overflow for type {context.RequestedType}.", token);

                if (token.JsonType == JsonTokenType.FloatingPointNumber)
                {
                    var decimalPartInfo = DecimalPartInfo.FromNumericJsonToken(token);
                    if (decimalPartInfo.AreTrailingDigitsOnlyZeros == false)
                        throw new ErroneousTokenException($"Could not deserialize value {token} because it is no integer, but a real number.", token);
                }

                var info = _unsignedIntegerTypes[context.RequestedType];
                return ParseResult.FromParsedValue(info.Type == typeof(ulong) ? 0UL : info.DowncastValue(0UL));
            }

            if (token.JsonType == JsonTokenType.FloatingPointNumber)
            {
                var decimalPartInfo = DecimalPartInfo.FromNumericJsonToken(token);
                if (decimalPartInfo.AreTrailingDigitsOnlyZeros == false)
                    throw new ErroneousTokenException($"Could not deserialize value {token} because it is no integer, but a real number.", token);

                digitsLeftToRead = decimalPartInfo.IndexOfDecimalPoint;
            }

            string overflowCompareString = null;
            var integerInfo = _unsignedIntegerTypes[context.RequestedType];
            if (digitsLeftToRead > integerInfo.MaximumAsString.Length)
                throw new ErroneousTokenException($"Could not deserialize value {token} because it produces an overflow for type {integerInfo.Type}.", token);
            if (digitsLeftToRead == integerInfo.MaximumAsString.Length)
                overflowCompareString = integerInfo.MaximumAsString;

            var result = 0ul;
            var isDefinitelyInRange = false;
            while (digitsLeftToRead > 0)
            {
                var digit = token[currentIndex] - '0';
                if (isDefinitelyInRange == false && overflowCompareString != null)
                {
                    var overflowCompareDigit = overflowCompareString[currentIndex] - '0';
                    if (digit < overflowCompareDigit)
                        isDefinitelyInRange = true;
                    else if (digit > overflowCompareDigit)
                        throw new ErroneousTokenException($"Could not deserialize value {token} because it produces an overflow for type {integerInfo.Type}.", token);
                }
                currentIndex++;
                digitsLeftToRead--;

                result += (ulong) digit * CalculateBase(digitsLeftToRead);
            }

            return ParseResult.FromParsedValue(integerInfo.Type == typeof(ulong) ? result : integerInfo.DowncastValue(result));
        }

        private static ulong CalculateBase(int digitsLeftToRead)
        {
            var result = 1ul;
            for (var i = 0; i < digitsLeftToRead; i++)
            {
                result *= 10;
            }
            return result;
        }
    }
}