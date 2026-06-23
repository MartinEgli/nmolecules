using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    public sealed class BrickRoleMapDocument
    {
        public const string CurrentSchema = "NMolecules.Bricks.RoleMap/1.0";

        public BrickRoleMapDocument(DateTimeOffset generatedAt, IEnumerable<BrickRoleMapEntry> entries)
            : this(generatedAt, entries, CurrentSchema)
        {
        }

        public BrickRoleMapDocument(DateTimeOffset generatedAt, IEnumerable<BrickRoleMapEntry> entries, string schema)
        {
            GeneratedAt = generatedAt;
            Entries = (entries ?? Enumerable.Empty<BrickRoleMapEntry>())
                .OrderBy(entry => entry.Element.Id.Value, StringComparer.Ordinal)
                .ToArray();
            Schema = schema ?? string.Empty;
        }

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickRoleMapEntry> Entries { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);

        public static BrickRoleMapDocument FromResolvedRoles(DateTimeOffset generatedAt, IEnumerable<BrickResolvedRoles> resolvedRoles, string schema = CurrentSchema) =>
            new BrickRoleMapDocument(
                generatedAt,
                (resolvedRoles ?? Enumerable.Empty<BrickResolvedRoles>()).Select(BrickRoleMapEntry.FromResolvedRoles),
                schema);
    }

    public sealed class BrickRoleMapEntry
    {
        public BrickRoleMapEntry(
            BrickElement element,
            IEnumerable<RoleId> effectiveRoles,
            bool hasConflicts,
            int appliedAssignmentCount,
            int suppressedAssignmentCount)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            EffectiveRoles = (effectiveRoles ?? Enumerable.Empty<RoleId>())
                .Distinct()
                .OrderBy(roleId => roleId.Value, StringComparer.Ordinal)
                .ToArray();
            HasConflicts = hasConflicts;
            AppliedAssignmentCount = appliedAssignmentCount;
            SuppressedAssignmentCount = suppressedAssignmentCount;
        }

        public BrickElement Element { get; }
        public IReadOnlyList<RoleId> EffectiveRoles { get; }
        public bool HasConflicts { get; }
        public int AppliedAssignmentCount { get; }
        public int SuppressedAssignmentCount { get; }

        internal static BrickRoleMapEntry FromResolvedRoles(BrickResolvedRoles resolvedRoles) =>
            new BrickRoleMapEntry(
                resolvedRoles.Element,
                resolvedRoles.EffectiveRoles,
                resolvedRoles.HasConflicts,
                resolvedRoles.AppliedAssignments.Count,
                resolvedRoles.SuppressedAssignments.Count);
    }

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

    public sealed class BrickDependencyGraphNode
    {
        public BrickDependencyGraphNode(BrickElement element)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
        }

        public BrickElement Element { get; }
    }

    public sealed class BrickDependencyGraphEdge
    {
        public BrickDependencyGraphEdge(
            BrickElement source,
            BrickElement target,
            BrickDependencyKindId kindId,
            BrickDependencyLayer layer,
            BrickDependencyStrength strength,
            BrickEvidenceLevel evidenceLevel)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            KindId = kindId;
            Layer = layer;
            Strength = strength;
            EvidenceLevel = evidenceLevel;
        }

        public BrickElement Source { get; }
        public BrickElement Target { get; }
        public BrickDependencyKindId KindId { get; }
        public BrickDependencyLayer Layer { get; }
        public BrickDependencyStrength Strength { get; }
        public BrickEvidenceLevel EvidenceLevel { get; }

        internal static BrickDependencyGraphEdge FromDependency(BrickDependency dependency) =>
            new BrickDependencyGraphEdge(
                dependency.Source,
                dependency.Target,
                dependency.KindId,
                dependency.Layer,
                dependency.Strength,
                dependency.EvidenceLevel);
    }
}
