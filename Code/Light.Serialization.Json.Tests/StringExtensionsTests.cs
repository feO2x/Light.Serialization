using FluentAssertions;
using Light.Serialization.Json.FrameworkExtensions;
using Xunit;

namespace Light.Serialization.Json.Tests
{
    public sealed class StringExtensionsTests
    {
        [Fact(DisplayName = "SurroundWithQuotationMarks adds quotation marks at the beginning and the end of the specified string.")]
        public void SurroundWithQuotationMarks()
        {
            var actual = "Foo".SurroundWithQuotationMarks();

            const string expected = "\"Foo\"";
            actual.Should().Be(expected);
        }

        [Theory(DisplayName = "SurroundWith adds the specified string at the beginning and at the end of the specified string.")]
        [InlineData("!", "!Foo!")]
        [InlineData(".", ".Foo.")]
        [InlineData("ab", "abFooab")]
        public void SurroundWith(string surroundCharacters, string expected)
        {
            var actual = "Foo".SurroundWith(surroundCharacters);

            actual.Should().Be(expected);
        }

        [Theory(DisplayName = "IsSurroundedByQuotationMarks returns true if there are quotation marks at the first and last position of the specified string, else false.")]
        [InlineData("foo", false)]
        [InlineData("\"foo\"", true)]
        [InlineData("\"Foo", false)]
        [InlineData("Foo\"", false)]
        [InlineData("a", false)]
        [InlineData("ab", false)]
        [InlineData("\"a", false)]
        [InlineData("b\"", false)]
        public void IsSurroundedByQuotationMarks(string @string, bool expected)
        {
            var actual = @string.IsSurroundedByQuotationMarks();

            actual.Should().Be(expected);
        }
    }
}