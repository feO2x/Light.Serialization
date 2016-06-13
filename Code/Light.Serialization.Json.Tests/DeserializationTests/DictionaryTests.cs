using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class DictionaryTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to deserialize dictionaries containing strings.")]
        [MemberData(nameof(DeserializeStringDictionariesData))]
        public void DeserializeStringDictionaries(string json, Dictionary<string, string> expected)
        {
            var actual = GetDeserializedJson<Dictionary<string, string>>(json);

            actual.ShouldAllBeEquivalentTo(expected);
        }

        public static readonly TestData DeserializeStringDictionariesData =
            new[]
            {
                new object[] { "{\"Hello\":\"World\"}", new Dictionary<string, string> { ["Hello"] = "World" } },
                new object[] { "{\"1\":\"Hey\",\"2\":\"Ho!\",\"3\":\"What?\"}", new Dictionary<string, string> { ["1"] = "Hey", ["2"] = "Ho!", ["3"] = "What?" } }
            };

        [Theory(DisplayName = "The deserializer must be able to deserialize dictiories when the requested type is a dictionary abstraction.")]
        [MemberData(nameof(UseAbstractionsData))]
        public void UseAbstractions(string json, IDictionary<string, string> expected)
        {
            ConfigureDefaultDomainFriendlyNames();
            var actual = GetDeserializedJson<IDictionary<string, string>>(json);

            actual.ShouldAllBeEquivalentTo(expected);
        }

        public static readonly TestData UseAbstractionsData =
            new[]
            {
                new object[]
                {
                    "{\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"string\",\"string\"]}, \"1\": \"Hello\", \"2\": \"World\"}",
                    new Dictionary<string, string> { ["1"] = "Hello", ["2"] = "World" }
                },
                new object[]
                {
                    "{\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"string\",\"string\"]}, \"This\": \"That\", \"Here\": \"There\"}",
                    new Dictionary<string, string> { ["This"] = "That", ["Here"] = "There" }
                }
            };

        [Theory(DisplayName = "The deserializer must be able to deserialize dictionaries when the key type is an enumeration.")]
        [MemberData(nameof(EnumKeysData))]
        public void EnumKeys<T>(string json, IDictionary<T, string> expected, T sampleValueForTypeResolving)
        {
            var enumType = sampleValueForTypeResolving.GetType();
            ConfigureDefaultDomainFriendlyNames(options => options.UseTypes(enumType));

            var actual = GetDeserializedJson<IDictionary<T, string>>(json);

            actual.ShouldAllBeEquivalentTo(expected);
        }

        public static readonly TestData EnumKeysData =
            new[]
            {
                new object[]
                {
                    "{\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"ConsoleColor\",\"string\"]}, \"Black\": \"black\", \"Cyan\": \"cyan\", \"DarkMagenta\": \"darkMagenta\"}",
                    new Dictionary<ConsoleColor, string> { [ConsoleColor.Black] = "black", [ConsoleColor.Cyan] = "cyan", [ConsoleColor.DarkMagenta] = "darkMagenta" }, ConsoleColor.Black
                },
                new object[]
                {
                    "{\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"CollectionBehavior\",\"string\"]}, \"CollectionPerAssembly\": \"run tests per assembly\", \"CollectionPerClass\": \"run tests per class\"}",
                    new Dictionary<CollectionBehavior, string> { [CollectionBehavior.CollectionPerAssembly] = "run tests per assembly", [CollectionBehavior.CollectionPerClass] = "run tests per class" }, CollectionBehavior.CollectionPerAssembly
                }
            };

        [Theory(DisplayName = "The deserializer must be able to deserialize dictionaries when the key is a numeric type.")]
        [MemberData(nameof(NumericKeysData))]
        public void NumericKeys<T>(string json, IDictionary<T, object> expected, T sampleValueForTypeResolving)
        {
            ConfigureDefaultDomainFriendlyNames();

            var actual = GetDeserializedJson<IDictionary<T, object>>(json);

            actual.ShouldAllBeEquivalentTo(expected);
        }

        public static readonly TestData NumericKeysData =
            new[]
            {
                new object[]
                {
                    "{\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"int32\",\"object\"]}, \"1\": \"Hello\", \"2\": \"World\"}",
                    new Dictionary<int, object> { [1] = "Hello", [2] = "World" }, new int()
                },
                new object[]
                {
                    "{\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"int16\",\"object\"]}, \"-14\": \"x\", \"0\": 19 }",
                    new Dictionary<short, object> { [-14] = "x", [0] = 19 }, new short()
                },
                new object[]
                {
                    "{\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"int64\",\"object\"]}, \"1\": \"Black\", \"2\": \"DarkBlue\", \"3\": \"Magenta\"}",
                    new Dictionary<long, object> { [1L] = "Black", [2L] = "DarkBlue", [3L] = "Magenta" }, new long()
                },
                new object[]
                {
                    "{\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"int8\",\"object\"]}, \"-128\": \"a\", \"0\": \"b\", \"127\": \"c\"}",
                    new Dictionary<sbyte, object> { [-128] = "a", [0] = "b", [127] = "c" }, new sbyte()
                },
                new object[]
                {
                    "{\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"uint32\",\"object\"]}, \"1\": \"Hello\", \"2\": \"There\"}",
                    new Dictionary<uint, object> { [1u] = "Hello", [2u] = "There" }, new uint()
                },
                new object[]
                {
                    "{\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"uint16\",\"object\"]}, \"30000\": \"number\", \"65535\": \"large number\"}",
                    new Dictionary<ushort, object> { [30000] = "number", [65535] = "large number" }, new ushort()
                },
                new object[]
                {
                    $"{{\"$type\":{{\"name\":\"genericMap\",\"typeArguments\":[\"uint64\",\"object\"]}}, \"{ulong.MinValue}\": \"long minimum\", \"{ulong.MaxValue}\": \"long maximum\"}}",
                    new Dictionary<ulong, object> { [ulong.MinValue] = "long minimum", [ulong.MaxValue] = "long maximum" }, new ulong()
                },
                new object[]
                {
                    "{\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"byte\",\"object\"]}, \"1\": \"small byte\", \"2\": \"slightly larger byte\", \"255\": \"largest byte\"}",
                    new Dictionary<byte, object> { [1] = "small byte", [2] = "slightly larger byte", [255] = "largest byte" }, new byte()
                },
                new object[]
                {
                    "{\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"float64\",\"object\"]}, \"42.7\": \"nearly the answer\", \"42.0\": \"that's it\"}",
                    new Dictionary<double, object> { [42.7] = "nearly the answer", [42.0] = "that's it" }, new double()
                },
                new object[]
                {
                    "{\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"float32\",\"object\"]}, \"-42.7\": \"nearly the answer\", \"42.0\": \"that's it\"}",
                    new Dictionary<float, object> { [-42.7f] = "nearly the answer", [42.0f] = "that's it" }, new float()
                },
                new object[]
                {
                    "{\"$type\":{\"name\":\"genericMap\",\"typeArguments\":[\"decimal\",\"object\"]}, \"-4200.705\": \"nearly the answer\", \"42.0\": \"that's it\"}",
                    new Dictionary<decimal, object> { [-4200.705m] = "nearly the answer", [42.0m] = "that's it" }, new decimal()
                }
            };

        [Fact(DisplayName = "The deserializer must be able to deserialize a dictionary that is referenced via type object.")]
        public void DeserializeDictionaryThatIsReferencedViaObject()
        {
            ConfigureDefaultDomainFriendlyNames();

            var expected = new Dictionary<string, string> { ["Foo"] = "Bar", ["Baz"] = "Qux" };
            const string json = "{ \"$type\": { \"name\": \"genericMap\", \"typeArguments\": [ \"string\", \"string\" ] }, \"Foo\": \"Bar\", \"Baz\": \"Qux\" }";

            var actual = (Dictionary<string, string>) GetDeserializedJson<object>(json);
            
            actual.ShouldAllBeEquivalentTo(expected);
        }

        [Fact(DisplayName = "The deserializer must be able to parse primitive values that are stored in a dictionary with key type object.")]
        public void DictioanryWithObjectAsKeyType()
        {
            var expected = new Dictionary<object, object>
                           {
                               [1] = "One",
                               [3.3] = "Three",
                               [false] = "Foo",
                               ['h'] = "Bar",
                               [new DateTime(2016, 6, 13)] = 14,
                               [new Guid("22CBA95F-F4AA-49D2-B48C-214ACFFE991F")] = 42.1
                           };

            const string json = "{ \"1\": \"One\", \"3.3\": \"Three\", \"false\": \"Foo\", \"h\": \"Bar\", \"2016-6-13\": 14, \"22CBA95F-F4AA-49D2-B48C-214ACFFE991F\": 42.1 }";

            var actual = GetDeserializedJson<IDictionary<object, object>>(json);

            var actualEnumerator = actual.GetEnumerator();
            var expectedEnumerator = expected.GetEnumerator();
            for (var i = 0; i < actual.Count; i++)
            {
                actualEnumerator.MoveNext();
                expectedEnumerator.MoveNext();

                actualEnumerator.Current.Key.Should().Be(expectedEnumerator.Current.Key);
                actualEnumerator.Current.Value.Should().Be(expectedEnumerator.Current.Value);
            }
        }
    }
}