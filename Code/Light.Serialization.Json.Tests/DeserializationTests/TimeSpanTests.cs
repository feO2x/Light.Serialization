using System;
using FluentAssertions;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class TimeSpanTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse JSON strings that represent valid ISO 8601 durations to .NET TimeSpans.")]
        [MemberData(nameof(ValidTimeSpansData))]
        public void ValidTimeSpans(string json, TimeSpan expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        public static readonly TestData ValidTimeSpansData =
            new[]
            {
                new object[] { "\"P12D\"", new TimeSpan(12, 0, 0, 0) },
                new object[] { "\"P3D\"", new TimeSpan(3, 0, 0, 0) },
                new object[] { "\"PT17H\"", new TimeSpan(17, 0, 0) },
                new object[] { "\"PT24H\"", new TimeSpan(24, 0, 0) },
                new object[] { "\"PT5M\"", new TimeSpan(0, 5, 0) },
                new object[] { "\"PT59M\"", new TimeSpan(0, 59, 0) },
                new object[] { "\"PT30S\"", new TimeSpan(0, 0, 30) },
                new object[] { "\"PT2S\"", new TimeSpan(0, 0, 2) },
                new object[] { "\"PT04S\"", new TimeSpan(0, 0, 4) },
                new object[] { "\"PT00.300S\"", new TimeSpan(0, 0, 0, 0, 300) },
                new object[] { "\"PT19.588S\"", new TimeSpan(0, 0, 0, 19, 588) },
                new object[] { "\"PT7.132S\"", new TimeSpan(0, 0, 0, 7, 132) },
                new object[] { "\"P1DT5H48M53.500S\"", new TimeSpan(1, 5, 48, 53, 500) }
            };

        [Theory(DisplayName = "The deserializer must throw a DeserializationException when the JSON string contains invalid ISO 8601 durations.")]
        [MemberData(nameof(InvalidTimeSpansData))]
        public void InvalidTimeSpans(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<TimeSpan>(json, $"The specified token {json} does not represent a valid time span.");
        }

        public static readonly TestData InvalidTimeSpansData =
            new[]
            {
                new object[] { "\"12D\"" },
                new object[] { "\"6D\"" },
                new object[] { "\"P12D40M\"" },
                new object[] { "\"P12D3H\"" },
                new object[] { "\"P4M\"" },
                new object[] { "\"P07H\"" },
                new object[] { "\"P12S\"" },
                new object[] { "\"PT30.0S\"" },
                new object[] { "\"P17DT5H-14M30S\"" }
            };

        [Theory(DisplayName = "A JSON string containing a valid ISO 8601 duration value must be parsed to a TimeSpan even when the requested type is a base type of TimeSpan.")]
        [MemberData(nameof(TimeSpanReferencedAsBaseTypeData))]
        public void TimeSpanReferencedAsBaseType(string json, Type requestedType, TimeSpan expected)
        {
            var actual = GetDeserializedJson(json, requestedType);

            actual.As<TimeSpan>().Should().Be(expected);
        }

        public static readonly TestData TimeSpanReferencedAsBaseTypeData =
            new[]
            {
                new object[] { "\"P12D\"", typeof(object), new TimeSpan(12, 0, 0, 0) },
                new object[] { "\"P3D\"", typeof(ValueType), new TimeSpan(3, 0, 0, 0) },
                new object[] { "\"PT17H\"", typeof(IComparable), new TimeSpan(17, 0, 0) },
                new object[] { "\"PT00.300S\"", typeof(IComparable<TimeSpan>), new TimeSpan(0, 0, 0, 0, 300) },
                new object[] { "\"PT7.132S\"", typeof(IFormattable), new TimeSpan(0, 0, 0, 7, 132) },
                new object[] { "\"P1DT5H48M53.500S\"", typeof(IEquatable<TimeSpan>), new TimeSpan(1, 5, 48, 53, 500) }
            };
    }
}