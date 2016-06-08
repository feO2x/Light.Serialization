using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class BooleanTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to deserialize JSON boolean values to .NET boolean values.")]
        [InlineData("true", true)]
        [InlineData("false", false)]
        public void BooleanValuesCanBeDeserializedCorrectly(string json, bool expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        [Theory(DisplayName = "The deserializer must throw a JsonDocumentException when the document contains an invalid JSON boolean value.")]
        [InlineData("treu")]
        [InlineData("teu")]
        [InlineData("tru")]
        [InlineData("tu")]
        [InlineData("tr")]
        [InlineData("fasle")]
        [InlineData("flse")]
        [InlineData("fse")]
        [InlineData("fa")]
        public void ExceptionIsThrownWhenFalseOrTrueAreNotSpelledCorrectly(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<bool>(json, $"Cannot deserialize value {json} to");
        }
    }
}