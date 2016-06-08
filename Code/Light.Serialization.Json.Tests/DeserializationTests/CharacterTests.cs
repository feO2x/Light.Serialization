using System.Collections.Generic;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class CharacterTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to deserialize JSON strings containing a single character to .NET char instances.")]
        [InlineData("\"c\"", 'c')]
        [InlineData("\"4\"", '4')]
        [InlineData("\"Y\"", 'Y')]
        [InlineData("\" \"", ' ')]
        public void SimpleCharacterCanBeDeserializedCorrectly(string json, char expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        [Theory(DisplayName = "The deserializer must be able to deserialize JSON strings that represent escaped special characters to .NET char instances.")]
        [InlineData("\"\\\\\"", '\\')] // Backslash
        [InlineData("\"\\\"\"", '"')] // Quotation mark
        [InlineData("\"\b\"", '\b')] // Backspace
        [InlineData("\"\f\"", '\f')] // Form feed
        [InlineData("\"\n\"", '\n')] // Line feed
        [InlineData("\"\r\"", '\r')] // Carriage return
        [InlineData("\"\t\"", '\t')] // Horizontal tab
        [InlineData("\"\\/\"", '/')] // Slash
        public void SingleEscapedCharactersCanBeDeserializedCorrectly(string json, char expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        [Theory(DisplayName = "The deserializer must be able to deserialize JSON strings that represent four-digit-hexadecimal escape sequences to .NET char instances.")]
        [InlineData("\"\\u2028\"", '\u2028')] // Line Separator
        [InlineData("\"\\u2029\"", '\u2029')] // Paragraph Separator
        [MemberData(nameof(UnicodeC0Block))] // Unicode C0 Block
        [MemberData(nameof(UnicodeC1Block))] // Unicode C1 Block
        public void HexadecimalEscapedCharactersCanBeDeserializedCorrectly(string json, char expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        public static TestData UnicodeC0Block => CreateCharacterSequence(0, ' '); // Unicode C0 block starts at zero and ends at whitespace

        public static TestData UnicodeC1Block => CreateCharacterSequence('\u007f', '\u00A0'); // Unicode C1 block starts at x007f and ends at x009f (included)

        private static TestData CreateCharacterSequence(int from, int to)
        {
            var testData = new List<object[]>();

            for (var i = from; i < to; i++)
            {
                testData.Add(new object[] { string.Format($"\"\\u{i:X4}\""), (char) i });
            }
            return testData;
        }


        [Theory(DisplayName = "The deserializer must throw a JsonDocumentException when the JSON string does not represent a valid .NET character.")]
        [InlineData("\"Hello\"")] // String with five characters
        [InlineData("\"c15\"")] // String with three characters
        [InlineData("\"\\bt\"")] // Escaped character (\b) with additional character (t)
        [InlineData("\"\\x\"")] // Incorrect single escape character (\x is not valid)
        [InlineData("\"\"")] // Empty JSON string
        [InlineData("\"\\u1\"")] // Incorrect hexadecimal character (1)
        [InlineData("\"\\u03\"")] // Incorrect hexadecimal character (2)
        [InlineData("\"\\u01C\"")] // Incorrect hexadecimal character (3)
        public void ExceptionIsThrownWhenJsonStringIsNotASingleCharacter(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<char>(json, $"Cannot deserialize value {json} to");
        }
    }
}