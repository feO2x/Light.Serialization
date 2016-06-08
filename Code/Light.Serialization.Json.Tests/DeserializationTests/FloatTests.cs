using FluentAssertions;
using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class FloatTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse JSON numbers that represent floating point numbers to .NET float instances.")]
        [InlineData("3.402823e38", 3.402823e38f, 0.000001f)]
        [InlineData("2.402823e38", 2.402823e38f, 0.000001f)]
        [InlineData("2.0e38", 2.0e38f, 0.1f)]
        [InlineData("0.0", 0.0f, 0.1f)]
        [InlineData("-2.402823e38", -2.402823e38f, 0.000001f)]
        [InlineData("-3.402823e38", -3.402823e38f, 0.000001f)]
        [InlineData("-2.0e38", -2.0e38, 0.1f)]
        public void FloatValuesCanBeDeserializedCorrectly(string json, float expected, float tolerance)
        {
            var actual = GetDeserializedJson<float>(json);
            actual.Should().BeApproximately(expected, tolerance);
        }

        [Theory(DisplayName = "The deserializer must throw a JsonDocumentException when the JSON number represents malformed floating point numbers or numbers that exceed the float limits.")]
        [InlineData("42.0.")]
        [InlineData("3k.0")]
        [InlineData("3.141E1f")]
        [InlineData("-3.502823e38")]
        [InlineData("-3.402823e39")]
        [InlineData("-3.402824e38")]
        [InlineData("3.502823e38")]
        [InlineData("3.402825e38")]
        [InlineData("3.402823e39")]
        [InlineData("3.502823e38e")]
        public void ExceptionIsThrownWhenNumberCannotBeParsed(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<float>(json, $"Cannot deserialize value {json} to");
        }
    }
}