using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.SerializationTests
{
    public sealed class CollectionTests : BaseJsonSerializerTest
    {
        [Theory(DisplayName = "The serializer must be able to serialize .NET collections to JSON arrays.")]
        [MemberData(nameof(CollectionsTestData))]
        public void Collections(IEnumerable collection, string expectedJson)
        {
            CompareJsonToExpected(collection, expectedJson);
        }

        public static readonly TestData CollectionsTestData =
            new[]
            {
                new object[] { new[] { 1, 2, 3 }, "[\"$id:0\",1,2,3]" },
                new object[] { new List<string> { "Foo", null, "Bar" }, "[\"$id:0\",\"Foo\",null,\"Bar\"]" },
                new object[] { new ObservableCollection<bool> { true, false }, "[\"$id:0\",true,false]" }
            };

        [Theory(DisplayName = "The client should be able to disable the metadata string so that the serializer does not include them in the JSON array.")]
        [MemberData(nameof(DisableMetadataTestData))]
        public void DisableMetadata(IEnumerable collection, string expectedJson)
        {
            DisableObjectReferencePreservation();

            CompareJsonToExpected(collection, expectedJson);
        }

        public static readonly TestData DisableMetadataTestData =
            new[]
            {
                new object[] { new[] { 4.2, 3.99, 810.0 }, "[4.2,3.99,810.0]" },
                new object[] { new List<string> { "Foo", "Bar" }, "[\"Foo\",\"Bar\"]" }
            };
    }
}