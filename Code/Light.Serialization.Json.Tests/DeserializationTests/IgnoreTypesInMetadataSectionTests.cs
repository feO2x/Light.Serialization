using System.Collections.Generic;
using FluentAssertions;
using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class IgnoreTypesInMetadataSectionTests : BaseJsonDeserializerTest
    {
        [Fact(DisplayName = "The deserializer must be able to ignore the $type metadata section and create the requested type instead.")]
        public void TypesIgnored()
        {
            ConfigureDefaultDomainFriendlyNames();
            IgnoreTypesInMetadataSection();
            const string json = "[\"$type\", { \"name\": \"observableGenericList\", \"typeArguments\": [\"int32\"] }, 1, 2, 3, 4]";

            var deserializedCollection = GetDeserializedJson<List<int>>(json);

            var expected = new List<int> { 1, 2, 3, 4 };
            deserializedCollection.GetType().Should().Be(expected.GetType());
            deserializedCollection.Should().BeEquivalentTo(expected);
        }
    }
}