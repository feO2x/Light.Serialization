using FluentAssertions;
using Light.Serialization.Json.FrameworkExtensions;
using Xunit;

namespace Light.Serialization.Json.Tests.FrameworkExtensionTests
{
    public sealed class StringExtensionsTests
    {
        [Fact(DisplayName = "SurroundWithQuotationMarks must add quotation marks around the specified string.")]
        public void SurroundWithQuotationMarksWorksCorrectly()
        {
            var actual = "Foo".SurroundWithQuotationMarks();

            const string expected = "\"Foo\"";
            actual.Should().Be(expected);
        }

        [Theory(DisplayName = "SurroundWith must insert the specified character at the first and last position.")]
        [InlineData("!", "!Foo!")]
        [InlineData(".", ".Foo.")]
        [InlineData("ab", "abFooab")]
        public void SurroundWithWorksCorrectly(string surroundCharacters, string expected)
        {
            var actual = "Foo".SurroundWith(surroundCharacters);

            actual.Should().Be(expected);
        }

        [Theory(DisplayName = "IsSurroundedByQuotationMarks must return true when the specified string has quotation marks at the first and last positions, or else false.")]
        [InlineData("foo", false)]
        [InlineData("\"foo\"", true)]
        [InlineData("\"Foo", false)]
        [InlineData("Foo\"", false)]
        [InlineData("a", false)]
        [InlineData("ab", false)]
        [InlineData("\"a", false)]
        [InlineData("b\"", false)]
        public void IsSurroundedByQuotationMarksWorksCorrectly(string @string, bool expected)
        {
            var actual = @string.IsSurroundedByQuotationMarks();

            actual.Should().Be(expected);
        }
    }
}