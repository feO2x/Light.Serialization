using System;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

namespace Light.Serialization.Json.Tests.SerializationTests
{
    public sealed class TimeSpanTests : BaseJsonSerializerTest
    {
        //https://en.wikipedia.org/wiki/ISO_8601

        [Theory(DisplayName = "TimeSpans are serialized according to the ISO 8601 duration rules")]
        [MemberData(nameof(TimeSpansTestData))]
        public void TimeSpans(TimeSpan timeSpan, string expectedJson)
        {
            CompareJsonToExpected(timeSpan, expectedJson);
        }

        public static readonly TestData TimeSpansTestData =
            new[]
            {
                new object[] { new TimeSpan(2, 0, 0, 0, 0), "\"P2D\"" },
                new object[] { new TimeSpan(81, 0, 0, 0, 0), "\"P81D\"" },
                new object[] { new TimeSpan(0, 1, 30, 0, 0), "\"PT1H30M\"" },
                new object[] { new TimeSpan(0, 17, 15, 0, 0), "\"PT17H15M\"" },
                new object[] { new TimeSpan(0, 0, 0, 1, 0), "\"PT1S\"" },
                new object[] { new TimeSpan(0, 0, 0, 2, 500), "\"PT2.500S\"" },
                new object[] { new TimeSpan(0, 0, 0, 13, 693), "\"PT13.693S\"" },
                new object[] { new TimeSpan(2, 10, 35, 47, 400), "\"P2DT10H35M47.400S\"" }
            };
    }
}