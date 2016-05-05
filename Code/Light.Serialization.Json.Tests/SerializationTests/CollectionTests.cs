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
            UseDomainFriendlyNames();

            CompareJsonToExpected(collection, expectedJson);
        }

        public static readonly TestData CollectionsTestData =
            new[]
            {
                new object[] { new[] { 1, 2, 3 }, "[\"$id\",0,\"$type\",{\"name\":\"array\",\"arrayType\":\"int32\",\"arrayRank\":1,\"arrayLenght\":3},1,2,3]" },
                new object[] { new List<string> { "Foo", null, "Bar" }, "[\"$id\",0,\"$type\",{\"name\":\"genericList\",\"typeArguments\":[\"string\"]},\"Foo\",null,\"Bar\"]" },
                new object[] { new ObservableCollection<bool> { true, false }, "[\"$id\",0,\"$type\",{\"name\":\"observableGenericList\",\"typeArguments\":[\"bool\"]},true,false]" }
            };

        [Theory(DisplayName = "The client should be able to disable the metadata string so that the serializer does not include them in the JSON array.")]
        [MemberData(nameof(DisableMetadataTestData))]
        public void DisableMetadata(IEnumerable collection, string expectedJson)
        {
            DisableObjectReferencePreservation();
            DisableTypeMetadata();

            CompareJsonToExpected(collection, expectedJson);
        }

        public static readonly TestData DisableMetadataTestData =
            new[]
            {
                new object[] { new[] { 4.2, 3.99, 810.0 }, "[4.2,3.99,810.0]" },
                new object[] { new List<string> { "Foo", "Bar" }, "[\"Foo\",\"Bar\"]" }
            };

        // TODO: add tests for multidimensional and jagged arrays
        //[Fact(DisplayName = "The serializer must serialize include the array length of multidimensional arrays in the corresponding metadata sections.")]
        //public void MultidimensionalArrays()
        //{
        //    UseDomainFriendlyNames();
        //    var twoByTwoArray = new int[,];
        //    var arrayContent = 5;
        //    for (var i = 0; i < 2; i++)
        //    {
        //        twoByTwoArray[i] = new[] { arrayContent++, arrayContent++, arrayContent++ };
        //    }
        //    /*
        //     * This array is the same as
        //     * 5  6  7
        //     * 8  9 10
        //     */

        //    const string expectedJson = "[\"$id\",0,\"$type\",{\"name\":\"array\",\"arrayType\":\"int32\",\"arrayRank\":2,\"arrayLenght\":2},[\"$id\",1,\"$type\",{\"name\":\"array\",\"arrayType\":\"int32\",\"arrayRank\":1,\"arrayLenght\":3},5,6,7][\"$id\",2,\"$type\",{\"name\":\"array\",\"arrayType\":\"int32\",\"arrayRank\":1,\"arrayLenght\":3},8,9,10]]";

        //    CompareJsonToExpected(twoByTwoArray, expectedJson);
        //}
    }
}