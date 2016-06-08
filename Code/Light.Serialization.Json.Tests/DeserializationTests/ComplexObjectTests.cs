using FluentAssertions;
using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class ComplexObjectTests : BaseJsonDeserializerTest
    {
        [Fact(DisplayName = "The deserializer must be able to deserialize a JSON document containing a complex object.")]
        public void ComplexObject()
        {
            const string json = "{\"x\": 42, \"y\": \"Foo\"}";

            var result = GetDeserializedJson<DummyDto>(json);

            var expected = new DummyDto(42, "Foo");
            result.ShouldBeEquivalentTo(expected);
        }

        public class DummyDto
        {
            public DummyDto(int x, string y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }

            public string Y { get; }
        }
    }
}