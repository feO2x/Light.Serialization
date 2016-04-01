using System;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

namespace Light.Serialization.Json.Tests.SerializationTests
{
    public sealed class DateTimeTests : BaseJsonSerializerTest
    {
        //https://en.wikipedia.org/wiki/ISO_8601

        [Theory(DisplayName = "UTC date times are serialized according to the ISO 8601.")]
        [MemberData(nameof(UtcTestData))]
        public void UtcDateTime(DateTime dateTime, string expectedJson)
        {
            CompareJsonToExpected(dateTime, expectedJson);
        }

        public static readonly TestData UtcTestData =
            new[]
            {
                new object[] { new DateTime(2016, 2, 2, 0, 0, 0, DateTimeKind.Utc), "\"2016-02-02\"" },
                new object[] { new DateTime(2016, 2, 2, 12, 22, 0, DateTimeKind.Utc), "\"2016-02-02T12:22Z\"" },
                new object[] { new DateTime(2016, 2, 2, 12, 23, 15, DateTimeKind.Utc), "\"2016-02-02T12:23:15Z\"" },
                new object[]
                { new DateTime(2016, 2, 2, 13, 54, 50, 300, DateTimeKind.Utc), "\"2016-02-02T13:54:50.300Z\"" },
                new object[] { new DateTime(2016, 2, 28, 0, 15, 0, DateTimeKind.Utc), "\"2016-02-28T00:15Z\"" },
                new object[] { new DateTime(10, 12, 24, 0, 0, 0, DateTimeKind.Utc), "\"0010-12-24\"" },
                new object[] { new DateTime(3, 12, 24, 0, 0, 0, 700, DateTimeKind.Utc), "\"0003-12-24T00:00:00.700Z\"" }
            };

        [Theory(DisplayName = "Local date times are serialized according to ISO 8601.")]
        [MemberData(nameof(LocalTestData))]
        public void LocalDateTime(DateTime dateTime, string expectedJson)
        {
            ReplaceTimeZoneInfoInDateTimeFormatter(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

            CompareJsonToExpected(dateTime, expectedJson);
        }

        public static readonly TestData LocalTestData =
            new[]
            {
                new object[] { new DateTime(2016, 2, 2, 13, 24, 0, DateTimeKind.Local), "\"2016-02-02T13:24+01\"" },
                new object[] { new DateTime(2016, 2, 2, 13, 25, 48, DateTimeKind.Local), "\"2016-02-02T13:25:48+01\"" },
                new object[]
                { new DateTime(2016, 2, 2, 13, 56, 25, 780, DateTimeKind.Local), "\"2016-02-02T13:56:25.780+01\"" },
                new object[] { new DateTime(2016, 6, 15, 12, 0, 0, DateTimeKind.Local), "\"2016-06-15T12:00+02\"" },
                new object[]
                { new DateTime(2016, 6, 15, 0, 34, 3, 350, DateTimeKind.Local), "\"2016-06-15T00:34:03.350+02\"" }
            };

        [Theory(DisplayName = "Unspecified date times are serialized without any ISO 8601 offset.")]
        [MemberData(nameof(UnspecifiedTestData))]
        public void UnspecifiedDateTime(DateTime dateTime, string expectedJson)
        {
            CompareJsonToExpected(dateTime, expectedJson);
        }

        public static readonly TestData UnspecifiedTestData =
            new[]
            {
                new object[] { new DateTime(2016, 2, 2, 13, 25, 48, DateTimeKind.Unspecified), "\"2016-02-02T13:25:48\"" },
                new object[] { new DateTime(2016, 2, 2, 13, 25, 0, DateTimeKind.Unspecified), "\"2016-02-02T13:25\"" },
                new object[]
                { new DateTime(2016, 2, 2, 13, 57, 0, 100, DateTimeKind.Unspecified), "\"2016-02-02T13:57:00.100\"" },
                new object[]
                { new DateTime(2016, 2, 2, 13, 57, 0, 45, DateTimeKind.Unspecified), "\"2016-02-02T13:57:00.045\"" }
            };
    }
}