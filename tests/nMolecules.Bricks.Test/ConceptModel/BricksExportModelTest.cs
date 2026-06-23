using System;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksExportModelTest
    {
        [Fact]
        public void BrickRoleMapDocumentProjectsResolvedRolesDeterministically()
        {
            var generatedAt = new DateTimeOffset(2026, 6, 23, 13, 0, 0, TimeSpan.Zero);
            var alpha = Element("type:Alpha", "Alpha");
            var beta = Element("type:Beta", "Beta");
            var betaAssignment = Assignment("Business.Domain");
            var alphaAssignment = Assignment("Contracts.Api");

            var roleMap = BrickRoleMapDocument.FromResolvedRoles(
                generatedAt,
                new[]
                {
                    new BrickResolvedRoles(beta, new[] { betaAssignment }, new[] { betaAssignment }, null, null),
                    new BrickResolvedRoles(alpha, new[] { alphaAssignment }, new[] { alphaAssignment }, new[] { Assignment("Legacy") }, new[] { new BrickRoleConflict(alphaAssignment, Assignment("Generated"), "Conflict") })
                });

            Assert.Equal(BrickRoleMapDocument.CurrentSchema, roleMap.Schema);
            Assert.True(roleMap.IsCurrentSchema);
            Assert.Equal(generatedAt, roleMap.GeneratedAt);
            Assert.Equal(new[] { alpha, beta }, roleMap.Entries.Select(entry => entry.Element).ToArray());
            Assert.Equal(new[] { RoleId.From("Contracts.Api") }, roleMap.Entries[0].EffectiveRoles.ToArray());
            Assert.True(roleMap.Entries[0].HasConflicts);
            Assert.Equal(1, roleMap.Entries[0].AppliedAssignmentCount);
            Assert.Equal(1, roleMap.Entries[0].SuppressedAssignmentCount);
            Assert.False(roleMap.Entries[1].HasConflicts);
        }

        [Fact]
        public void BrickRoleMapDocumentNormalizesNullInputs()
        {
            var document = BrickRoleMapDocument.FromResolvedRoles(DateTimeOffset.UnixEpoch, null, null);

            Assert.Equal(string.Empty, document.Schema);
            Assert.False(document.IsCurrentSchema);
            Assert.Empty(document.Entries);
        }

        [Fact]
        public void BrickRoleMapDocumentDirectConstructorUsesCurrentSchemaAndNormalizesNullEntries()
        {
            var document = new BrickRoleMapDocument(DateTimeOffset.UnixEpoch, null);

            Assert.Equal(BrickRoleMapDocument.CurrentSchema, document.Schema);
            Assert.True(document.IsCurrentSchema);
            Assert.Empty(document.Entries);
        }

        [Fact]
        public void BrickRoleMapEntryCopiesAndSortsRoles()
        {
            var roles = new[] { RoleId.From("Zeta"), RoleId.From("Alpha"), RoleId.From("Alpha") };
            var entry = new BrickRoleMapEntry(Element("type:Order", "Order"), roles, true, 2, 1);
            roles[0] = RoleId.From("Mutated");

            Assert.Equal(new[] { RoleId.From("Alpha"), RoleId.From("Zeta") }, entry.EffectiveRoles.ToArray());
            Assert.True(entry.HasConflicts);
            Assert.Equal(2, entry.AppliedAssignmentCount);
            Assert.Equal(1, entry.SuppressedAssignmentCount);
        }

        [Fact]
        public void BrickRoleMapEntryNormalizesNullRoles()
        {
            var entry = new BrickRoleMapEntry(Element("type:Order", "Order"), null, false, 0, 0);

            Assert.Empty(entry.EffectiveRoles);
        }

        [Fact]
        public void BrickRoleMapEntryRequiresElement()
        {
            Assert.Throws<ArgumentNullException>(() => new BrickRoleMapEntry(null, null, false, 0, 0));
        }

        [Fact]
        public void BrickDependencyGraphDocumentProjectsDependenciesDeterministically()
        {
            var generatedAt = new DateTimeOffset(2026, 6, 23, 14, 0, 0, TimeSpan.Zero);
            var alpha = Element("type:Alpha", "Alpha");
            var beta = Element("type:Beta", "Beta");
            var gamma = Element("type:Gamma", "Gamma");
            var dependencies = new[]
            {
                Dependency(beta, gamma, "uses"),
                Dependency(alpha, beta, "references")
            };

            var graph = BrickDependencyGraphDocument.FromDependencies(generatedAt, dependencies);
            dependencies[0] = Dependency(alpha, gamma, "mutated");

            Assert.Equal(BrickDependencyGraphDocument.CurrentSchema, graph.Schema);
            Assert.True(graph.IsCurrentSchema);
            Assert.Equal(generatedAt, graph.GeneratedAt);
            Assert.Equal(new[] { alpha, beta, gamma }, graph.Nodes.Select(node => node.Element).ToArray());
            Assert.Equal(new[] { "type:Alpha->type:Beta:references", "type:Beta->type:Gamma:uses" }, graph.Edges.Select(EdgeKey).ToArray());
            Assert.Equal(BrickDependencyLayer.Static, graph.Edges[0].Layer);
            Assert.Equal(BrickDependencyStrength.Direct, graph.Edges[0].Strength);
            Assert.Equal(BrickEvidenceLevel.CompilerConfirmed, graph.Edges[0].EvidenceLevel);
        }

        [Fact]
        public void BrickDependencyGraphDocumentNormalizesNullInputs()
        {
            var graph = BrickDependencyGraphDocument.FromDependencies(DateTimeOffset.UnixEpoch, null, null);

            Assert.Equal(string.Empty, graph.Schema);
            Assert.False(graph.IsCurrentSchema);
            Assert.Empty(graph.Nodes);
            Assert.Empty(graph.Edges);
        }

        [Fact]
        public void BrickDependencyGraphDocumentDirectConstructorUsesCurrentSchemaAndNormalizesNullCollections()
        {
            var graph = new BrickDependencyGraphDocument(DateTimeOffset.UnixEpoch, null, null);

            Assert.Equal(BrickDependencyGraphDocument.CurrentSchema, graph.Schema);
            Assert.True(graph.IsCurrentSchema);
            Assert.Empty(graph.Nodes);
            Assert.Empty(graph.Edges);
        }

        [Fact]
        public void BrickDependencyGraphNodeRequiresElement()
        {
            Assert.Throws<ArgumentNullException>(() => new BrickDependencyGraphNode(null));
        }

        [Fact]
        public void BrickDependencyGraphEdgeRequiresSourceAndTarget()
        {
            var target = Element("type:Target", "Target");

            Assert.Throws<ArgumentNullException>(() => new BrickDependencyGraphEdge(null, target, BrickDependencyKindId.From("uses"), BrickDependencyLayer.Static, BrickDependencyStrength.Direct, BrickEvidenceLevel.CompilerConfirmed));
            Assert.Throws<ArgumentNullException>(() => new BrickDependencyGraphEdge(target, null, BrickDependencyKindId.From("uses"), BrickDependencyLayer.Static, BrickDependencyStrength.Direct, BrickEvidenceLevel.CompilerConfirmed));
        }

        [Fact]
        public void BrickResolutionTraceDocumentProjectsTracesDeterministically()
        {
            var generatedAt = new DateTimeOffset(2026, 6, 23, 15, 0, 0, TimeSpan.Zero);
            var alpha = Element("type:Alpha", "Alpha");
            var beta = Element("type:Beta", "Beta");
            var betaTrace = new BrickResolutionTrace(
                beta,
                new[] { Assignment("Contracts.Api") },
                new[] { RoleId.From("Contracts.Api") },
                new[] { "Applied direct role." },
                false);
            var alphaTrace = new BrickResolutionTrace(
                alpha,
                new[] { Assignment("Generated"), Assignment("Business.Domain") },
                new[] { RoleId.From("Business.Domain") },
                new[] { "Suppressed generated alias.", null },
                true);

            var document = BrickResolutionTraceDocument.FromTraces(generatedAt, new[] { betaTrace, alphaTrace });

            Assert.Equal(BrickResolutionTraceDocument.CurrentSchema, document.Schema);
            Assert.True(document.IsCurrentSchema);
            Assert.Equal(generatedAt, document.GeneratedAt);
            Assert.Equal(new[] { alpha, beta }, document.Entries.Select(entry => entry.Element).ToArray());
            Assert.Equal(new[] { RoleId.From("Business.Domain"), RoleId.From("Generated") }, document.Entries[0].CandidateRoles.ToArray());
            Assert.Equal(new[] { RoleId.From("Business.Domain") }, document.Entries[0].ResolvedRoles.ToArray());
            Assert.Equal(new[] { "Suppressed generated alias.", string.Empty }, document.Entries[0].Decisions.ToArray());
            Assert.True(document.Entries[0].HasConflict);
            Assert.False(document.Entries[1].HasConflict);
        }

        [Fact]
        public void BrickResolutionTraceDocumentNormalizesNullInputs()
        {
            var document = BrickResolutionTraceDocument.FromTraces(DateTimeOffset.UnixEpoch, null, null);

            Assert.Equal(string.Empty, document.Schema);
            Assert.False(document.IsCurrentSchema);
            Assert.Empty(document.Entries);
        }

        [Fact]
        public void BrickResolutionTraceDocumentDirectConstructorUsesCurrentSchemaAndNormalizesNullEntries()
        {
            var document = new BrickResolutionTraceDocument(DateTimeOffset.UnixEpoch, null);

            Assert.Equal(BrickResolutionTraceDocument.CurrentSchema, document.Schema);
            Assert.True(document.IsCurrentSchema);
            Assert.Empty(document.Entries);
        }

        [Fact]
        public void BrickResolutionTraceEntryCopiesAndSortsRoles()
        {
            var candidateRoles = new[] { RoleId.From("Zeta"), RoleId.From("Alpha"), RoleId.From("Alpha") };
            var resolvedRoles = new[] { RoleId.From("Zeta"), RoleId.From("Alpha"), RoleId.From("Alpha") };
            var decisions = new[] { "A", null };

            var entry = new BrickResolutionTraceEntry(Element("type:Order", "Order"), candidateRoles, resolvedRoles, decisions, false);
            candidateRoles[0] = RoleId.From("Mutated");
            resolvedRoles[0] = RoleId.From("Mutated");
            decisions[0] = "Mutated";

            Assert.Equal(new[] { RoleId.From("Alpha"), RoleId.From("Zeta") }, entry.CandidateRoles.ToArray());
            Assert.Equal(new[] { RoleId.From("Alpha"), RoleId.From("Zeta") }, entry.ResolvedRoles.ToArray());
            Assert.Equal(new[] { "A", string.Empty }, entry.Decisions.ToArray());
            Assert.False(entry.HasConflict);
        }

        [Fact]
        public void BrickResolutionTraceEntryNormalizesNullCollections()
        {
            var entry = new BrickResolutionTraceEntry(Element("type:Order", "Order"), null, null, null, false);

            Assert.Empty(entry.CandidateRoles);
            Assert.Empty(entry.ResolvedRoles);
            Assert.Empty(entry.Decisions);
        }

        [Fact]
        public void BrickResolutionTraceEntryRequiresElement()
        {
            Assert.Throws<ArgumentNullException>(() => new BrickResolutionTraceEntry(null, null, null, null, false));
        }

        private static BrickElement Element(string id, string displayName) =>
            new BrickElement(BrickElementId.From(id), BrickElementKind.Type, displayName);

        private static BrickRoleAssignment Assignment(string roleId) =>
            new BrickRoleAssignment(
                null,
                RoleId.From(roleId),
                BrickAssignmentMode.DirectAttribute,
                BrickAssignmentSource.SourceAttribute,
                new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct),
                BrickAssignmentBehavior.Apply);

        private static BrickDependency Dependency(BrickElement source, BrickElement target, string kindId) =>
            new BrickDependency(
                source,
                target,
                BrickDependencyKindId.From(kindId),
                BrickScope.Type,
                BrickDependencyLayer.Static,
                BrickDependencyStrength.Direct,
                BrickEvidenceLevel.CompilerConfirmed);

        private static string EdgeKey(BrickDependencyGraphEdge edge) =>
            $"{edge.Source.Id.Value}->{edge.Target.Id.Value}:{edge.KindId.Value}";
    }
}
