using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Light.Serialization.Json.LowLevelReading;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class TypeAndTypeInfoTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse Type instances with the default name-to-type-mapping.")]
        [InlineData(typeof(int))]
        [InlineData(typeof(string))]
        [InlineData(typeof(List<object>))]
        [InlineData(typeof(Dictionary<,>))]
        public void TypeAssemblyName(Type type)
        {
            var json = $"{{\"type\":\"{type.AssemblyQualifiedName}\"}}";
            CompareDeserializedJsonToExpected(json, type);
        }

        [Theory(DisplayName = "The deserializer must throw a JsonDocumentException when the \"type\" key in the complex JSON object is misspelled.")]
        [InlineData("tpe")]
        [InlineData("typ")]
        [InlineData("tp")]
        public void TypeKeyMisspelled(string misspelledKey)
        {
            var invalidJson = $"{{\"{misspelledKey}\": \"{typeof(int).AssemblyQualifiedName}\"}}";

            Action act = () => GetDeserializer().Deserialize<Type>(invalidJson);

            act.ShouldThrow<JsonDocumentException>()
               .And.Message.Should().Contain($"Expected key \"type\" in complex JSON object describing a Type or TypeInfo instance, but found \"{misspelledKey}\".");
        }

        [Theory(DisplayName = "The deserializer must be able to parse Type instances from their Domain-Friendly Names.")]
        [InlineData("{\"type\": \"int32\"}", typeof(int))]
        [InlineData("{\"type\": \"uint16\"}", typeof(ushort))]
        [InlineData("{\"type\": \"int8\"}", typeof(sbyte))]
        [InlineData("{\"type\": \"genericMap\"}", typeof(Dictionary<,>))]
        [InlineData("{\"type\": { \"name\": \"genericList\", \"typeArguments\": [\"string\"] }}", typeof(List<string>))]
        [InlineData("{\"type\": { \"name\": \"array\", \"arrayType\": \"int32\" }}", typeof(int[]))]
        [InlineData("{\"type\": { \"name\": \"array\", \"arrayType\": \"character\", \"arrayRank\": 2 }}", typeof(char[,]))]
        public void TypeDomainFriendlyNames(string json, Type type)
        {
            UseDomainFriendlyNames();
            CompareDeserializedJsonToExpected(json, type);
        }

        [Theory(DisplayName = "The deserializer must be able to parse TypeInfo instances from their Assembly-Qualified Name.")]
        [MemberData(nameof(TypeInfoAssemblyNamesData))]
        public void TypeInfoAssemblyNames(TypeInfo typeInfo)
        {
            var json = $"{{\"type\": \"{typeInfo.AssemblyQualifiedName}\"}}";
            CompareDeserializedJsonToExpected(json, typeInfo);
        }

        public static readonly TestData TypeInfoAssemblyNamesData =
            new[]
            {
                new object[] { typeof(int).GetTypeInfo() },
                new object[] { typeof(string).GetTypeInfo() },
                new object[] { typeof(IDictionary<,>).GetTypeInfo() },
                new object[] { typeof(List<object>).GetTypeInfo() }
            };

        [Theory(DisplayName = "The deserializer must be able to parse TypeInfo instances from their Domain-Friendly Names.")]
        [MemberData(nameof(TypeInfoDomainFriendlyNamesData))]
        public void TypeInfoDomainFriendlyNames(string json, TypeInfo typeInfo)
        {
            UseDomainFriendlyNames();

            CompareDeserializedJsonToExpected(json, typeInfo);
        }

        public static readonly TestData TypeInfoDomainFriendlyNamesData =
            new[]
            {
                new object[] { "{\"type\": \"uint32\"}", typeof(uint).GetTypeInfo() },
                new object[] { "{\"type\": \"string\"}", typeof(string).GetTypeInfo() },
                new object[] { "{\"type\": \"abstractGenericSet\"}", typeof(ISet<>).GetTypeInfo() },
                new object[] { "{\"type\": { \"name\": \"genericList\", \"typeArguments\": [\"int32\"] }}", typeof(List<int>).GetTypeInfo() }
            };

        [Fact(DisplayName = "The deserializer must be able deserialize type instances when the requested type is object.")]
        public void TypeReferencedAsObject()
        {
            UseDomainFriendlyNames();
            const string json = "{ \"$type\": \"type\", \"type\": \"int32\"}";
            var expected = typeof(int);

            CompareDeserializedJsonToExpected<object>(json, expected);
        }
    }
}