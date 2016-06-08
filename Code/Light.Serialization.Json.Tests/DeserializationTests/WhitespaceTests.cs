using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class WhitespaceTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "Space before a single value must be ignored by the deserializer.")]
        [InlineData(" 42", 42)]
        [InlineData("  false", false)]
        [InlineData("   true", true)]
        [InlineData(" 0.8863", 0.8863)]
        [InlineData("      \"Hello\"", "Hello")]
        [InlineData("   null", null)]
        public void SpacesBeforeSingleValueIsIgnoredCorrectly<T>(string json, T expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        [Theory(DisplayName = "Space after a single value must be ignored by the deserializer.")]
        [InlineData("-42 ", -42)]
        [InlineData("true  ", true)]
        [InlineData("false    ", false)]
        [InlineData("3255.25 ", 3255.25)]
        [InlineData("\"Hi there\"  ", "Hi there")]
        [InlineData("null ", null)]
        public void SpacesAfterSingleValueAreIgnoredCorrectly<T>(string json, T expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }
    }
}