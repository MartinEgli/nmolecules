using System;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace NMolecules.Bricks.Test.ConceptModel
{
    public class BricksDependencyCoverageModelTest
    {
        [Fact]
        public void CoverageTargetCapturesDependencyKindLayerAndEvidenceRequirement()
        {
            var target = new BrickDependencyCoverageTarget(
                BrickDependencyKindId.From("DependencyRegistration"),
                BrickDependencyLayer.Runtime,
                BrickEvidenceLevel.AnalyzerInferred,
                true,
                "DI registrations are partially observable.");

            Assert.Equal(BrickDependencyKindId.From("DependencyRegistration"), target.KindId);
            Assert.Equal(BrickDependencyLayer.Runtime, target.Layer);
            Assert.Equal(BrickEvidenceLevel.AnalyzerInferred, target.MinimumEvidenceLevel);
            Assert.True(target.Required);
            Assert.Equal("DI registrations are partially observable.", target.Rationale);
        }

        [Fact]
        public void CoverageTargetNormalizesRationale()
        {
            var target = new BrickDependencyCoverageTarget(
                default,
                BrickDependencyLayer.Static,
                BrickEvidenceLevel.CompilerConfirmed,
                false,
                null);

            Assert.True(target.KindId.IsEmpty);
            Assert.Equal(string.Empty, target.Rationale);
        }

        [Theory]
        [InlineData(BrickEvidenceLevel.CompilerConfirmed, BrickEvidenceLevel.CompilerConfirmed, true)]
        [InlineData(BrickEvidenceLevel.CompilerConfirmed, BrickEvidenceLevel.AnalyzerInferred, true)]
        [InlineData(BrickEvidenceLevel.AnalyzerInferred, BrickEvidenceLevel.CompilerConfirmed, false)]
        [InlineData(BrickEvidenceLevel.ConfigurationDeclared, BrickEvidenceLevel.ConfigurationDeclared, true)]
        [InlineData(BrickEvidenceLevel.ConfigurationDeclared, BrickEvidenceLevel.RuntimeInferred, true)]
        [InlineData(BrickEvidenceLevel.Unknown, BrickEvidenceLevel.RuntimeInferred, false)]
        [InlineData(BrickEvidenceLevel.Unknown, BrickEvidenceLevel.Unknown, true)]
        public void CoverageTargetComparesEvidenceByConfidence(
            BrickEvidenceLevel observed,
            BrickEvidenceLevel minimum,
            bool expected)
        {
            var target = new BrickDependencyCoverageTarget(
                BrickDependencyKindId.From("uses"),
                BrickDependencyLayer.Static,
                minimum,
                true);

            Assert.Equal(expected, target.IsSatisfiedBy(observed));
        }

        [Fact]
        public void CoverageResultReportsCoveredDependencyKind()
        {
            var target = Target("uses", BrickDependencyLayer.Static, BrickEvidenceLevel.CompilerConfirmed);

            var result = new BrickDependencyCoverageResult(target, 4, 4, BrickEvidenceLevel.CompilerConfirmed, "static pass");

            Assert.Equal(target, result.Target);
            Assert.Equal(4, result.AnalyzedDependencies);
            Assert.Equal(4, result.ObservableDependencies);
            Assert.Equal(0, result.UnobservableDependencies);
            Assert.Equal(1.0, result.CoverageRatio);
            Assert.Equal(BrickDependencyCoverageStatus.Covered, result.Status);
            Assert.Equal("static pass", result.Notes);
            Assert.True(result.MeetsEvidenceRequirement);
        }

        [Fact]
        public void CoverageResultReportsPartialObservability()
        {
            var target = Target("ReflectionAccess", BrickDependencyLayer.Runtime, BrickEvidenceLevel.RuntimeInferred);

            var result = new BrickDependencyCoverageResult(target, 10, 3, BrickEvidenceLevel.RuntimeInferred);

            Assert.Equal(3, result.ObservableDependencies);
            Assert.Equal(7, result.UnobservableDependencies);
            Assert.Equal(0.3, result.CoverageRatio, 3);
            Assert.Equal(BrickDependencyCoverageStatus.PartiallyObservable, result.Status);
        }

        [Fact]
        public void CoverageResultReportsNotObservableWhenNothingCanBeSeen()
        {
            var target = Target("RuntimeActivation", BrickDependencyLayer.Runtime, BrickEvidenceLevel.RuntimeInferred);

            var result = new BrickDependencyCoverageResult(target, 0, 0, BrickEvidenceLevel.Unknown);

            Assert.Equal(0, result.CoverageRatio);
            Assert.Equal(BrickDependencyCoverageStatus.NotObservable, result.Status);
            Assert.False(result.MeetsEvidenceRequirement);
        }

        [Fact]
        public void CoverageResultReportsInsufficientEvidenceBeforeCoverageStatus()
        {
            var target = Target("DependencyRegistration", BrickDependencyLayer.Runtime, BrickEvidenceLevel.AnalyzerInferred);

            var result = new BrickDependencyCoverageResult(target, 2, 2, BrickEvidenceLevel.RuntimeInferred);

            Assert.Equal(BrickDependencyCoverageStatus.InsufficientEvidence, result.Status);
            Assert.False(result.MeetsEvidenceRequirement);
            Assert.Equal(1.0, result.CoverageRatio);
        }

        [Fact]
        public void CoverageResultNormalizesCountsAndCapsRatio()
        {
            var target = Target("uses", BrickDependencyLayer.Static, BrickEvidenceLevel.CompilerConfirmed);

            var negative = new BrickDependencyCoverageResult(target, -1, -3, BrickEvidenceLevel.CompilerConfirmed, null);
            var capped = new BrickDependencyCoverageResult(target, 2, 5, BrickEvidenceLevel.CompilerConfirmed);

            Assert.Equal(0, negative.AnalyzedDependencies);
            Assert.Equal(0, negative.ObservableDependencies);
            Assert.Equal(string.Empty, negative.Notes);
            Assert.Equal(1.0, capped.CoverageRatio);
            Assert.Equal(0, capped.UnobservableDependencies);
        }

        [Fact]
        public void CoverageResultRequiresTarget()
        {
            Assert.Throws<ArgumentNullException>(() => new BrickDependencyCoverageResult(null, 1, 1, BrickEvidenceLevel.CompilerConfirmed));
        }

        [Fact]
        public void BuiltInCoverageTargetsExposeCentralDependencyKindsInStableOrder()
        {
            var targets = BrickBuiltInDependencyCoverageTargets.All.ToArray();

            Assert.Equal(
                new[]
                {
                    "DependencyRegistration",
                    "FriendAssembly",
                    "ReflectionAccess",
                    "RuntimeActivation",
                    BrickDependencyKinds.TypeReference
                },
                targets.Select(target => target.KindId.Value).ToArray());
            Assert.Equal(BrickDependencyRegistration.DependencyKind, targets[0].KindId.Value);
            Assert.Equal(BrickFriendAssemblyGrant.DependencyKind, targets[1].KindId.Value);
            Assert.Equal(BrickReflectionAccess.DependencyKind, targets[2].KindId.Value);
            Assert.Equal(BrickRuntimeActivation.DependencyKind, targets[3].KindId.Value);
            Assert.All(targets, target => Assert.False(string.IsNullOrWhiteSpace(target.Rationale)));
        }

        [Fact]
        public void CoverageReportSortsResultsAndSummarizesStatuses()
        {
            var generatedAt = new DateTimeOffset(2026, 6, 23, 10, 0, 0, TimeSpan.Zero);
            var partial = new BrickDependencyCoverageResult(Target("z", BrickDependencyLayer.Runtime, BrickEvidenceLevel.RuntimeInferred), 4, 2, BrickEvidenceLevel.RuntimeInferred);
            var covered = new BrickDependencyCoverageResult(Target("a", BrickDependencyLayer.Static, BrickEvidenceLevel.CompilerConfirmed), 2, 2, BrickEvidenceLevel.CompilerConfirmed);
            var missing = new BrickDependencyCoverageResult(Target("m", BrickDependencyLayer.Runtime, BrickEvidenceLevel.RuntimeInferred), 0, 0, BrickEvidenceLevel.Unknown);
            var weak = new BrickDependencyCoverageResult(Target("w", BrickDependencyLayer.Runtime, BrickEvidenceLevel.AnalyzerInferred), 1, 1, BrickEvidenceLevel.RuntimeInferred);

            var report = new BrickDependencyCoverageReport(generatedAt, new[] { partial, covered, missing, weak });

            Assert.Equal(BrickDependencyCoverageReport.CurrentSchema, report.Schema);
            Assert.True(report.IsCurrentSchema);
            Assert.Equal(generatedAt, report.GeneratedAt);
            Assert.Equal(new[] { "a", "m", "w", "z" }, report.Results.Select(result => result.Target.KindId.Value).ToArray());
            Assert.Equal(4, report.Summary.Total);
            Assert.Equal(1, report.Summary.Covered);
            Assert.Equal(1, report.Summary.PartiallyObservable);
            Assert.Equal(1, report.Summary.NotObservable);
            Assert.Equal(1, report.Summary.InsufficientEvidence);
            Assert.Equal(0.625, report.Summary.AverageCoverageRatio, 3);
        }

        [Fact]
        public void CoverageReportNormalizesNullResultsAndSchema()
        {
            var generatedAt = new DateTimeOffset(2026, 6, 23, 10, 0, 0, TimeSpan.Zero);

            var report = new BrickDependencyCoverageReport(generatedAt, null, null);

            Assert.Equal(generatedAt, report.GeneratedAt);
            Assert.Empty(report.Results);
            Assert.Equal(string.Empty, report.Schema);
            Assert.False(report.IsCurrentSchema);
            Assert.Equal(0, report.Summary.Total);
            Assert.Equal(0, report.Summary.AverageCoverageRatio);
        }

        [Fact]
        public void CoverageSummaryNormalizesNullResults()
        {
            var summary = BrickDependencyCoverageSummary.FromResults(null);

            Assert.Equal(0, summary.Total);
            Assert.Equal(0, summary.Covered);
            Assert.Equal(0, summary.PartiallyObservable);
            Assert.Equal(0, summary.NotObservable);
            Assert.Equal(0, summary.InsufficientEvidence);
            Assert.Equal(0, summary.AverageCoverageRatio);
        }

        [Fact]
        public void CoverageJsonSerializerWritesVersionedMachineReadableReport()
        {
            var generatedAt = new DateTimeOffset(2026, 6, 23, 10, 0, 0, TimeSpan.Zero);
            var report = new BrickDependencyCoverageReport(
                generatedAt,
                new[]
                {
                    new BrickDependencyCoverageResult(Target("z", BrickDependencyLayer.Runtime, BrickEvidenceLevel.RuntimeInferred), 4, 2, BrickEvidenceLevel.RuntimeInferred, "runtime partial"),
                    new BrickDependencyCoverageResult(Target("a", BrickDependencyLayer.Static, BrickEvidenceLevel.CompilerConfirmed), 2, 2, BrickEvidenceLevel.CompilerConfirmed)
                });

            var json = BrickDependencyCoverageReportJsonSerializer.Serialize(report);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var results = root.GetProperty("results");

            Assert.Equal(BrickDependencyCoverageReport.CurrentSchema, root.GetProperty("schema").GetString());
            Assert.Equal(2, root.GetProperty("summary").GetProperty("total").GetInt32());
            Assert.Equal("a", results[0].GetProperty("kindId").GetString());
            Assert.Equal("Covered", results[0].GetProperty("status").GetString());
            Assert.Equal("z", results[1].GetProperty("kindId").GetString());
            Assert.Equal("Runtime", results[1].GetProperty("layer").GetString());
            Assert.Equal("PartiallyObservable", results[1].GetProperty("status").GetString());
            Assert.Equal("runtime partial", results[1].GetProperty("notes").GetString());
        }

        [Fact]
        public void CoverageJsonSerializerRequiresReport()
        {
            Assert.Throws<ArgumentNullException>(() => BrickDependencyCoverageReportJsonSerializer.Serialize(null));
        }

        private static BrickDependencyCoverageTarget Target(
            string kindId,
            BrickDependencyLayer layer,
            BrickEvidenceLevel minimumEvidenceLevel) =>
            new BrickDependencyCoverageTarget(
                BrickDependencyKindId.From(kindId),
                layer,
                minimumEvidenceLevel,
                true,
                "required");
    }
}
