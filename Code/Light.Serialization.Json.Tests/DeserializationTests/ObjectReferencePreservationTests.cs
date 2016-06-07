using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class ObjectReferencePreservationTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to inject the same collection into different objects of the JSON document.")]
        [InlineData("{ \"$id\": 0, \"items\": [\"$id\", 1, 1, 2, 3], \"child\": { \"$id\": 2, \"child\": null, \"items\": [\"$ref\", 1] } }")]
        [InlineData("{ \"$id\": 0, \"items\": [\"$ref\", 2], \"child\": { \"$id\": 1, \"items\": [\"$id\", 2, 5, 6], \"child\": null} }")] // This should result in a deferred reference
        public void CollectionPreservation(string json)
        {
            var deserializedObjectGraph = GetDeserializedJson<Node>(json);

            deserializedObjectGraph.Items.Should().BeSameAs(deserializedObjectGraph.Child.Items);
        }

        public class Node
        {
            public Node(List<int> items, Node child)
            {
                Items = items;
                Child = child;
            }

            public Node Child { get; }

            public List<int> Items { get; set; }
        }
    }
}