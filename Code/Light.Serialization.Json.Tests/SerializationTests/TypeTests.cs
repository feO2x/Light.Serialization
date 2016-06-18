using System;
using System.Collections.Generic;
using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.SerializationTests
{
    public sealed class TypeTests : BaseJsonSerializerTest
    {
        [Theory(DisplayName = "The serializer must be able to serialize type objects with the .NET assembly qualified name when the default type to name mapping is used.")]
        [InlineData(typeof(int))]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(object))]
        [InlineData(typeof(Dictionary<,>))]
        public void AssemblyName(Type type)
        {
            DisableAllMetadata();

            var expectedJson = $"{{\"type\":\"{type.AssemblyQualifiedName}\"}}";
            CompareJsonToExpected(type, expectedJson);
        }

        [Theory(DisplayName = "The serializer must be able to serialize type objects with their Domain-Friendly Names.")]
        [InlineData(typeof(int), "{\"$type\":\"type\",\"type\":\"int32\"}")]
        [InlineData(typeof(object), "{\"$type\":\"type\",\"type\":\"object\"}")]
        [InlineData(typeof(string), "{\"$type\":\"type\",\"type\":\"string\"}")]
        [InlineData(typeof(IList<object>), "{\"$type\":\"type\",\"type\":{\"name\":\"abstractGenericList\",\"typeArguments\":[\"object\"]}}")]
        [InlineData(typeof(IDictionary<,>), "{\"$type\":\"type\",\"type\":\"abstractGenericMap\"}")]
        public void DomainFriendlyName(Type type, string expectedJson)
        {
            UseDomainFriendlyNames();
            DisableObjectReferencePreservation();

            CompareJsonToExpected(type, expectedJson);
        }
    }
}