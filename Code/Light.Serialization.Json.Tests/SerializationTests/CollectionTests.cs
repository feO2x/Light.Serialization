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
                new object[] { new[] { 1, 2, 3 }, "[\"$id\",0,\"$type\",{\"name\":\"array\",\"arrayType\":\"int32\",\"arrayLength\":3},1,2,3]" },
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

        [Fact(DisplayName = "The serializer must include the array rank as an integer number and the length as a JSON array for multidimensional arrays in the corresponding metadata sections.")]
        public void MultidimensionalArray()
        {
            UseDomainFriendlyNames();
            var threeByTwoArray = new int[2, 3];
            threeByTwoArray[0, 0] = 5;
            threeByTwoArray[0, 1] = 6;
            threeByTwoArray[0, 2] = 7;
            threeByTwoArray[1, 0] = 8;
            threeByTwoArray[1, 1] = 9;
            threeByTwoArray[1, 2] = 10;
            /*
             * This array is the same as
             * 5  6  7
             * 8  9 10
             */

            const string expectedJson = "[\"$id\",0,\"$type\",{\"name\":\"array\",\"arrayType\":\"int32\",\"arrayRank\":2,\"arrayLength\":[2,3]},5,6,7,8,9,10]";

            CompareJsonToExpected(threeByTwoArray, expectedJson);
        }

        [Fact(DisplayName = "The serializer must include the length as a JSON number for jagged arrays in the corresponding metadata sections.")]
        public void JaggedArray()
        {
            UseDomainFriendlyNames();
            var jaggedArray = new int[2][];
            jaggedArray[0] = new[] { 42, 87, -111 };
            jaggedArray[1] = new[] { 66 };

            const string expectedJson = "[\"$id\",0,\"$type\",{\"name\":\"array\",\"arrayType\":{\"name\":\"array\",\"arrayType\":\"int32\"},\"arrayLength\":2},[\"$id\",1,\"$type\",{\"name\":\"array\",\"arrayType\":\"int32\",\"arrayLength\":3},42,87,-111],[\"$id\",2,\"$type\",{\"name\":\"array\",\"arrayType\":\"int32\",\"arrayLength\":1},66]]";

            CompareJsonToExpected(jaggedArray, expectedJson);
        }
    }
}