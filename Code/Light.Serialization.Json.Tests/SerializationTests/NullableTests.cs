using System;
using Light.Serialization.Json.FrameworkExtensions;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.SerializationTests
{
    public sealed class NullableTests : BaseJsonSerializerTest
    {
        [Theory(DisplayName = "The serializer must be able to serialize Nullable values.")]
        [InlineData(42, "42")]
        [InlineData(-192, "-192")]
        [InlineData(null, "null")]
        public void Nullables(int? value, string expectedJson)
        {
            CompareJsonToExpected(value, expectedJson);
        }

        [Theory(DisplayName = "The serializer must be able to serialize nullable GUID values.")]
        [MemberData(nameof(NullableGuidData))]
        public void NullableGuid(Guid? value, string expectedJson)
        {
            CompareJsonToExpected(value, expectedJson);
        }

        public static TestData NullableGuidData
        {
            get
            {
                var testRunData = new object[2][];
                var guid = Guid.NewGuid();
                var guidJson = guid.ToString().SurroundWithQuotationMarks();
                testRunData[0] = new object[] { guid, guidJson };
                testRunData[1] = new object[] { new Guid?(), JsonSymbols.Null };
                return testRunData;
            }
        }
    }
}