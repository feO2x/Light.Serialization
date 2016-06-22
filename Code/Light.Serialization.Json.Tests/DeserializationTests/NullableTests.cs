using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class NullableTests : BaseJsonDeserializerTest
    {
        [Fact(DisplayName = "The deserializer must able to deserialize nullable values.")]
        public void DeserializeNullable()
        {
            const string json = "null";
            int? nullable = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            CompareDeserializedJsonToExpected(json, nullable);
        }
    }
}