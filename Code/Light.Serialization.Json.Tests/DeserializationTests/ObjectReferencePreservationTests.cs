using System.Collections.Generic;
using FluentAssertions;
using Light.GuardClauses;
using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class ObjectReferencePreservationTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to inject the same collection into different objects of the JSON document.")]
        [InlineData("{ \"$id\": 0, \"items\": [\"$id\", 1, 1, 2, 3], \"child\": { \"$id\": 2, \"child\": null, \"items\": [\"$ref\", 1] } }")]
        [InlineData("{ \"$id\": 0, \"items\": [\"$ref\", 2], \"child\": { \"$id\": 1, \"items\": [\"$id\", 2, 5, 6], \"child\": null} }")]
        public void CollectionPreservation(string json)
        {
            var deserializedObjectGraph = GetDeserializedJson<Node>(json);

            deserializedObjectGraph.Items.Should().BeSameAs(deserializedObjectGraph.Child.Items);
        }

        public class Node
        {
            public Node Child { get; set; }

            public List<int> Items { get; set; }
        }

        [Fact(DisplayName = "The deserializer must be able to resolve deferred object references.")]
        public void ObjectPreservation()
        {
            const string json = "{ \"$id\": 1, \"otherB\": { \"$id\": 2, \"otherA\": { \"$ref\": 1 } } }";

            var a = GetDeserializedJson<A>(json);

            a.OtherB.OtherA.Should().BeSameAs(a);
        }

        public class A
        {
            public readonly B OtherB;

            public A(B otherB)
            {
                OtherB = otherB;
            }
        }

        public class B
        {
            private A _otherA;

            public A OtherA
            {
                get { return _otherA; }
                set
                {
                    value.MustNotBeNull();
                    _otherA = value;
                }
            }
        }
    }
}