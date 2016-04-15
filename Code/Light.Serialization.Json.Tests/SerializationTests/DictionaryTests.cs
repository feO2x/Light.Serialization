using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.Tests.SampleTypes;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.SerializationTests
{
    public sealed class DictionaryTests : BaseJsonSerializerTest
    {
        [Theory(DisplayName = "The serializer must be able to serialize dictionaries to complex JSON objects.")]
        [MemberData(nameof(DictionariesTestData))]
        public void Dictionaries(IDictionary dictionary, string expectedJson)
        {
            UseDomainFriendlyNames(options => options.UseTypes(typeof (Person)));

            CompareJsonToExpected(dictionary, expectedJson);
        }

        public static readonly TestData DictionariesTestData =
            new[]
            {
                new object[]
                {
                    new Dictionary<int, int> { { 1, 87 }, { 2, 88 }, { 3, 89 } },
                    "{\"$id\":0,\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"int32\",\"int32\"]},\"1\":87,\"2\":88,\"3\":89}"
                },
                new object[]
                {
                    new Dictionary<string, Person>
                    {
                        ["Walter"] = new Person("Walter", "White", 52),
                        ["Jesse"] = new Person("Jesse", "Pinkman", 27)
                    },
                    "{\"$id\":0,\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"string\",\"Person\"]},\"Walter\":{\"$id\":1,\"$type\":\"Person\",\"firstName\":\"Walter\",\"lastName\":\"White\",\"age\":52},\"Jesse\":{\"$id\":2,\"$type\":\"Person\",\"firstName\":\"Jesse\",\"lastName\":\"Pinkman\",\"age\":27}}"
                }
            };

        [Fact(DisplayName = "The serializer must omit Object IDs when the client disable Object Reference Preservation.")]
        public void WithoutObjectReferencePreservation()
        {
            var dictionary = new Dictionary<string, Person>
                             {
                                 ["Walter"] = new Person("Walter", "White", 52),
                                 ["Jesse"] = new Person("Jesse", "Pinkman", 27)
                             };
            UseDomainFriendlyNames(options => options.UseTypes(typeof (Person)));
            DisableObjectReferencePreservation();
            const string expectedJson = "{\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"string\",\"Person\"]},\"Walter\":{\"$type\":\"Person\",\"firstName\":\"Walter\",\"lastName\":\"White\",\"age\":52},\"Jesse\":{\"$type\":\"Person\",\"firstName\":\"Jesse\",\"lastName\":\"Pinkman\",\"age\":27}}";

            CompareJsonToExpected(dictionary, expectedJson);
        }

        [Fact(DisplayName = "The serializer must omit any metadata when the client disable Object Reference Preservation and Type Metadata.")]
        public void NoMetadata()
        {
            var dictionary = new Dictionary<string, Person>
                             {
                                 ["Walter"] = new Person("Walter", "White", 52),
                                 ["Jesse"] = new Person("Jesse", "Pinkman", 27)
                             };

            DisableObjectReferencePreservation();
            DisableTypeMetadata();
            const string expectedJson = "{\"Walter\":{\"firstName\":\"Walter\",\"lastName\":\"White\",\"age\":52},\"Jesse\":{\"firstName\":\"Jesse\",\"lastName\":\"Pinkman\",\"age\":27}}";

            CompareJsonToExpected(dictionary, expectedJson);
        }

        [Fact(DisplayName = "The serializer must throw an exception when a key type is used that cannot be serialized to a JSON string.")]
        public void NonPrimitiveKeyType()
        {
            var walter = new Person("Walter", "White", 52);
            var dictionary = new Dictionary<Person, int>
                             {
                                 [walter] = 5
                             };

            Action act = () => GetSerializedJson(dictionary);

            act.ShouldThrow<SerializationException>()
               .And.Message.Should().Contain($"The value \"{walter}\" can not be transformed to a JSON string and therefore cannot be used as a key in a complex JSON object.");
        }
    }
}