using System;
using FluentAssertions;
using Light.Serialization.Abstractions;
using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.SerializationTests
{
    public sealed class DelegateTests : BaseJsonSerializerTest
    {
        [Fact(DisplayName = "An exception is thrown when a delegate should be serialized as delegates are currently not supported.")]
        public void ExceptionMustBeThrownWhenDelegateIsSerialized()
        {
            var numberOfCalls = 0;
            var action = new Action(() => numberOfCalls++);

            Action act = () => GetSerializedJson(action);

            act.ShouldThrow<SerializationException>()
               .And.Message.Should().Contain($"Type {action.GetType()} cannot be serialized because there is no IJsonWriterInstructor registered that can cover this type.");
        }
    }
}