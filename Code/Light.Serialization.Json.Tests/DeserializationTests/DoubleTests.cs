using FluentAssertions;
using Xunit;

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class DoubleTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse JSON numbers that represent floating-point numbers to .NET double instances.")]
        [InlineData("42.0", 42.0, 0.1)]
        [InlineData("0.75", 0.75, 0.01)]
        [InlineData("-146.311", -146.311, 0.001)]
        [InlineData("-1.7976931348623157", -1.7976931348623157, 0.0000000000000001)]
        [InlineData("32", 32.0, 0.1)]
        [InlineData("1.625e10", 1.625E10, 0.1)]
        [InlineData("3.141E-3", 3.141E-3, 0.1E-7)]
        public void DoubleValuesCanBeDeserializedCorrectly(string json, double expected, double tolerance)
        {
            var actual = GetDeserializedJson<double>(json);
            actual.Should().BeApproximately(expected, tolerance);
        }

        [Theory(DisplayName = "The deserializer must throw a JsonDocumentException when the token is not a valid JSON number.")]
        [InlineData("42.0.")]
        [InlineData("3k.0")]
        [InlineData("3.141E1f")]
        public void ExceptionIsThrownWhenNumberCannotBeParsed(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<double>(json, $"Cannot deserialize value {json} to");
        }
    }
}