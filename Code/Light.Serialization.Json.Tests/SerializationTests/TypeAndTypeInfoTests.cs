using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.SerializationTests
{
    public sealed class TypeAndTypeInfoTests : BaseJsonSerializerTest
    {
        [Theory(DisplayName = "The serializer must be able to serialize Type objects with the .NET assembly qualified name when the default type-to-name-mapping is used.")]
        [InlineData(typeof(int))]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(object))]
        [InlineData(typeof(Dictionary<,>))]
        public void TypeAssemblyName(Type type)
        {
            DisableAllMetadata();

            var expectedJson = $"{{\"type\":\"{type.AssemblyQualifiedName}\"}}";
            CompareJsonToExpected(type, expectedJson);
        }

        [Theory(DisplayName = "The serializer must be able to serialize Type objects with their Domain-Friendly Names.")]
        [InlineData(typeof(int), "{\"$type\":\"type\",\"type\":\"int32\"}")]
        [InlineData(typeof(object), "{\"$type\":\"type\",\"type\":\"object\"}")]
        [InlineData(typeof(string), "{\"$type\":\"type\",\"type\":\"string\"}")]
        [InlineData(typeof(IList<object>), "{\"$type\":\"type\",\"type\":{\"name\":\"abstractGenericList\",\"typeArguments\":[\"object\"]}}")]
        [InlineData(typeof(IDictionary<,>), "{\"$type\":\"type\",\"type\":\"abstractGenericMap\"}")]
        public void TypeDomainFriendlyName(Type type, string expectedJson)
        {
            UseDomainFriendlyNames();
            DisableObjectReferencePreservation();

            CompareJsonToExpected(type, expectedJson);
        }

        [Theory(DisplayName = "The serializer must be able to serialize TypeInfo objects with the .NET assembly qualified name when the default type-to-name-mapping is used.")]
        [MemberData(nameof(TypeInfoAssemblyNameData))]
        public void TypeInfoAssemblyName(TypeInfo typeInfo)
        {
            DisableObjectReferencePreservation();

            var expectedJson = $"{{\"$type\":\"{typeof(Type).AssemblyQualifiedName}\",\"type\":\"{typeInfo.AssemblyQualifiedName}\"}}";
            CompareJsonToExpected(typeInfo, expectedJson);
        }

        public static readonly TestData TypeInfoAssemblyNameData =
            new[]
            {
                new object[] { typeof(short).GetTypeInfo() },
                new object[] { typeof(string).GetTypeInfo() },
                new object[] { typeof(IReadOnlyList<DateTime>).GetTypeInfo() },
                new object[] { typeof(ObservableCollection<>).GetTypeInfo() }
            };

        [Theory(DisplayName = "The serializer must be able to serialize TypeInfo objects with their Domain-Friendly Names.")]
        [MemberData(nameof(TypeInfoDomainFriendlyNameData))]
        public void TypeInfoDomainFriendlyName(TypeInfo typeInfo, string expectedJson)
        {
            UseDomainFriendlyNames();
            DisableObjectReferencePreservation();

            CompareJsonToExpected(typeInfo, expectedJson);
        }

        public static readonly TestData TypeInfoDomainFriendlyNameData =
            new[]
            {
                new object[] { typeof(uint).GetTypeInfo(), "{\"$type\":\"type\",\"type\":\"uint32\"}" },
                new object[] { typeof(long).GetTypeInfo(), "{\"$type\":\"type\",\"type\":\"int64\"}" },
                new object[] { typeof(IEnumerable<string>).GetTypeInfo(), "{\"$type\":\"type\",\"type\":{\"name\":\"abstractGenericEnumerable\",\"typeArguments\":[\"string\"]}}" },
                new object[] { typeof(ISet<>).GetTypeInfo(), "{\"$type\":\"type\",\"type\":\"abstractGenericSet\"}" }
            };
    }
}