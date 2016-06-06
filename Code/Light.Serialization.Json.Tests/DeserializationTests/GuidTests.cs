using System;
using FluentAssertions;
using Light.Serialization.Json.FrameworkExtensions;
using Xunit;

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class GuidTests : BaseJsonDeserializerTest
    {
        [Fact(DisplayName = "The deserializer must be able to deserialize JSON strings containing a GUID to .NET Guid instances.")]
        public void DeserializeGuid()
        {
            var guid = Guid.NewGuid();
            var json = guid.ToString().SurroundWithQuotationMarks();

            CompareDeserializedJsonToExpected(json, guid);
        }

        [Theory(DisplayName = "The deserializer must be able to deserialize JSON string to a GUID when it is references as an object or ValueType.")]
        [InlineData(typeof(object))]
        [InlineData(typeof(ValueType))]
        public void DeserializeGuidUsingBaseClass(Type requestedType)
        {
            var guid = Guid.NewGuid();
            var json = guid.ToString().SurroundWithQuotationMarks();

            var deserializedValue = GetDeserializedJson(json, requestedType);

            deserializedValue.As<Guid>().Should().Be(guid);
        }

        [Fact(DisplayName = "The deserializer must throw a JsonDocumentException when the JSON string representing a GUID is malformed.")]
        public void MalformedJson()
        {
            const string malformedJson = "\"X9168C5E-KEB2-4faa-B6BF-329BF39FA1E4\"";

            CheckDeserializerThrowsExceptionWithMessageContaining<Guid>(malformedJson, $"Could not deserialize token {malformedJson} to a valid GUID.");
        }
    }
}