using System;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksReportJsonSerializerTest
    {
        [Fact]
        public void SerializeWritesSchemaSummaryAndSortedViolations()
        {
            var generatedAt = new DateTimeOffset(2026, 6, 23, 17, 0, 0, TimeSpan.Zero);
            var later = Violation("type:Zeta", "type:Target", BrickViolationState.Suppressed);
            var earlier = Violation("type:Alpha", "type:Target", BrickViolationState.Active);
            var report = new BrickReportDocument(generatedAt, new[] { later, earlier });

            using var json = JsonDocument.Parse(BrickReportJsonSerializer.Serialize(report));
            var root = json.RootElement;

            Assert.Equal(BrickReportDocument.CurrentSchema, root.GetProperty("schema").GetString());
            Assert.Equal(generatedAt, root.GetProperty("generatedAt").GetDateTimeOffset());
            Assert.Equal(2, root.GetProperty("summary").GetProperty("total").GetInt32());
            Assert.Equal(1, root.GetProperty("summary").GetProperty("active").GetInt32());
            Assert.Equal(1, root.GetProperty("summary").GetProperty("suppressed").GetInt32());
            Assert.Equal(0, root.GetProperty("summary").GetProperty("baselined").GetInt32());
            Assert.Equal(0, root.GetProperty("summary").GetProperty("expiredSuppressions").GetInt32());
            Assert.Equal(0, root.GetProperty("summary").GetProperty("expiredBaselineEntries").GetInt32());

            var violations = root.GetProperty("violations").EnumerateArray().ToArray();
            Assert.Equal("type:Alpha", violations[0].GetProperty("source").GetProperty("id").GetString());
            Assert.Equal("type:Zeta", violations[1].GetProperty("source").GetProperty("id").GetString());
            Assert.Equal("DependencyRule", violations[0].GetProperty("kind").GetString());
            Assert.Equal("XMoleculesBricks0001", violations[0].GetProperty("ruleId").GetString());
            Assert.Equal("No dependency", violations[0].GetProperty("ruleName").GetString());
            Assert.Equal("type:Target", violations[0].GetProperty("target").GetProperty("id").GetString());
            Assert.Equal(new[] { "DDD.Entity", "DDD.Repository" }, violations[0].GetProperty("resolvedSourceRoles").EnumerateArray().Select(role => role.GetString()).ToArray());
            Assert.Equal(new[] { "Infrastructure" }, violations[0].GetProperty("resolvedTargetRoles").EnumerateArray().Select(role => role.GetString()).ToArray());
            Assert.Equal("uses", violations[0].GetProperty("dependencyKindId").GetString());
            Assert.Equal("Type", violations[0].GetProperty("scope").GetString());
            Assert.Equal("Static", violations[0].GetProperty("dependencyLayer").GetString());
            Assert.Equal("Error", violations[0].GetProperty("severity").GetString());
            Assert.Equal("Violation", violations[0].GetProperty("message").GetString());
            Assert.Equal("CompilerConfirmed", violations[0].GetProperty("evidenceLevel").GetString());
            Assert.Equal("Active", violations[0].GetProperty("state").GetString());
            Assert.Equal("Reason", violations[0].GetProperty("stateReason").GetString());
        }

        [Fact]
        public void SerializeOmitsNullOptionalViolationProperties()
        {
            var report = new BrickReportDocument(
                DateTimeOffset.UnixEpoch,
                new[]
                {
                    new BrickViolation(
                        BrickViolationKind.PolicyConfiguration,
                        new BrickElement(BrickElementId.From("type:Policy"), BrickElementKind.Type, "Policy"),
                        "Policy issue",
                        BrickSeverity.Warning,
                        BrickViolationState.Active)
                });

            using var json = JsonDocument.Parse(BrickReportJsonSerializer.Serialize(report));
            var violation = json.RootElement.GetProperty("violations").EnumerateArray().Single();

            Assert.False(violation.TryGetProperty("ruleId", out _));
            Assert.False(violation.TryGetProperty("target", out _));
            Assert.False(violation.TryGetProperty("dependencyKindId", out _));
            Assert.False(violation.TryGetProperty("dependencyLayer", out _));
            Assert.False(violation.TryGetProperty("stateReason", out _));
            Assert.Empty(violation.GetProperty("resolvedSourceRoles").EnumerateArray());
            Assert.Empty(violation.GetProperty("resolvedTargetRoles").EnumerateArray());
        }

        [Fact]
        public void SerializeRequiresReport()
        {
            Assert.Throws<ArgumentNullException>(() => BrickReportJsonSerializer.Serialize(null));
        }

        private static BrickViolation Violation(string sourceId, string targetId, BrickViolationState state) =>
            new BrickViolation(
                BrickViolationKind.DependencyRule,
                new BrickElement(
                    BrickElementId.From(sourceId),
                    BrickElementKind.Type,
                    sourceId.Substring("type:".Length),
                    "Billing",
                    "Billing.Domain",
                    sourceId.Replace("type:", "Billing.Domain."),
                    BrickElementOrigin.Source,
                    BrickElementSource.Code),
                "Violation",
                BrickSeverity.Error,
                state,
                RuleId.From("XMoleculesBricks0001"),
                "No dependency",
                new BrickElement(BrickElementId.From(targetId), BrickElementKind.Type, "Target"),
                new[] { RoleId.From("DDD.Repository"), RoleId.From("DDD.Entity") },
                new[] { RoleId.From("Infrastructure") },
                BrickDependencyKindId.From("uses"),
                BrickScope.Type,
                BrickDependencyLayer.Static,
                BrickEvidenceLevel.CompilerConfirmed,
                "Reason");
    }
}
