using System;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksReportSarifSerializerTest
    {
        [Fact]
        public void SerializeWritesSarifRunRulesAndSortedResults()
        {
            var warning = Violation("type:Zeta", "XMoleculesBricks0002", BrickSeverity.Warning, BrickViolationState.Suppressed);
            var error = Violation("type:Alpha", "XMoleculesBricks0001", BrickSeverity.Error, BrickViolationState.Active);
            var report = new BrickReportDocument(new DateTimeOffset(2026, 6, 23, 18, 0, 0, TimeSpan.Zero), new[] { warning, error });

            using var json = JsonDocument.Parse(BrickReportSarifSerializer.Serialize(report));
            var root = json.RootElement;
            var run = root.GetProperty("runs").EnumerateArray().Single();
            var driver = run.GetProperty("tool").GetProperty("driver");
            var rules = driver.GetProperty("rules").EnumerateArray().ToArray();
            var results = run.GetProperty("results").EnumerateArray().ToArray();

            Assert.Equal("2.1.0", root.GetProperty("version").GetString());
            Assert.Equal("NMolecules.Bricks", driver.GetProperty("name").GetString());
            Assert.Equal(new[] { "XMoleculesBricks0001", "XMoleculesBricks0002" }, rules.Select(rule => rule.GetProperty("id").GetString()).ToArray());
            Assert.Equal(new[] { "type:Alpha", "type:Zeta" }, results.Select(result => result.GetProperty("properties").GetProperty("sourceId").GetString()).ToArray());
            Assert.Equal("error", results[0].GetProperty("level").GetString());
            Assert.Equal("warning", results[1].GetProperty("level").GetString());
            Assert.Equal("Violation", results[0].GetProperty("message").GetProperty("text").GetString());
            Assert.Equal("DependencyRule", results[0].GetProperty("properties").GetProperty("kind").GetString());
            Assert.Equal("Active", results[0].GetProperty("properties").GetProperty("state").GetString());
            Assert.Equal("CompilerConfirmed", results[0].GetProperty("properties").GetProperty("evidenceLevel").GetString());
            Assert.Equal("type:Target", results[0].GetProperty("properties").GetProperty("targetId").GetString());
            Assert.Equal("Billing.Domain.Alpha", results[0].GetProperty("locations").EnumerateArray().Single().GetProperty("logicalLocations").EnumerateArray().Single().GetProperty("fullyQualifiedName").GetString());
        }

        [Fact]
        public void SerializeMapsInfoAndUnknownRuleWithoutTarget()
        {
            var report = new BrickReportDocument(
                DateTimeOffset.UnixEpoch,
                new[]
                {
                    new BrickViolation(
                        BrickViolationKind.PolicyConfiguration,
                        new BrickElement(BrickElementId.From("type:Policy"), BrickElementKind.Type, "Policy"),
                        "Policy issue",
                        BrickSeverity.Info,
                        BrickViolationState.Baselined)
                });

            using var json = JsonDocument.Parse(BrickReportSarifSerializer.Serialize(report));
            var result = json.RootElement.GetProperty("runs").EnumerateArray().Single().GetProperty("results").EnumerateArray().Single();
            var rule = json.RootElement.GetProperty("runs").EnumerateArray().Single().GetProperty("tool").GetProperty("driver").GetProperty("rules").EnumerateArray().Single();

            Assert.Equal("NMolecules.Bricks.Unknown", rule.GetProperty("id").GetString());
            Assert.Equal("note", result.GetProperty("level").GetString());
            Assert.False(result.GetProperty("properties").TryGetProperty("targetId", out _));
        }

        [Fact]
        public void SerializeNormalizesNullViolationMessageAndRequiresReport()
        {
            var report = new BrickReportDocument(
                DateTimeOffset.UnixEpoch,
                new[]
                {
                    new BrickViolation(
                        BrickViolationKind.PolicyConfiguration,
                        new BrickElement(BrickElementId.From("type:Policy"), BrickElementKind.Type, "Policy"),
                        null,
                        BrickSeverity.Warning,
                        BrickViolationState.Active)
                });

            using var json = JsonDocument.Parse(BrickReportSarifSerializer.Serialize(report));
            var result = json.RootElement.GetProperty("runs").EnumerateArray().Single().GetProperty("results").EnumerateArray().Single();

            Assert.Equal(string.Empty, result.GetProperty("message").GetProperty("text").GetString());
            Assert.Throws<ArgumentNullException>(() => BrickReportSarifSerializer.Serialize(null));
        }

        private static BrickViolation Violation(string sourceId, string ruleId, BrickSeverity severity, BrickViolationState state) =>
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
                severity,
                state,
                RuleId.From(ruleId),
                "No dependency",
                new BrickElement(BrickElementId.From("type:Target"), BrickElementKind.Type, "Target"),
                new[] { RoleId.From("DDD.Entity") },
                new[] { RoleId.From("Infrastructure") },
                BrickDependencyKindId.From("uses"),
                BrickScope.Type,
                BrickDependencyLayer.Static,
                BrickEvidenceLevel.CompilerConfirmed,
                "Reason");
    }
}
