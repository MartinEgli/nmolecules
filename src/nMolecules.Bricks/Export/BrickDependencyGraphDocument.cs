using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Defines the document shape used to exchange dependency graph document data between Bricks tools.
/// </summary>
public sealed class BrickDependencyGraphDocument
    {
        public const string CurrentSchema = "NMolecules.Bricks.DependencyGraph/1.0";

        public BrickDependencyGraphDocument(DateTimeOffset generatedAt, IEnumerable<BrickDependencyGraphNode> nodes, IEnumerable<BrickDependencyGraphEdge> edges)
            : this(generatedAt, nodes, edges, CurrentSchema)
        {
        }

        public BrickDependencyGraphDocument(DateTimeOffset generatedAt, IEnumerable<BrickDependencyGraphNode> nodes, IEnumerable<BrickDependencyGraphEdge> edges, string schema)
        {
            GeneratedAt = generatedAt;
            Nodes = (nodes ?? Enumerable.Empty<BrickDependencyGraphNode>())
                .OrderBy(node => node.Element.Id.Value, StringComparer.Ordinal)
                .ToArray();
            Edges = (edges ?? Enumerable.Empty<BrickDependencyGraphEdge>())
                .OrderBy(edge => edge.Source.Id.Value, StringComparer.Ordinal)
                .ThenBy(edge => edge.Target.Id.Value, StringComparer.Ordinal)
                .ThenBy(edge => edge.KindId.Value, StringComparer.Ordinal)
                .ToArray();
            Schema = schema ?? string.Empty;
        }

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickDependencyGraphNode> Nodes { get; }
        public IReadOnlyList<BrickDependencyGraphEdge> Edges { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);

        public static BrickDependencyGraphDocument FromDependencies(DateTimeOffset generatedAt, IEnumerable<BrickDependency> dependencies, string schema = CurrentSchema)
        {
            var dependencyList = (dependencies ?? Enumerable.Empty<BrickDependency>()).ToArray();
            var nodes = dependencyList
                .SelectMany(dependency => new[] { dependency.Source, dependency.Target })
                .GroupBy(element => element.Id)
                .Select(group => new BrickDependencyGraphNode(group.First()));
            var edges = dependencyList.Select(BrickDependencyGraphEdge.FromDependency);

            return new BrickDependencyGraphDocument(generatedAt, nodes, edges, schema);
        }
    }
}
