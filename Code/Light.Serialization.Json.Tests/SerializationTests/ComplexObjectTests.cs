using System.Collections.Generic;
using Light.Serialization.Json.Tests.SampleTypes;
using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.SerializationTests
{
    public sealed class ComplexObjectTests : BaseJsonSerializerTest
    {
        [Fact(DisplayName = "The serializer must be able to serialize object graphs even if they contain multiple references to the same object.")]
        public void GraphWithMultipleReferencesToSameObject()
        {
            var nodeC = new LeafNode("Baz");
            var nodeB = new NodeWithChilds("Bar", new Node[] { nodeC });
            var nodeA = new NodeWithChilds("Foo", new List<Node> { nodeB, nodeC });
            UseDomainFriendlyNames(options => options.UseTypes(typeof(Node),
                                                               typeof(NodeWithChilds),
                                                               typeof(LeafNode)));

            const string expectedJson = "{\"$id\":0,\"$type\":\"NodeWithChilds\",\"id\":\"Foo\",\"childNodes\":[\"$id\",1,\"$type\",{\"name\":\"genericList\",\"typeArguments\":[\"Node\"]},{\"$id\":2,\"$type\":\"NodeWithChilds\",\"id\":\"Bar\",\"childNodes\":[\"$id\",3,\"$type\",{\"name\":\"array\",\"arrayType\":\"Node\",\"arrayLength\":1},{\"$id\":4,\"$type\":\"LeafNode\",\"id\":\"Baz\"}]},{\"$ref\":4}]}";

            CompareJsonToExpected(nodeA, expectedJson);
        }

        [Fact(DisplayName = "The client can create serialization rules that describe members that the serializer must not include in the resulting JSON document.")]
        public void BlacklistingMembers()
        {
            var person = new Person("Walter", "White", 52);
            UseDomainFriendlyNames(options => options.UseTypes(typeof(Person)));
            AddRule<Person>(r => r.IgnoreField(p => p.Age));

            const string expectedJson = "{\"$id\":0,\"$type\":\"Person\",\"firstName\":\"Walter\",\"lastName\":\"White\"}";

            CompareJsonToExpected(person, expectedJson);
        }

        [Fact(DisplayName = "The client can create serialization rules that describe the only members that the serializer must include in the resulting JSON document.")]
        public void WhitelistingMembers()
        {
            UseDomainFriendlyNames(options => options.UseTypes(typeof(Person)));
            var person = new Person("Jesse", "Pinkman", 27);
            AddRule<Person>(r => r.IgnoreAll()
                                  .ButProperty(p => p.FirstName)
                                  .AndField(p => p.Age));

            const string expectedJson = "{\"$id\":0,\"$type\":\"Person\",\"firstName\":\"Jesse\",\"age\":27}";

            CompareJsonToExpected(person, expectedJson);
        }

        [Fact(DisplayName = "The serializer must not include Object IDs in the resulting JSON document if the client disabled the Object Reference Preservation option.")]
        public void NoObjectReferencePreservation()
        {
            var nodeC = new LeafNode("Baz");
            var nodeB = new NodeWithChilds("Bar", new Node[] { nodeC });
            var nodeA = new NodeWithChilds("Foo", new List<Node> { nodeB, nodeC });
            UseDomainFriendlyNames(options => options.UseTypes(typeof(Node),
                                                               typeof(NodeWithChilds),
                                                               typeof(LeafNode)));
            DisableObjectReferencePreservation();

            const string expectedJson = "{\"$type\":\"NodeWithChilds\",\"id\":\"Foo\",\"childNodes\":[\"$type\",{\"name\":\"genericList\",\"typeArguments\":[\"Node\"]},{\"$type\":\"NodeWithChilds\",\"id\":\"Bar\",\"childNodes\":[\"$type\",{\"name\":\"array\",\"arrayType\":\"Node\",\"arrayLength\":1},{\"$type\":\"LeafNode\",\"id\":\"Baz\"}]},{\"$type\":\"LeafNode\",\"id\":\"Baz\"}]}";

            CompareJsonToExpected(nodeA, expectedJson);
        }

        [Fact(DisplayName = "The serializer must not include any metadata in complex JSON objects or JSON arrays when all metadata options are turned off by the client.")]
        public void NoMetadataSection()
        {
            var nodeC = new LeafNode("Baz");
            var nodeB = new NodeWithChilds("Bar", new Node[] { nodeC });
            var nodeA = new NodeWithChilds("Foo", new List<Node> { nodeB, nodeC });

            DisableObjectReferencePreservation();
            DisableTypeMetadata();

            const string expectedJson = "{\"id\":\"Foo\",\"childNodes\":[{\"id\":\"Bar\",\"childNodes\":[{\"id\":\"Baz\"}]},{\"id\":\"Baz\"}]}";

            CompareJsonToExpected(nodeA, expectedJson);
        }
    }
}