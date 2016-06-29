using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Light.Serialization.Json.LowLevelReading;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class ErrorLocationTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to indicate the location of an error when a JsonDocumentException is thrown.")]
        [InlineData("nll", typeof(object), 1, 1)]
        [InlineData("2k4443", typeof(int), 1, 1)]
        [MemberData(nameof(ErrorLocationData))]
        public void ErrorLocation(string json, Type requestedType, int lineNumber, int position)
        {
            var deserializer = GetDeserializer();

            Action act = () => deserializer.Deserialize(json, requestedType);

            act.ShouldThrow<ErroneousTokenException>()
               .And.Message.Should().Contain($"The error occured in line {lineNumber} at position {position}");
        }

        public static readonly TestData ErrorLocationData =
            new[]
            {
                new object[]
                {
                    new StringBuilder().AppendLine("{")
                                       .AppendLine("  \"value1\": 1,")
                                       .AppendLine("  value2: \"Foo\",")
                                       .AppendLine("}")
                                       .ToString(),
                    typeof(Dictionary<string, object>), 3, 3
                },
                new object[]
                {
                    new StringBuilder().AppendLine("[")
                                       .AppendLine("    4,")
                                       .AppendLine("    781,")
                                       .AppendLine("    45jfjf")
                                       .AppendLine("]")
                                       .ToString(),
                    typeof(List<int>), 4, 5
                },
                new object[]
                {
                    new StringBuilder().AppendLine("[")
                                       .AppendLine("  -14,")
                                       .AppendLine("  2.22345,")
                                       .AppendLine("  3.14")
                                       .AppendLine("]")
                                       .ToString(),
                    typeof(IEnumerable<int>), 3, 3
                },
                new object[]
                {
                    new StringBuilder().AppendLine("[")
                                       .AppendLine("  2,")
                                       .AppendLine("  1,")
                                       .AppendLine("  3.14")
                                       .AppendLine("]")
                                       .ToString(),
                    typeof(IEnumerable<uint>), 4, 3
                },
                new object[]
                {
                    new StringBuilder().AppendLine("{")
                                       .AppendLine("    [\"invalidToken\"]: \"Foo\"")
                                       .AppendLine("}")
                                       .ToString(),
                    typeof(Dictionary<object, object>), 2, 5
                }
            };
    }
}