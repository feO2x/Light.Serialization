using System;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class DateTimeOffsetTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse JSON strings that represent ISO 8601 date times to .NET DateTimeOffset instances.")]
        [MemberData(nameof(ValidDateTimeOffsetsData))]
        public void ValidDateTimeOffsets(string json, DateTimeOffset expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }

        public static readonly TestData ValidDateTimeOffsetsData =
            new[]
            {
                new object[] { "\"2016-03\"", new DateTimeOffset(new DateTime(2016, 3, 1), TimeSpan.Zero) },
                new object[] { "\"2016-03-08\"", new DateTimeOffset(new DateTime(2016, 3, 8), TimeSpan.Zero) },
                new object[] { "\"0011-08-19\"", new DateTimeOffset(new DateTime(11, 8, 19), TimeSpan.Zero) },
                new object[] { "\"2016-03-08T21:41+01\"", new DateTimeOffset(2016, 3, 8, 21, 41, 0, TimeSpan.FromHours(1d)) },
                new object[] { "\"2015-06-30T07:09Z\"", new DateTimeOffset(new DateTime(2015, 6, 30, 7, 9, 0, DateTimeKind.Utc)) },
                new object[] { "\"2015-06-30T07:09:42Z\"", new DateTimeOffset(new DateTime(2015, 6, 30, 7, 9, 42, DateTimeKind.Utc)) },
                new object[] { "\"2015-06-30T07:09:42.375Z\"", new DateTimeOffset(new DateTime(2015, 6, 30, 7, 9, 42, 375, DateTimeKind.Utc)) },
                new object[] { "\"2000-11-21T10:39:30-04\"", new DateTimeOffset(2000, 11, 21, 10, 39, 30, TimeSpan.FromHours(-4d)) },
                new object[] { "\"1999-12-31T23:59:59.999+10:30\"", new DateTimeOffset(1999, 12, 31, 23, 59, 59, 999, new TimeSpan(10, 30, 0)) }
            };

        [Theory(DisplayName = "JSON strings containing malformed date times result in an exception.")]
        [MemberData(nameof(InvalidDateTimeOffsetsData))]
        public void InvalidDateTimeOffsets(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<DateTimeOffset>(json, $"The specified token {json} does not represent a valid date time.");
        }

        public static readonly TestData InvalidDateTimeOffsetsData =
            new[]
            {
                new object[] { "\"201-01\"" },
                new object[] { "\"2000-13\"" },
                new object[] { "\"2014-01-32\"" },
                new object[] { "\"2011-02-29\"" },
                new object[] { "\"1987-04-31\"" },
                new object[] { "\"1955-01-00\"" },
                new object[] { "\"88-01-01\"" },
                new object[] { "\"1995-00\"" },
                new object[] { "\"1997-01-0114:15\"" },
                new object[] { "\"2001-04-17T3:15\"" },
                new object[] { "\"2001-04-17T03/15\"" },
                new object[] { "\"2001-04/17T03:15\"" },
                new object[] { "\"2001/04-17T03:15\"" },
                new object[] { "\"2001-04-17T03:15+1\"" },
                new object[] { "\"2001-04-17T03:15-1\"" },
                new object[] { "\"2010-09-20T05:14+01:1\"" },
                new object[] { "\"2010-09-20T05:14-01:7\"" },
                new object[] { "\"2016-03-08T05:14K01:00\"" },
                new object[] { "\"2016-03-08T05:14K10\"" },
                new object[] { "\"2016-03-08U05:14Z\"" },
                new object[] { "\"2016-03-08T05:14:30.889Zabc\"" },
                new object[] { "\"2016-03-08T05:14:30.889+011\"" },
                new object[] { "\"2016-03-08T05:14:30.889+10:300303\"" }
            };
    }
}