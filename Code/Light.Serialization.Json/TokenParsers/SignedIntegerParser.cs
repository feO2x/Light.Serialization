using System;
using Light.GuardClauses;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.IntegerMetadata;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON Token Parser that can parse JSON numeric tokens to .NET integer values.
    /// </summary>
    public sealed class SignedIntegerParser : IJsonTokenParser
    {
        private SignedIntegerTypes _signedIntegerTypes = SignedIntegerTypes.CreateDefaultSignedIntegerTypes();

        /// <summary>
        ///     Gets or sets the signed integer types info object.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public SignedIntegerTypes SignedIntegerTypes
        {
            get { return _signedIntegerTypes; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _signedIntegerTypes = value;
            }
        }

        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Checks if the requested type is a signed integer type and if the JSON token is a number or string.
        /// </summary>
        public bool IsSuitableFor(JsonToken token, Type requestedType)
        {
            return (_signedIntegerTypes.IntegerTypeInfos.ContainsKey(requestedType) && (token.JsonType == JsonTokenType.IntegerNumber || token.JsonType == JsonTokenType.FloatingPointNumber || token.JsonType == JsonTokenType.String)) ||
                   ((requestedType == typeof(object) || requestedType == typeof(ValueType)) && token.JsonType == JsonTokenType.IntegerNumber);
        }

        /// <summary>
        ///     Parses the given token as a .NET signed integer type.
        ///     This method must only be called when <see cref="IsSuitableFor" /> would return true.
        /// </summary>
        public object ParseValue(JsonDeserializationContext context)
        {
            var token = context.Token;
            if (token.JsonType == JsonTokenType.String)
                token = token.RemoveOuterQuotationMarks();

            var digitsLeftToRead = token.Length;

            if (token.JsonType == JsonTokenType.FloatingPointNumber)
            {
                var decimalPartInfo = DecimalPartInfo.FromNumericJsonToken(token);
                if (decimalPartInfo.AreTrailingDigitsOnlyZeros == false)
                    throw new DeserializationException($"Could not deserialize value {token} because it is no integer, but a real number.");

                digitsLeftToRead = decimalPartInfo.IndexOfDecimalPoint;
            }

            var currentIndex = 0;
            var isResultNegative = false;
            string overflowCompareString = null;
            SignedIntegerTypeInfo integerInfo;
            if (_signedIntegerTypes.IntegerTypeInfos.TryGetValue(context.RequestedType, out integerInfo) == false)
                integerInfo = _signedIntegerTypes.DefaultType;

            if (token[0] == JsonSymbols.Minus)
            {
                if (digitsLeftToRead > integerInfo.MinimumAsString.Length)
                    throw new DeserializationException($"Could not deserialize value {token} because it produces an overflow for type {integerInfo.Type}.");
                if (digitsLeftToRead == integerInfo.MinimumAsString.Length)
                    overflowCompareString = integerInfo.MinimumAsString;
                isResultNegative = true;

                digitsLeftToRead--;
                currentIndex++;
            }
            else if (digitsLeftToRead > integerInfo.MaximumAsString.Length)
                throw new DeserializationException($"Could not deserialize value {token} because it produces an overflow for type {integerInfo.Type}.");
            else if (digitsLeftToRead == integerInfo.MaximumAsString.Length)
                overflowCompareString = integerInfo.MaximumAsString;

            var result = 0L;
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
                        throw new DeserializationException($"Could not deserialize value {token} because it produces an overflow for type {integerInfo.Type}.");
                }
                currentIndex++;
                digitsLeftToRead--;

                result += digit * CalculateBase(digitsLeftToRead);
            }

            if (isResultNegative)
                result = -result;

            return integerInfo.Type == typeof(long) ? result : integerInfo.DowncastValue(result);
        }

        private static long CalculateBase(int digitsLeftToRead)
        {
            var result = 1L;
            for (var i = 0; i < digitsLeftToRead; i++)
            {
                result *= 10;
            }
            return result;
        }
    }
}