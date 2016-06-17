using System;
using FluentAssertions;
using Light.Serialization.Abstractions;
using Xunit;

namespace Light.Serialization.Json.Tests.SerializationTests
{
    public sealed class EndlessRecursionTests : BaseJsonSerializerTest
    {
        [Fact(DisplayName = "The serializer must throw a SerializationException when the recursion level 300 is reached.")]
        public void EndlessRecursionWithDefaultLimit()
        {
            DisableObjectReferencePreservation();
            var recursiveDummy = new RecursiveDummy();

            Action act = () => GetSerializedJson(recursiveDummy);

            act.ShouldThrow<SerializationException>()
               .And.Message.Should().Contain($"The serializer probably is in an endless recursion - therefore the serialization process was stopped at recursion level {JsonSerializer.DefaultRecursionLevelLimit}.");
        }

        [Theory(DisplayName = "The serializer must be configurable so that recursion level limit can be changed by client code.")]
        [InlineData(3)]
        [InlineData(10)]
        public void ConfigurableRecursionLimits(int recursionLimit)
        {
            DisableObjectReferencePreservation();
            var serializer = GetSerializer();
            serializer.RecursionLevelLimit = recursionLimit;
            var recursiveDummy = new RecursiveDummy();

            Action act = () => serializer.Serialize(recursiveDummy);

            act.ShouldThrow<SerializationException>()
               .And.Message.Should().Contain($"The serializer probably is in an endless recursion - therefore the serialization process was stopped at recursion level {recursionLimit}.");
        }

        [Theory(DisplayName = "The serializer must throw an ArgumentOutOfRangeException when the specified recursion level is less than 3.")]
        [InlineData(2)]
        [InlineData(-1)]
        [InlineData(0)]
        public void InvalidLimit(int invalidRecursionLimit)
        {
            Action act = () => GetSerializer().RecursionLevelLimit = invalidRecursionLimit;

            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        public class RecursiveDummy
        {
            public RecursiveDummy Next;

            public RecursiveDummy()
            {
                Next = this;
            }
        }
    }
}