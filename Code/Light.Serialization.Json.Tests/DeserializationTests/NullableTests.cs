using System;
using Light.Serialization.Json.FrameworkExtensions;
using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class NullableTests : BaseJsonDeserializerTest
    {
        [Fact(DisplayName = "The deserializer must able to deserialize nullable values that are null.")]
        public void Null()
        {
            const string json = "null";
            int? nullable = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            CompareDeserializedJsonToExpected(json, nullable);
        }

        [Fact(DisplayName = "The deserializer must be able to deserialize nullable values that are not null.")]
        public void NotNull()
        {
            var guid = Guid.NewGuid();
            var json = guid.ToString().SurroundWithQuotationMarks();

            Guid? expected = guid;
            CompareDeserializedJsonToExpected(json, expected);
        }
    }
}