using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Light.Serialization.Json.Tests.SampleTypes;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.SerializationTests
{
    public sealed class HumanReadableJsonTests : BaseJsonSerializerTest
    {
        public static readonly TestData Testdata =
            new[]
            {
                new object[]
                {
                    new[] { 1, 2, 3 },
                    "[\r\n    1,\r\n    2,\r\n    3\r\n]"
                },
                new object[]
                {
                    new List<string>
                    {
                        "Foo", null, "Bla"
                    },
                    "[\r\n    \"Foo\",\r\n    null,\r\n    \"Bla\"\r\n]"
                },
                new object[]
                {
                    new ObservableCollection<DummyPerson>
                    {
                        new DummyPerson { Name = "Walter White", Age = 52 },
                        new DummyPerson { Name = "Jesse Pinkman", Age = 27 }
                    },
                    "[\r\n    {\r\n        \"name\": \"Walter White\",\r\n        \"age\": 52\r\n    },\r\n    {\r\n        \"name\": \"Jesse Pinkman\",\r\n        \"age\": 27\r\n    }\r\n]"
                },
                new object[]
                {
                    new Dictionary<int, int> { { 1, 87 }, { 2, 88 }, { 3, 89 } },
                    "{\r\n    \"1\": 87,\r\n    \"2\": 88,\r\n    \"3\": 89\r\n}"
                },
                new object[]
                {
                    new Dictionary<string, DummyPerson>
                    {
                        { "Walter", new DummyPerson { Name = "Walter White", Age = 52 } },
                        { "Jesse", new DummyPerson { Name = "Jesse Pinkman", Age = 27 } }
                    },
                    "{\r\n    \"Walter\": {\r\n        \"name\": \"Walter White\",\r\n        \"age\": 52\r\n    },\r\n    \"Jesse\": {\r\n        \"name\": \"Jesse Pinkman\",\r\n        \"age\": 27\r\n    }\r\n}"
                }
            };

        [Theory(DisplayName = "Collection and dictionary serialization output can be switched to human readable json.")]
        [MemberData(nameof(Testdata))]
        public void CollectionsAndDictionariesAreSerializedToHumanReadableJsonCorrectly(IEnumerable enumerable, string expected)
        {
            DisableAllMetadata();

            CompareHumanReadableJsonToExpected(enumerable, expected);
        }

        [Fact(DisplayName = "Serialize must produce a JSON document with an empty object when the target has no public properties or fields.")]
        public void EmptyObject()
        {
            DisableAllMetadata();

            CompareHumanReadableJsonToExpected(new EmptyClass(), "{\r\n    \r\n}");
        }

        public class EmptyClass { }
    }
}