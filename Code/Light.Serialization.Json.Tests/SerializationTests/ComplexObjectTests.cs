﻿using System.Collections.Generic;
using Light.Serialization.Json.Tests.SampleTypes;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

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
            UseDomainFriendlyNames(options => options.UseTypes(typeof (Node),
                                                               typeof (NodeWithChilds),
                                                               typeof (LeafNode)));

            const string expectedJson = "{\"$id\":0,\"$type\":\"NodeWithChilds\",\"id\":\"Foo\",\"childNodes\":[\"$id:1\",{\"$id\":2,\"$type\":\"NodeWithChilds\",\"id\":\"Bar\",\"childNodes\":[\"$id:3\",{\"$id\":4,\"$type\":\"LeafNode\",\"id\":\"Baz\"}]},{\"$ref\":4}]}";

            CompareJsonToExpected(nodeA, expectedJson);
        }

        [Fact(DisplayName = "The client can create serialization rules that describe members that the serializer must not include in the resulting JSON document.")]
        public void BlacklistingMembers()
        {
            var person = new Person("Walter", "White", 52);
            AddRule<Person>(r => r.IgnoreField(p => p.Age));

            const string expectedJson = "{\"$id\":0,\"$type\":\"Light.Serialization.Json.Tests.SampleTypes.Person, Light.Serialization.Json.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\",\"firstName\":\"Walter\",\"lastName\":\"White\"}";

            CompareJsonToExpected(person, expectedJson);
        }

        [Fact(DisplayName = "The client can create serialization rules that describe the only members that the serializer must include in the resulting JSON document.")]
        public void WhitelistingMembers()
        {
            var person = new Person("Jesse", "Pinkman", 27);
            AddRule<Person>(r => r.IgnoreAll()
                                  .ButProperty(p => p.FirstName)
                                  .AndField(p => p.Age));
            UseDomainFriendlyNames(options => options.UseTypes(typeof (Person)));

            const string expectedJson = "{\"$id\":0,\"$type\":\"Person\",\"firstName\":\"Jesse\",\"age\":27}";

            CompareJsonToExpected(person, expectedJson);
        }

        [Fact(DisplayName = "The serializer must not include Object IDs in the resulting JSON document if the client disabled the Object Reference Preservation option.")]
        public void NoObjectReferencePreservation()
        {
            var nodeC = new LeafNode("Baz");
            var nodeB = new NodeWithChilds("Bar", new Node[] { nodeC });
            var nodeA = new NodeWithChilds("Foo", new List<Node> { nodeB, nodeC });
            UseDomainFriendlyNames(options => options.UseTypes(typeof (Node),
                                                               typeof (NodeWithChilds),
                                                               typeof (LeafNode)));
            DisableObjectReferencePreservation();

            const string expectedJson = "{\"$type\":\"NodeWithChilds\",\"id\":\"Foo\",\"childNodes\":[{\"$type\":\"NodeWithChilds\",\"id\":\"Bar\",\"childNodes\":[{\"$type\":\"LeafNode\",\"id\":\"Baz\"}]},{\"$type\":\"LeafNode\",\"id\":\"Baz\"}]}";

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