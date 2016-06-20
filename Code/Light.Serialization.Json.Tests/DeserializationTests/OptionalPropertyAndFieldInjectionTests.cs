using FluentAssertions;
using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class OptionalPropertyAndFieldInjectionTests : BaseJsonDeserializerTest
    {
        [Fact(DisplayName = "The deserializer must not perform property injection when the same value can be injected through the constructor.")]
        public void PropertyInjectionOptional()
        {
            const string json = "{ \"value\": \"Foo\" }";

            var deserializedInstance = GetDeserializedJson<PropertyInjectionStub>(json);

            deserializedInstance.NumberOfSetCalls.Should().Be(0);
        }

        public sealed class PropertyInjectionStub
        {
            private int _numberOfSetCalls;
            private string _value;

            public PropertyInjectionStub(string value)
            {
                _value = value;
            }

            public string Value
            {
                get { return _value; }
                set
                {
                    _numberOfSetCalls++;
                    _value = value;
                }
            }

            public int NumberOfSetCalls => _numberOfSetCalls;
        }
    }
}