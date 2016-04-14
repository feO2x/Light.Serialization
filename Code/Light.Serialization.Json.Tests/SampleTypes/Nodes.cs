using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.Serialization.Json.Tests.SampleTypes
{
    public abstract class Node
    {
        protected Node(string id)
        {
            id.MustNotBeNullOrWhiteSpace(nameof(id));

            Id = id;
        }

        public string Id { get; }
    }

    public sealed class NodeWithChilds : Node
    {
        public readonly IList<Node> ChildNodes;

        public NodeWithChilds(string id, IList<Node> childNodes) : base(id)
        {
            childNodes.MustNotBeNullOrEmpty(nameof(childNodes));

            ChildNodes = childNodes;
        }
    }

    public sealed class LeafNode : Node
    {
        public LeafNode(string id) : base(id) { }
    }
}