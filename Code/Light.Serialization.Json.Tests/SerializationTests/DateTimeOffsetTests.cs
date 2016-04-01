using System;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

namespace Light.Serialization.Json.Tests.SerializationTests
{
    public sealed class DateTimeOffsetTests : BaseJsonSerializerTest
    {
        //https://en.wikipedia.org/wiki/ISO_8601

        [Theory(DisplayName = "UTC date time offsets are serialized according to ISO 8601.")]
        [MemberData(nameof(UtcDateTimeOffsetData))]
        public void UtcDateTimeOffset(DateTimeOffset dateTimeOffset, string expectedJson)
        {
            CompareJsonToExpected(dateTimeOffset, expectedJson);
        }

        public static readonly TestData UtcDateTimeOffsetData =
            new[]
            {
                new object[] { new DateTimeOffset(new DateTime(2016, 3, 7, 0, 0, 0, DateTimeKind.Utc)), "\"2016-03-07\"" },
                new object[] { new DateTimeOffset(new DateTime(2016, 3, 7, 17, 4, 0, DateTimeKind.Utc)), "\"2016-03-07T17:04Z\"" },
                new object[] { new DateTimeOffset(new DateTime(2016, 3, 7, 17, 5, 23, DateTimeKind.Utc)), "\"2016-03-07T17:05:23Z\"" },
                new object[] { new DateTimeOffset(new DateTime(2016, 3, 7, 17, 9, 45, 380, DateTimeKind.Utc)), "\"2016-03-07T17:09:45.380Z\"" },
                new object[] { new DateTimeOffset(new DateTime(10, 12, 24, 0, 0, 0, DateTimeKind.Utc)), "\"0010-12-24\"" },
                new object[] { new DateTimeOffset(new DateTime(3, 12, 24, 0, 0, 0, 700, DateTimeKind.Utc)), "\"0003-12-24T00:00:00.700Z\"" }
            };

        [Theory(DisplayName = "Local date time offsets are serialized according to ISO 8601.")]
        [MemberData(nameof(LocalDateTimeOffsetData))]
        public void LocalDateTimeOffset(DateTimeOffset dateTimeOffset, string expectedJson)
        {
            CompareJsonToExpected(dateTimeOffset, expectedJson);
        }

        public static readonly TestData LocalDateTimeOffsetData =
            new[]
            {
                new object[] { new DateTimeOffset(new DateTime(2016, 3, 7, 14, 19, 0), TimeSpan.FromHours(-3d)), "\"2016-03-07T14:19-03\"" },
                new object[] { new DateTimeOffset(new DateTime(2016, 3, 7, 18, 21, 30), TimeSpan.FromHours(1d)), "\"2016-03-07T18:21:30+01\"" },
                new object[] { new DateTimeOffset(new DateTime(2016, 4, 7, 15, 0, 40, 360), TimeSpan.FromHours(10.5d)), "\"2016-04-07T15:00:40.360+10:30\"" },
                new object[] { new DateTimeOffset(new DateTime(2016, 4, 7, 15, 0, 40, 360), TimeSpan.FromHours(-9.5d)), "\"2016-04-07T15:00:40.360-09:30\"" }
            };
    }
}