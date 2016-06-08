using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class DecimalTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse JSON numbers that represent floating point numbers to .NET decimal instances.")]
        [MemberData(nameof(DecimalTestData))]
        public void DecimalValuesCanBeDeserializedCorrectly(string json, decimal expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        public static readonly TestData DecimalTestData =
            new[]
            {
                new object[] { "42.0", 42m },
                new object[] { "42.01", 42.01m },
                new object[] { "42.0001", 42.0001m },
                new object[] { "79228162514264337593543950335.0", decimal.MaxValue },
                new object[] { "-79228162514264337593543950335.0", decimal.MinValue },
                new object[] { "-42.00200", -42.00200m }
            };

        [Theory(DisplayName = "The deserializer must throw a DeserializationException when JSON numbers that contain malformed floating point numbers or numbers that are to exceed the decimal limits are parsed.")]
        [InlineData("42.0.")]
        [InlineData("42.0m")]
        [InlineData("3k.0")]
        [InlineData("3.141e1")]
        [InlineData("79228162514264337593543950336.0")]
        [InlineData("-79228162514264337593543950336.0")]
        public void ExceptionIsThrownWhenNumberCannotBeParsed(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<decimal>(json, $"Cannot deserialize value {json} to");
        }
    }
}