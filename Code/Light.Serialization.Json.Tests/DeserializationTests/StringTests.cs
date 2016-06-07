using Xunit;

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class StringTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse JSON strings containing no escape sequences to .NET strings.")]
        [InlineData("\"Hello\"", "Hello")]
        [InlineData("\"World\"", "World")]
        [InlineData("\"2\"", "2")]
        [InlineData("\"\"", "")]
        public void SimpleStringCanBeDeserializedCorrectly(string json, string expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        // http://mentalfloss.com/article/49238/7-sentences-sound-crazy-are-still-grammatical
        [Theory(DisplayName = "The deserializer must be able to parse JSON strings with escape sequences to .NET strings.")]
        [InlineData("\"What now,\\tbrown cow?\"", "What now,\tbrown cow?")] // Single \t
        [InlineData("\"Everything is okay in the end.\\u2028If it's not okay, then it's not the end.\"", "Everything is okay in the end.\u2028If it's not okay, then it's not the end.")] // Single hexadecimal escape sequence (Line feed)
        [InlineData("\"The complex houses married\\nand single soldiers\\nand their families.\"", "The complex houses married\nand single soldiers\nand their families.")] // Multiple \n (in fact it's only 2 :-/ )
        [InlineData("\"\\u0002Buffalo Buffalo Buffalo\\u000bBuffalo Buffalo Buffalo\\u0007Buffalo Buffalo\\u0017\"", "\u0002Buffalo Buffalo Buffalo\u000bBuffalo Buffalo Buffalo\u0007Buffalo Buffalo\u0017")] // Multiple hexadecimal escape sequences
        [InlineData("\"The horse\\n\\rraced past the barn\\n\\rfell\\u2029\"", "The horse\n\rraced past the barn\n\rfell\u2029")] // Mixed escape sequences
        public void StringWithEscapeSequencesCanBeDeserializedCorrectly(string json, string expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }
    }
}