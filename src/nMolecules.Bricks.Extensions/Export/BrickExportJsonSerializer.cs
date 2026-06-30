using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Serializes and deserializes export serializer documents using the stable Bricks JSON format.
/// </summary>
public static class BrickExportJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        public static string Serialize(BrickRoleMapDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return JsonSerializer.Serialize(ToDto(document), Options);
        }

        public static string Serialize(BrickDependencyGraphDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return JsonSerializer.Serialize(ToDto(document), Options);
        }

        public static string Serialize(BrickResolutionTraceDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return JsonSerializer.Serialize(ToDto(document), Options);
        }

        private static RoleMapDto ToDto(BrickRoleMapDocument document) =>
            new RoleMapDto
            {
                Schema = document.Schema,
                GeneratedAt = document.GeneratedAt,
                Entries = document.Entries
                    .Select(entry => new RoleMapEntryDto
                    {
                        Element = ToDto(entry.Element),
                        EffectiveRoles = entry.EffectiveRoles.Select(role => role.Value).ToArray(),
                        HasConflicts = entry.HasConflicts,
                        AppliedAssignmentCount = entry.AppliedAssignmentCount,
                        SuppressedAssignmentCount = entry.SuppressedAssignmentCount
                    })
                    .ToArray()
            };

        private static DependencyGraphDto ToDto(BrickDependencyGraphDocument document) =>
            new DependencyGraphDto
            {
                Schema = document.Schema,
                GeneratedAt = document.GeneratedAt,
                Nodes = document.Nodes
                    .Select(node => new DependencyGraphNodeDto { Element = ToDto(node.Element) })
                    .ToArray(),
                Edges = document.Edges
                    .Select(edge => new DependencyGraphEdgeDto
                    {
                        SourceId = edge.Source.Id.Value,
                        TargetId = edge.Target.Id.Value,
                        KindId = edge.KindId.Value,
                        Layer = edge.Layer.ToString(),
                        Strength = edge.Strength.ToString(),
                        EvidenceLevel = edge.EvidenceLevel.ToString()
                    })
                    .ToArray()
            };

        private static ResolutionTraceDto ToDto(BrickResolutionTraceDocument document) =>
            new ResolutionTraceDto
            {
                Schema = document.Schema,
                GeneratedAt = document.GeneratedAt,
                Entries = document.Entries
                    .Select(entry => new ResolutionTraceEntryDto
                    {
                        Element = ToDto(entry.Element),
                        CandidateRoles = entry.CandidateRoles.Select(role => role.Value).ToArray(),
                        ResolvedRoles = entry.ResolvedRoles.Select(role => role.Value).ToArray(),
                        Decisions = entry.Decisions.ToArray(),
                        HasConflict = entry.HasConflict
                    })
                    .ToArray()
            };

        private static ElementDto ToDto(BrickElement element) =>
            new ElementDto
            {
                Id = element.Id.Value,
                Kind = element.Kind.ToString(),
                DisplayName = element.DisplayName,
                AssemblyName = element.AssemblyName,
                NamespaceName = element.NamespaceName,
                FullName = element.FullName,
                Origin = element.Origin.ToString(),
                Source = element.Source.ToString()
            };

        private sealed class RoleMapDto
        {
            public string Schema { get; set; }
            public DateTimeOffset GeneratedAt { get; set; }
            public RoleMapEntryDto[] Entries { get; set; }
        }

        private sealed class RoleMapEntryDto
        {
            public ElementDto Element { get; set; }
            public string[] EffectiveRoles { get; set; }
            public bool HasConflicts { get; set; }
            public int AppliedAssignmentCount { get; set; }
            public int SuppressedAssignmentCount { get; set; }
        }

        private sealed class DependencyGraphDto
        {
            public string Schema { get; set; }
            public DateTimeOffset GeneratedAt { get; set; }
            public DependencyGraphNodeDto[] Nodes { get; set; }
            public DependencyGraphEdgeDto[] Edges { get; set; }
        }

        private sealed class DependencyGraphNodeDto
        {
            public ElementDto Element { get; set; }
        }

        private sealed class DependencyGraphEdgeDto
        {
            public string SourceId { get; set; }
            public string TargetId { get; set; }
            public string KindId { get; set; }
            public string Layer { get; set; }
            public string Strength { get; set; }
            public string EvidenceLevel { get; set; }
        }

        private sealed class ResolutionTraceDto
        {
            public string Schema { get; set; }
            public DateTimeOffset GeneratedAt { get; set; }
            public ResolutionTraceEntryDto[] Entries { get; set; }
        }

        private sealed class ResolutionTraceEntryDto
        {
            public ElementDto Element { get; set; }
            public string[] CandidateRoles { get; set; }
            public string[] ResolvedRoles { get; set; }
            public string[] Decisions { get; set; }
            public bool HasConflict { get; set; }
        }

        private sealed class ElementDto
        {
            public string Id { get; set; }
            public string Kind { get; set; }
            public string DisplayName { get; set; }
            public string AssemblyName { get; set; }
            public string NamespaceName { get; set; }
            public string FullName { get; set; }
            public string Origin { get; set; }
            public string Source { get; set; }
        }
    }
}
