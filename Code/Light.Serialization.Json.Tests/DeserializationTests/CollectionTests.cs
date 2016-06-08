using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class CollectionTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse JSON documents containing an array of JSON numbers.")]
        [MemberData(nameof(IntegerCollections))]
        public void IntegerCollectionsCanBeDeserialized(string json, int[] expected)
        {
            var actual = GetDeserializedJson<List<int>>(json);
            actual.ShouldAllBeEquivalentTo(expected);
        }

        public static readonly TestData IntegerCollections =
            new[]
            {
                new object[] { "[1,2,3]", new[] { 1, 2, 3 } },
                new object[] { "[9,83]", new[] { 9, 83 } },
                new object[] { "[138, 145, 633]", new[] { 138, 145, 633 } },
                new object[] { "[\r\n  1,\r\n  2,\r\n  3\r\n]", new[] { 1, 2, 3 } },
                new object[] { "[]", new int[0] }
            };

        [Theory(DisplayName = "The deserializer must be able to parse JSON documents containing an array of JSON strings.")]
        [MemberData(nameof(StringCollections))]
        public void StringCollectionsCanBeDeserializedCorrectly(string json, string[] expected)
        {
            var actual = GetDeserializedJson<List<string>>(json);
            actual.ShouldAllBeEquivalentTo(expected);
        }

        public static readonly TestData StringCollections =
            new[]
            {
                new object[] { "[\"Hello\", \"World\"]", new[] { "Hello", "World" } },
                new object[] { "[\"3\"]", new[] { "3" } },
                new object[] { "[\"Hey\\u2028you\", \"What's\\tup?\"]", new[] { "Hey\u2028you", "What's\tup?" } },
                new object[] { "[]", new string[0] }
            };

        [Theory(DisplayName = "The deserializer must be able to parse JSON documents containing an array of JSON floating point numbers.")]
        [MemberData(nameof(DoubleCollections))]
        public void DoubleCollectionsCanBeDeserializedCorrectly(string json, double[] expected, double tolerance)
        {
            var actual = GetDeserializedJson<List<double>>(json);
            actual.Should().HaveCount(expected.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                actual[i].Should().BeApproximately(expected[i], tolerance);
            }
        }

        public static readonly TestData DoubleCollections =
            new[]
            {
                new object[] { "[12.23, 3.11, 15556.99]", new[] { 12.23, 3.11, 15556.99 }, 0.001 },
                new object[] { "[3.141,1.6e10]", new[] { 3.141, 1.6e10 }, 0.0001 },
                new object[] { "[]", new double[0], 0.0 }
            };
    }
}