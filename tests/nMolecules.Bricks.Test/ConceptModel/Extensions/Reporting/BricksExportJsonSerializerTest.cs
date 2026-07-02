using System;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksExportJsonSerializerTest
    {
        [Fact]
        public void SerializeRoleMapWritesEntries()
        {
            var generatedAt = new DateTimeOffset(2026, 6, 23, 18, 0, 0, TimeSpan.Zero);
            var document = new BrickRoleMapDocument(
                generatedAt,
                new[]
                {
                    new BrickRoleMapEntry(Element("type:Order"), new[] { RoleId.From("DDD.Entity"), RoleId.From("Business.Billing") }, true, 2, 1)
                });

            using var json = JsonDocument.Parse(BrickExportJsonSerializer.Serialize(document));
            var root = json.RootElement;
            var entry = root.GetProperty("entries").EnumerateArray().Single();

            Assert.Equal(BrickRoleMapDocument.CurrentSchema, root.GetProperty("schema").GetString());
            Assert.Equal(generatedAt, root.GetProperty("generatedAt").GetDateTimeOffset());
            Assert.Equal("type:Order", entry.GetProperty("element").GetProperty("id").GetString());
            Assert.Equal(new[] { "Business.Billing", "DDD.Entity" }, entry.GetProperty("effectiveRoles").EnumerateArray().Select(role => role.GetString()).ToArray());
            Assert.True(entry.GetProperty("hasConflicts").GetBoolean());
            Assert.Equal(2, entry.GetProperty("appliedAssignmentCount").GetInt32());
            Assert.Equal(1, entry.GetProperty("suppressedAssignmentCount").GetInt32());
        }

        [Fact]
        public void SerializeDependencyGraphWritesNodesAndEdges()
        {
            var source = Element("type:Order");
            var target = Element("type:Repository");
            var document = new BrickDependencyGraphDocument(
                DateTimeOffset.UnixEpoch,
                new[] { new BrickDependencyGraphNode(source), new BrickDependencyGraphNode(target) },
                new[] { new BrickDependencyGraphEdge(source, target, BrickDependencyKindId.From("uses"), BrickDependencyLayer.Static, BrickDependencyStrength.Direct, BrickEvidenceLevel.CompilerConfirmed) });

            using var json = JsonDocument.Parse(BrickExportJsonSerializer.Serialize(document));
            var root = json.RootElement;
            var edge = root.GetProperty("edges").EnumerateArray().Single();

            Assert.Equal(BrickDependencyGraphDocument.CurrentSchema, root.GetProperty("schema").GetString());
            Assert.Equal(new[] { "type:Order", "type:Repository" }, root.GetProperty("nodes").EnumerateArray().Select(node => node.GetProperty("element").GetProperty("id").GetString()).ToArray());
            Assert.Equal("type:Order", edge.GetProperty("sourceId").GetString());
            Assert.Equal("type:Repository", edge.GetProperty("targetId").GetString());
            Assert.Equal("uses", edge.GetProperty("kindId").GetString());
            Assert.Equal("Static", edge.GetProperty("layer").GetString());
            Assert.Equal("Direct", edge.GetProperty("strength").GetString());
            Assert.Equal("CompilerConfirmed", edge.GetProperty("evidenceLevel").GetString());
        }

        [Fact]
        public void SerializeResolutionTraceWritesEntries()
        {
            var document = new BrickResolutionTraceDocument(
                DateTimeOffset.UnixEpoch,
                new[]
                {
                    new BrickResolutionTraceEntry(
                        Element("type:Order"),
                        new[] { RoleId.From("DDD.Entity"), RoleId.From("Business.Billing") },
                        new[] { RoleId.From("DDD.Entity") },
                        new[] { "Applied DDD role.", "Suppressed business alias." },
                        true)
                });

            using var json = JsonDocument.Parse(BrickExportJsonSerializer.Serialize(document));
            var root = json.RootElement;
            var entry = root.GetProperty("entries").EnumerateArray().Single();

            Assert.Equal(BrickResolutionTraceDocument.CurrentSchema, root.GetProperty("schema").GetString());
            Assert.Equal("type:Order", entry.GetProperty("element").GetProperty("id").GetString());
            Assert.Equal(new[] { "Business.Billing", "DDD.Entity" }, entry.GetProperty("candidateRoles").EnumerateArray().Select(role => role.GetString()).ToArray());
            Assert.Equal(new[] { "DDD.Entity" }, entry.GetProperty("resolvedRoles").EnumerateArray().Select(role => role.GetString()).ToArray());
            Assert.Equal(new[] { "Applied DDD role.", "Suppressed business alias." }, entry.GetProperty("decisions").EnumerateArray().Select(decision => decision.GetString()).ToArray());
            Assert.True(entry.GetProperty("hasConflict").GetBoolean());
        }

        [Fact]
        public void SerializeOmitsNullOptionalElementProperties()
        {
            var document = new BrickRoleMapDocument(
                DateTimeOffset.UnixEpoch,
                new[] { new BrickRoleMapEntry(new BrickElement(BrickElementId.From("type:Order"), BrickElementKind.Type, "Order"), null, false, 0, 0) });

            using var json = JsonDocument.Parse(BrickExportJsonSerializer.Serialize(document));
            var element = json.RootElement.GetProperty("entries").EnumerateArray().Single().GetProperty("element");

            Assert.False(element.TryGetProperty("assemblyName", out _));
            Assert.False(element.TryGetProperty("namespaceName", out _));
            Assert.False(element.TryGetProperty("fullName", out _));
        }

        [Fact]
        public void SerializeRequiresDocuments()
        {
            Assert.Throws<ArgumentNullException>(() => BrickExportJsonSerializer.Serialize((BrickRoleMapDocument)null));
            Assert.Throws<ArgumentNullException>(() => BrickExportJsonSerializer.Serialize((BrickDependencyGraphDocument)null));
            Assert.Throws<ArgumentNullException>(() => BrickExportJsonSerializer.Serialize((BrickResolutionTraceDocument)null));
        }

        private static BrickElement Element(string id) =>
            new BrickElement(
                BrickElementId.From(id),
                BrickElementKind.Type,
                id.Substring("type:".Length),
                "Billing",
                "Billing.Domain",
                id.Replace("type:", "Billing.Domain."),
                BrickElementOrigin.Source,
                BrickElementSource.Code);
    }
}
