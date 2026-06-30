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
        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public const string CurrentSchema = "NMolecules.Bricks.DependencyGraph/1.0";

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickDependencyGraphDocument(DateTimeOffset generatedAt, IEnumerable<BrickDependencyGraphNode> nodes, IEnumerable<BrickDependencyGraphEdge> edges)
            : this(generatedAt, nodes, edges, CurrentSchema)
        {
        }

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
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

        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public string Schema { get; }
        /// <summary>
        /// Gets the timestamp associated with this Bricks model object.
        /// </summary>
        public DateTimeOffset GeneratedAt { get; }
        /// <summary>
        /// Gets the Nodes value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickDependencyGraphNode> Nodes { get; }
        /// <summary>
        /// Gets the Edges value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickDependencyGraphEdge> Edges { get; }
        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);

        /// <summary>
        /// Creates a Bricks configuration object from external key-value properties.
        /// </summary>
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
