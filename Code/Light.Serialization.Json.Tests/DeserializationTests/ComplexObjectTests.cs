using FluentAssertions;
using Xunit;

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class ComplexObjectTests : BaseJsonDeserializerTest
    {
        [Fact(DisplayName = "The deserializer must be able to deserialize a JSON document containing a complex object.")]
        public void ComplexObject()
        {
            const string json = "{\"x\": 42, \"y\": \"Foo\"}";

            var result = GetDeserializedJson<DummyClass>(json);

            var expected = new DummyClass(42, "Foo");
            result.ShouldBeEquivalentTo(expected);
        }

        public class DummyClass
        {
            public DummyClass(int x, string y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }

            public string Y { get; }
        }
    }
}