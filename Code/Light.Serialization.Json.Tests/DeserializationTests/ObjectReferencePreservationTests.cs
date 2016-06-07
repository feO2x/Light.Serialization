using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class ObjectReferencePreservationTests : BaseJsonDeserializerTest
    {
        [Fact(DisplayName = "The deserializer must be able to inject the same collection into different objects of the JSON document.")]
        public void CollectionPreservation()
        {
            const string json = "{ \"$id\": 0, \"items\": [\"$id\", 1, 1, 2, 3], \"child\": { \"$id\": 2, \"child\": null, \"items\": [\"$ref\", 1] } }";

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

            public List<int> Items { get; }
        }
    }
}