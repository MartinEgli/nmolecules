using System;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksBenchmarkComparisonModelTest
    {
        [Fact]
        public void BenchmarkComparisonThresholdRecordsRatiosAndRationale()
        {
            var threshold = new BrickBenchmarkComparisonThreshold(0.15, 0.07, "CI guardrail.");

            Assert.Equal(0.15, threshold.MaxAllowedSlowdownRatio);
            Assert.Equal(0.07, threshold.MinSignificantImprovementRatio);
            Assert.Equal("CI guardrail.", threshold.Rationale);
        }

        [Fact]
        public void BenchmarkComparisonThresholdNormalizesValues()
        {
            var threshold = new BrickBenchmarkComparisonThreshold(-1, -2, null);

            Assert.Equal(0, threshold.MaxAllowedSlowdownRatio);
            Assert.Equal(0, threshold.MinSignificantImprovementRatio);
            Assert.Equal(string.Empty, threshold.Rationale);
        }

        [Fact]
        public void BenchmarkComparisonMarksNoBaseline()
        {
            var current = Result("role", 100);

            var comparison = BrickBenchmarkComparison.Compare(null, current);

            Assert.Equal("role", comparison.Id);
            Assert.Equal(current, comparison.Current);
            Assert.Null(comparison.Baseline);
            Assert.Equal(BrickBenchmarkComparisonStatus.NoBaseline, comparison.Status);
            Assert.False(comparison.IsRegression);
            Assert.Equal(0, comparison.ElapsedPerOperationDeltaTicks);
            Assert.Equal(0, comparison.ElapsedPerOperationDeltaRatio);
        }

        [Fact]
        public void BenchmarkComparisonMarksStableWhenSlowdownIsInsideThreshold()
        {
            var baseline = Result("policy", 100);
            var current = Result("policy", 109);
            var threshold = new BrickBenchmarkComparisonThreshold(0.10, 0.05);

            var comparison = BrickBenchmarkComparison.Compare(baseline, current, threshold);

            Assert.Equal(BrickBenchmarkComparisonStatus.Stable, comparison.Status);
            Assert.False(comparison.IsRegression);
            Assert.Equal(9, comparison.ElapsedPerOperationDeltaTicks);
            Assert.Equal(0.09, comparison.ElapsedPerOperationDeltaRatio, 5);
        }

        [Fact]
        public void BenchmarkComparisonMarksRegressionWhenSlowdownExceedsThreshold()
        {
            var comparison = BrickBenchmarkComparison.Compare(
                Result("rules", 100),
                Result("rules", 125),
                new BrickBenchmarkComparisonThreshold(0.10, 0.05));

            Assert.Equal(BrickBenchmarkComparisonStatus.Regressed, comparison.Status);
            Assert.True(comparison.IsRegression);
            Assert.Equal(25, comparison.ElapsedPerOperationDeltaTicks);
            Assert.Equal(0.25, comparison.ElapsedPerOperationDeltaRatio, 5);
        }

        [Fact]
        public void BenchmarkComparisonMarksImprovementWhenGainExceedsThreshold()
        {
            var comparison = BrickBenchmarkComparison.Compare(
                Result("serialization", 100),
                Result("serialization", 92),
                new BrickBenchmarkComparisonThreshold(0.10, 0.05));

            Assert.Equal(BrickBenchmarkComparisonStatus.Improved, comparison.Status);
            Assert.False(comparison.IsRegression);
            Assert.Equal(-8, comparison.ElapsedPerOperationDeltaTicks);
            Assert.Equal(-0.08, comparison.ElapsedPerOperationDeltaRatio, 5);
        }

        [Fact]
        public void BenchmarkComparisonHandlesZeroBaselineAsStableWhenCurrentIsZero()
        {
            var comparison = BrickBenchmarkComparison.Compare(Result("zero", 0), Result("zero", 0));

            Assert.Equal(BrickBenchmarkComparisonStatus.Stable, comparison.Status);
            Assert.Equal(0, comparison.ElapsedPerOperationDeltaRatio);
        }

        [Fact]
        public void BenchmarkComparisonHandlesZeroBaselineAsRegressionWhenCurrentIsSlower()
        {
            var comparison = BrickBenchmarkComparison.Compare(Result("zero", 0), Result("zero", 1));

            Assert.Equal(BrickBenchmarkComparisonStatus.Regressed, comparison.Status);
            Assert.True(double.IsPositiveInfinity(comparison.ElapsedPerOperationDeltaRatio));
        }

        [Fact]
        public void BenchmarkComparisonRequiresCurrentResult()
        {
            Assert.Throws<ArgumentNullException>(() => BrickBenchmarkComparison.Compare(Result("baseline", 1), null));
        }

        [Fact]
        public void BenchmarkComparisonReportMatchesResultsByIdAndSummarizes()
        {
            var generatedAt = new DateTimeOffset(2026, 6, 24, 10, 0, 0, TimeSpan.Zero);
            var baselineReport = new BrickBenchmarkReport(
                generatedAt.AddDays(-1),
                new[] { Result("stable", 100), Result("regressed", 100), Result("old-only", 100) });
            var currentReport = new BrickBenchmarkReport(
                generatedAt,
                new[] { Result("new", 10), Result("regressed", 130), Result("stable", 103) });

            var report = BrickBenchmarkComparisonReport.Compare(
                baselineReport,
                currentReport,
                new BrickBenchmarkComparisonThreshold(0.10, 0.05));

            Assert.Equal(BrickBenchmarkComparisonReport.CurrentSchema, report.Schema);
            Assert.True(report.IsCurrentSchema);
            Assert.Equal(generatedAt, report.GeneratedAt);
            Assert.Equal(new[] { "new", "regressed", "stable" }, report.Comparisons.Select(comparison => comparison.Id).ToArray());
            Assert.Equal(3, report.Summary.Total);
            Assert.Equal(1, report.Summary.Stable);
            Assert.Equal(1, report.Summary.Regressed);
            Assert.Equal(1, report.Summary.NoBaseline);
            Assert.Equal(0, report.Summary.Improved);
            Assert.True(report.HasRegressions);
        }

        [Fact]
        public void BenchmarkComparisonReportNormalizesSchemaAndInputs()
        {
            var report = new BrickBenchmarkComparisonReport(
                DateTimeOffset.UnixEpoch,
                null,
                null);

            Assert.Equal(string.Empty, report.Schema);
            Assert.False(report.IsCurrentSchema);
            Assert.Empty(report.Comparisons);
            Assert.Equal(0, report.Summary.Total);
        }

        [Fact]
        public void BenchmarkComparisonReportRequiresCurrentReport()
        {
            Assert.Throws<ArgumentNullException>(() => BrickBenchmarkComparisonReport.Compare(null, null));
        }

        [Fact]
        public void BenchmarkComparisonReportTreatsMissingBaselineReportAsNoBaseline()
        {
            var currentReport = new BrickBenchmarkReport(DateTimeOffset.UnixEpoch, new[] { Result("first-run", 50) });

            var report = BrickBenchmarkComparisonReport.Compare(null, currentReport);

            Assert.Single(report.Comparisons);
            Assert.Equal(BrickBenchmarkComparisonStatus.NoBaseline, report.Comparisons[0].Status);
            Assert.Equal(1, report.Summary.NoBaseline);
            Assert.False(report.HasRegressions);
        }

        [Fact]
        public void BenchmarkComparisonReportJsonSerializerWritesSortedComparisonsAndRequiresReport()
        {
            var report = new BrickBenchmarkComparisonReport(
                DateTimeOffset.UnixEpoch,
                new[]
                {
                    BrickBenchmarkComparison.Compare(Result("zeta", 100), Result("zeta", 130)),
                    BrickBenchmarkComparison.Compare(null, Result("alpha", 50))
                });

            using var json = JsonDocument.Parse(BrickBenchmarkComparisonReportJsonSerializer.Serialize(report));
            var root = json.RootElement;
            var comparisons = root.GetProperty("comparisons").EnumerateArray().ToArray();

            Assert.Equal(BrickBenchmarkComparisonReport.CurrentSchema, root.GetProperty("schema").GetString());
            Assert.Equal(2, root.GetProperty("summary").GetProperty("total").GetInt32());
            Assert.Equal(1, root.GetProperty("summary").GetProperty("regressed").GetInt32());
            Assert.Equal(1, root.GetProperty("summary").GetProperty("noBaseline").GetInt32());
            Assert.True(root.GetProperty("hasRegressions").GetBoolean());
            Assert.Equal("alpha", comparisons[0].GetProperty("id").GetString());
            Assert.Equal("NoBaseline", comparisons[0].GetProperty("status").GetString());
            Assert.False(comparisons[0].TryGetProperty("baselineElapsedPerOperationTicks", out _));
            Assert.Equal("zeta", comparisons[1].GetProperty("id").GetString());
            Assert.Equal("Regressed", comparisons[1].GetProperty("status").GetString());
            Assert.Equal(100, comparisons[1].GetProperty("baselineElapsedPerOperationTicks").GetInt64());
            Assert.Equal(130, comparisons[1].GetProperty("currentElapsedPerOperationTicks").GetInt64());
            Assert.Equal(30, comparisons[1].GetProperty("elapsedPerOperationDeltaTicks").GetInt64());
            Assert.Throws<ArgumentNullException>(() => BrickBenchmarkComparisonReportJsonSerializer.Serialize(null));
        }

        private static BrickBenchmarkResult Result(string id, long elapsedPerOperationTicks)
        {
            var benchmarkCase = new BrickBenchmarkCase(
                id,
                id,
                BrickBenchmarkSubject.RuleEvaluation,
                1,
                new BrickBenchmarkBudget(TimeSpan.FromTicks(1000)));

            return new BrickBenchmarkResult(benchmarkCase, 1, 1, TimeSpan.FromTicks(elapsedPerOperationTicks));
        }
    }
}
