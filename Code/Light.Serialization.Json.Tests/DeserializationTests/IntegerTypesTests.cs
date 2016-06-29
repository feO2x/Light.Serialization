using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class IntTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse JSON numbers to .NET int instances.")]
        [InlineData("42", 42)]
        [InlineData(int.MaxValue / 2, int.MaxValue / 2)]
        [InlineData("1247483647", 1247483647)]
        [InlineData(int.MaxValue, int.MaxValue)]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData("0", 0)]
        [InlineData("-0", 0)]
        [InlineData(int.MinValue + int.MaxValue / 2, int.MinValue + int.MaxValue / 2)]
        public void IntCanBeDeserializedCorrectly(string json, int expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        [Theory(DisplayName = "The deserializer must throw an exception when the JSON number exceeds the limits of the .NET int type.")]
        [InlineData("2147483648")] //int max value ( 2147483647 ) +1
        [InlineData("-2147483658")]
        [InlineData("2247483647")]
        [InlineData("3147483647")]
        [InlineData("214748364700000000")]
        public void ExceptionIsThrownWhenOverflowingIntIsDeserialized(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<int>(json, $"Could not deserialize value {json} because it produces an overflow for type System.Int32.");
        }

        [Theory(DisplayName = "The deserializer must be able to deserialize JSON numbers with trailing zeros to .NET int instances.")]
        [InlineData("42.0", 42)]
        [InlineData("1247483647.000", 1247483647)]
        [InlineData("2147483647.0000", int.MaxValue)]
        [InlineData("-2147483648.00000", int.MinValue)]
        [InlineData("0.0", 0)]
        [InlineData("-0.0", 0)]
        public void IntWithTrailingZerosAfterDecimalPointCanBeDeserialized(string json, int expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        [Theory(DisplayName = "The deserializer must throw an exception when the JSON number has trailing digits that are not zero.")]
        [InlineData("42.7")]
        [InlineData("1247483647.007")]
        [InlineData("2147483647.0353")]
        [InlineData("0.00000856")]
        [InlineData("-2147483648.000000001")]
        public void ExceptionIsThrownWhenIntWithNonZeroDigitsAfterDecimalPointIsDeserialized(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<int>(json, $"Could not deserialize value {json} because it is no integer, but a real number.");
        }
    }

    public sealed class UIntTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse JSON numbers to .NET uint instances.")]
        [InlineData("42", 42u)]
        [InlineData(uint.MaxValue / 2u, uint.MaxValue / 2u)]
        [InlineData("3394967295", 3394967295u)]
        [InlineData(uint.MaxValue, uint.MaxValue)]
        [InlineData(uint.MinValue, uint.MinValue)]
        [InlineData("-0", 0u)]
        public void UIntCanBeDeserializedCorrectly(string json, uint expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        [Theory(DisplayName = "The deserializer must throw an exception when the JSON number exceeds the limits of the .NET uint type.")]
        [InlineData("4294967296")] //uint max value ( 4294967295 ) +1
        [InlineData("-1")]
        [InlineData("4394967295")]
        [InlineData("5294967295")]
        [InlineData("429496729500000000")]
        public void ExceptionIsThrownWhenOverflowingUIntIsDeserialized(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<uint>(json, $"Could not deserialize value {json} because it produces an overflow for type System.UInt32.");
        }

        [Theory(DisplayName = "The deserializer must be able to deserialize JSON numbers with trailing zeros to .NET uint instances.")]
        [InlineData("42.0", 42u)]
        [InlineData("3394967295.000", 3394967295u)]
        [InlineData("4294967295.0000", uint.MaxValue)]
        [InlineData("0.00000", uint.MinValue)]
        [InlineData("-0.00000", uint.MinValue)]
        public void UintWithTrailingZerosAfterDecimalPointCanBeDeserialized(string json, uint expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }
    }

    public sealed class ShortTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse JSON numbers to .NET short instances.")]
        [InlineData("42", 42)]
        [InlineData(short.MaxValue / 2, short.MaxValue / 2)]
        [InlineData("23767", 23767)]
        [InlineData(short.MaxValue, short.MaxValue)]
        [InlineData(short.MinValue, short.MinValue)]
        [InlineData("0", 0)]
        [InlineData("-0", 0)]
        [InlineData(short.MinValue + short.MaxValue / 2, short.MinValue + short.MaxValue / 2)]
        public void ShortCanBeDeserializedCorrectly(string json, short expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        [Theory(DisplayName = "The deserializer must throw an exception when the JSON number exceeds the limits of the .NET short type.")]
        [InlineData("32768")] //short max value ( 32767 ) +1
        [InlineData("-32778")]
        [InlineData("33767")]
        [InlineData("42767")]
        [InlineData("3276700000000")]
        public void ExceptionIsThrownWhenOverflowingShortIsDeserialized(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<short>(json, $"Could not deserialize value {json} because it produces an overflow for type System.Int16.");
        }

        [Theory(DisplayName = "The deserializer must be able to deserialize JSON numbers with trailing zeros to .NET short instances.")]
        [InlineData("42.0", 42)]
        [InlineData("23767.000", 23767)]
        [InlineData("32767.0000", short.MaxValue)]
        [InlineData("-32768.00000", short.MinValue)]
        [InlineData("0.0", 0)]
        [InlineData("-0.0", 0)]
        public void ShortWithTrailingZerosAfterDecimalPointCanBeDeserialized(string json, short expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }
    }

    public sealed class UShortTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse JSON numbers to .NET ushort instances.")]
        [InlineData("42", 42)]
        [InlineData(ushort.MaxValue / 2, ushort.MaxValue / 2)]
        [InlineData("56535", 56535)]
        [InlineData(ushort.MaxValue, ushort.MaxValue)]
        [InlineData(ushort.MinValue, ushort.MinValue)]
        [InlineData("0", 0)]
        [InlineData("-0", 0)]
        [InlineData(ushort.MinValue + ushort.MaxValue / 2, ushort.MinValue + ushort.MaxValue / 2)]
        public void UShortCanBeDeserializedCorrectly(string json, ushort expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        [Theory(DisplayName = "The deserializer must throw an exception when the JSON number exceeds the limits of the .NET ushort type.")]
        [InlineData("65536")] //ushort max value ( 65535 ) +1
        [InlineData("-1")]
        [InlineData("66535")]
        [InlineData("75535")]
        [InlineData("6553500000000")]
        public void ExceptionIsThrownWhenOverflowingUShortIsDeserialized(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<ushort>(json, $"Could not deserialize value {json} because it produces an overflow for type System.UInt16.");
        }

        [Theory(DisplayName = "The deserializer must be able to deserialize JSON numbers with trailing zeros to .NET ushort instances.")]
        [InlineData("42.0", 42)]
        [InlineData("56535.000", 56535)]
        [InlineData("65535.0000", ushort.MaxValue)]
        [InlineData("0.00000", ushort.MinValue)]
        [InlineData("0.0", 0)]
        [InlineData("-0.0", 0)]
        public void UShortWithTrailingZerosAfterDecimalPointCanBeDeserialized(string json, ushort expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }
    }

    public sealed class ByteTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse JSON numbers to .NET byte instances.")]
        [InlineData("42", 42)]
        [InlineData(byte.MaxValue / 2, byte.MaxValue / 2)]
        [InlineData("165", 165)]
        [InlineData(byte.MaxValue, byte.MaxValue)]
        [InlineData(byte.MinValue, byte.MinValue)]
        [InlineData("0", 0)]
        [InlineData("-0", 0)]
        [InlineData(byte.MinValue + byte.MaxValue / 2, byte.MinValue + byte.MaxValue / 2)]
        public void ByteCanBeDeserializedCorrectly(string json, byte expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        [Theory(DisplayName = "The deserializer must throw an exception when the JSON number exceeds the limits of the .NET byte type.")]
        [InlineData("256")] //byte max value ( 255 ) +1
        [InlineData("-1")]
        [InlineData("265")]
        [InlineData("355")]
        [InlineData("25500000000")]
        public void ExceptionIsThrownWhenOverflowingByteIsDeserialized(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<byte>(json, $"Could not deserialize value {json} because it produces an overflow for type System.Byte.");
        }

        [Theory(DisplayName = "The deserializer must be able to deserialize JSON numbers with trailing zeros to .NET byte instances.")]
        [InlineData("42.0", 42)]
        [InlineData("165.000", 165)]
        [InlineData("255.0000", byte.MaxValue)]
        [InlineData("0.00000", byte.MinValue)]
        [InlineData("0.0", 0)]
        [InlineData("-0.0", 0)]
        public void ByteWithTrailingZerosAfterDecimalPointCanBeDeserialized(string json, byte expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }
    }

    public sealed class SByteTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse JSON numbers to .NET sbyte instances.")]
        [InlineData("42", 42)]
        [InlineData(sbyte.MaxValue / 2, sbyte.MaxValue / 2)]
        [InlineData("37", 37)]
        [InlineData(sbyte.MaxValue, sbyte.MaxValue)]
        [InlineData(sbyte.MinValue, sbyte.MinValue)]
        [InlineData("0", 0)]
        [InlineData("-0", 0)]
        [InlineData(sbyte.MinValue + sbyte.MaxValue / 2, sbyte.MinValue + sbyte.MaxValue / 2)]
        public void SByteCanBeDeserializedCorrectly(string json, sbyte expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        [Theory(DisplayName = "The deserializer must throw an exception when the JSON number exceeds the limits of the .NET sbyte type.")]
        [InlineData("128")] //sbyte max value ( 127 ) +1
        [InlineData("-138")]
        [InlineData("137")]
        [InlineData("227")]
        [InlineData("12700000000")]
        public void ExceptionIsThrownWhenOverflowingSByteIsDeserialized(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<sbyte>(json, $"Could not deserialize value {json} because it produces an overflow for type System.SByte.");
        }

        [Theory(DisplayName = "The deserializer must be able to deserialize JSON numbers with trailing zeros to .NET sbyte instances.")]
        [InlineData("42.0", 42)]
        [InlineData("37.000", 37)]
        [InlineData("127.0000", sbyte.MaxValue)]
        [InlineData("-128.00000", sbyte.MinValue)]
        [InlineData("0.0", 0)]
        [InlineData("-0.0", 0)]
        public void SByteWithTrailingZerosAfterDecimalPointCanBeDeserialized(string json, sbyte expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }
    }

    public sealed class LongTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse JSON numbers to .NET long instances.")]
        [InlineData("42", 42L)]
        [InlineData(long.MaxValue / 2L, long.MaxValue / 2L)]
        [InlineData("8323372036854775807", 8323372036854775807)]
        [InlineData(long.MaxValue, long.MaxValue)]
        [InlineData(long.MinValue, long.MinValue)]
        [InlineData("0", 0L)]
        [InlineData("-0", 0L)]
        [InlineData(long.MinValue + long.MaxValue / 2L, long.MinValue + long.MaxValue / 2L)]
        public void LongCanBeDeserializedCorrectly(string json, long expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        [Theory(DisplayName = "The deserializer must throw an exception when the JSON number exceeds the limits of the .NET long type.")]
        [InlineData("9223372036854775808")] //long max value ( 9223372036854775807 ) +1
        [InlineData("-9223372036854775818")]
        [InlineData("9323372036854775807")]
        [InlineData("10223372036854775807")]
        [InlineData("922337203685477580700000000")]
        public void ExceptionIsThrownWhenOverflowingLongIsDeserialized(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<long>(json, $"Could not deserialize value {json} because it produces an overflow for type System.Int64.");
        }

        [Theory(DisplayName = "The deserializer must be able to deserialize JSON numbers with trailing zeros to .NET long instances.")]
        [InlineData("42.0", 42L)]
        [InlineData("8323372036854775807.000", 8323372036854775807)]
        [InlineData("9223372036854775807.0000", long.MaxValue)]
        [InlineData("-9223372036854775808.00000", long.MinValue)]
        [InlineData("0.0", 0L)]
        [InlineData("-0.0", 0L)]
        public void LongWithTrailingZerosAfterDecimalPointCanBeDeserialized(string json, long expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }
    }

    public sealed class ULongTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse JSON numbers to .NET ulong instances.")]
        [InlineData("42", 42ul)]
        [InlineData(ulong.MaxValue / 2, ulong.MaxValue / 2)]
        [InlineData("8323372036854775807", 8323372036854775807ul)]
        [InlineData(ulong.MaxValue, ulong.MaxValue)]
        [InlineData("0", 0ul)]
        [InlineData("-0", 0ul)]
        public void ULongCanBeDeserializedCorrectly(string json, ulong expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        [Theory(DisplayName = "The deserializer must throw an exception when the JSON number exceeds the limits of the .NET ulong type.")]
        [InlineData("18446744073709551616")] // ulong max value ( 18446744073709551615 ) +1
        [InlineData("-9223372036854775818")]
        [InlineData("922337203685477580700000000")]
        public void ExceptionIsThrownWhenOverflowingULongIsDeserialized(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<ulong>(json, $"Could not deserialize value {json} because it produces an overflow for type System.UInt64.");
        }

        [Theory(DisplayName = "The deserializer must be able to deserialize JSON numbers with trailing zeros to .NET ulong instances.")]
        [InlineData("42.0", 42ul)]
        [InlineData("8323372036854775807.000", 8323372036854775807ul)]
        [InlineData("18446744073709551615.0000", ulong.MaxValue)]
        [InlineData("0.0", 0ul)]
        [InlineData("-0.0", 0ul)]
        public void ULongWithTrailingZerosAfterDecimalPointCanBeDeserialized(string json, ulong expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }
    }
}