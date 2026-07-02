using System;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksBenchmarkModelTest
    {
        [Fact]
        public void BenchmarkCaseRecordsCentralElementAndBudget()
        {
            var budget = new BrickBenchmarkBudget(TimeSpan.FromMilliseconds(2), "IDE feedback budget.");
            var benchmarkCase = new BrickBenchmarkCase(
                "role-resolution",
                "Role Resolution",
                BrickBenchmarkSubject.RoleResolution,
                10,
                budget);

            Assert.Equal("role-resolution", benchmarkCase.Id);
            Assert.Equal("Role Resolution", benchmarkCase.DisplayName);
            Assert.Equal(BrickBenchmarkSubject.RoleResolution, benchmarkCase.Subject);
            Assert.Equal(10, benchmarkCase.OperationCount);
            Assert.Equal(budget, benchmarkCase.Budget);
        }

        [Fact]
        public void BenchmarkCaseNormalizesTextAndOperationCount()
        {
            var benchmarkCase = new BrickBenchmarkCase(null, null, BrickBenchmarkSubject.PolicyComposition, 0, null);

            Assert.Equal(string.Empty, benchmarkCase.Id);
            Assert.Equal(string.Empty, benchmarkCase.DisplayName);
            Assert.Equal(1, benchmarkCase.OperationCount);
            Assert.Null(benchmarkCase.Budget);
        }

        [Fact]
        public void BenchmarkBudgetRecordsElapsedLimitAndRationale()
        {
            var budget = new BrickBenchmarkBudget(TimeSpan.FromTicks(500), "Keep analyzer feedback fast.");

            Assert.Equal(TimeSpan.FromTicks(500), budget.MaxElapsedPerOperation);
            Assert.Equal("Keep analyzer feedback fast.", budget.Rationale);
            Assert.True(budget.HasElapsedBudget);
        }

        [Fact]
        public void BenchmarkBudgetNormalizesValues()
        {
            var budget = new BrickBenchmarkBudget(TimeSpan.FromTicks(-1), null);

            Assert.Equal(TimeSpan.Zero, budget.MaxElapsedPerOperation);
            Assert.Equal(string.Empty, budget.Rationale);
            Assert.False(budget.HasElapsedBudget);
        }

        [Fact]
        public void BenchmarkRunnerMeasuresOperationAndMarksWithinBudget()
        {
            var benchmarkCase = new BrickBenchmarkCase(
                "rule-evaluation",
                "Rule Evaluation",
                BrickBenchmarkSubject.RuleEvaluation,
                2,
                new BrickBenchmarkBudget(TimeSpan.FromTicks(10)));
            var clock = new FixedBenchmarkClock(TimeSpan.FromTicks(6));
            var calls = 0;

            var result = BrickBenchmarkRunner.Run(benchmarkCase, () => calls++, 3, clock);

            Assert.Equal(3, calls);
            Assert.Equal(3, clock.MeasureCalls);
            Assert.Equal(benchmarkCase, result.Case);
            Assert.Equal(3, result.Iterations);
            Assert.Equal(6, result.TotalOperations);
            Assert.Equal(TimeSpan.FromTicks(18), result.Elapsed);
            Assert.Equal(TimeSpan.FromTicks(3), result.ElapsedPerOperation);
            Assert.Equal(BrickBenchmarkStatus.WithinBudget, result.Status);
            Assert.True(result.IsWithinBudget);
            Assert.Equal(TimeSpan.Zero, result.BudgetExceededBy);
            Assert.True(result.OperationsPerSecond > 0);
        }

        [Fact]
        public void BenchmarkRunnerMarksOverBudget()
        {
            var benchmarkCase = new BrickBenchmarkCase(
                "serialization",
                "Report Serialization",
                BrickBenchmarkSubject.ReportSerialization,
                1,
                new BrickBenchmarkBudget(TimeSpan.FromTicks(2)));

            var result = BrickBenchmarkRunner.Run(benchmarkCase, () => { }, 2, new FixedBenchmarkClock(TimeSpan.FromTicks(5)));

            Assert.Equal(TimeSpan.FromTicks(5), result.ElapsedPerOperation);
            Assert.Equal(BrickBenchmarkStatus.OverBudget, result.Status);
            Assert.False(result.IsWithinBudget);
            Assert.Equal(TimeSpan.FromTicks(3), result.BudgetExceededBy);
        }

        [Fact]
        public void BenchmarkRunnerMarksMissingBudget()
        {
            var benchmarkCase = new BrickBenchmarkCase(
                "runtime-dependencies",
                "Runtime Dependencies",
                BrickBenchmarkSubject.RuntimeDependencyEvaluation,
                4,
                null);

            var result = BrickBenchmarkRunner.Run(benchmarkCase, () => { }, 2, new FixedBenchmarkClock(TimeSpan.FromTicks(8)));

            Assert.Equal(BrickBenchmarkStatus.NotBudgeted, result.Status);
            Assert.False(result.IsWithinBudget);
            Assert.Equal(TimeSpan.Zero, result.BudgetExceededBy);
        }

        [Fact]
        public void BenchmarkResultNormalizesConstructorValuesAndZeroElapsed()
        {
            var benchmarkCase = new BrickBenchmarkCase(
                "no-budget",
                "No Budget",
                BrickBenchmarkSubject.RuleEvaluation,
                1,
                new BrickBenchmarkBudget(TimeSpan.Zero));

            var result = new BrickBenchmarkResult(benchmarkCase, 0, 0, TimeSpan.FromTicks(-1));

            Assert.Equal(1, result.Iterations);
            Assert.Equal(1, result.TotalOperations);
            Assert.Equal(TimeSpan.Zero, result.Elapsed);
            Assert.Equal(TimeSpan.Zero, result.ElapsedPerOperation);
            Assert.Equal(double.PositiveInfinity, result.OperationsPerSecond);
            Assert.Equal(BrickBenchmarkStatus.NotBudgeted, result.Status);
        }

        [Fact]
        public void BenchmarkResultRequiresCase()
        {
            Assert.Throws<ArgumentNullException>(() => new BrickBenchmarkResult(null, 1, 1, TimeSpan.Zero));
        }

        [Fact]
        public void BenchmarkRunnerUsesStopwatchClockByDefault()
        {
            var benchmarkCase = new BrickBenchmarkCase("default-clock", "Default Clock", BrickBenchmarkSubject.RuleEvaluation, 1, null);
            var called = false;

            var result = BrickBenchmarkRunner.Run(benchmarkCase, () => called = true);

            Assert.True(called);
            Assert.Equal(1, result.Iterations);
            Assert.Equal(1, result.TotalOperations);
        }

        [Fact]
        public void BenchmarkRunnerValidatesInputs()
        {
            var benchmarkCase = new BrickBenchmarkCase("id", "Name", BrickBenchmarkSubject.RuleEvaluation, 1, null);

            Assert.Throws<ArgumentNullException>(() => BrickBenchmarkRunner.Run(null, () => { }));
            Assert.Throws<ArgumentNullException>(() => BrickBenchmarkRunner.Run(benchmarkCase, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => BrickBenchmarkRunner.Run(benchmarkCase, () => { }, 0));
        }

        [Fact]
        public void StopwatchBenchmarkClockMeasuresOperationAndRequiresOperation()
        {
            var clock = new BrickStopwatchBenchmarkClock();
            var called = false;

            var elapsed = clock.Measure(() => called = true);

            Assert.True(called);
            Assert.True(elapsed >= TimeSpan.Zero);
            Assert.Throws<ArgumentNullException>(() => clock.Measure(null));
        }

        [Fact]
        public void BuiltInBenchmarkCasesCoverCentralElementsInStableOrder()
        {
            Assert.Equal(new[]
            {
                BrickBenchmarkSubject.RuleEvaluation,
                BrickBenchmarkSubject.RoleResolution,
                BrickBenchmarkSubject.PolicyComposition,
                BrickBenchmarkSubject.ViolationProjection,
                BrickBenchmarkSubject.RuntimeDependencyEvaluation,
                BrickBenchmarkSubject.ReportSerialization
            }, BrickBuiltInBenchmarkCases.All.Select(benchmarkCase => benchmarkCase.Subject).ToArray());

            Assert.All(BrickBuiltInBenchmarkCases.All, benchmarkCase => Assert.True(benchmarkCase.Budget.HasElapsedBudget));
        }

        [Fact]
        public void BenchmarkReportSummarizesResults()
        {
            var now = new DateTimeOffset(2026, 6, 23, 20, 0, 0, TimeSpan.Zero);
            var within = Result("within", BrickBenchmarkStatus.WithinBudget);
            var over = Result("over", BrickBenchmarkStatus.OverBudget);
            var notBudgeted = Result("none", BrickBenchmarkStatus.NotBudgeted);
            var results = new[] { over, within, notBudgeted };

            var report = new BrickBenchmarkReport(now, results);
            results[0] = Result("mutated", BrickBenchmarkStatus.WithinBudget);

            Assert.Equal(BrickBenchmarkReport.CurrentSchema, report.Schema);
            Assert.True(report.IsCurrentSchema);
            Assert.Equal(now, report.GeneratedAt);
            Assert.Equal(new[] { notBudgeted, over, within }, report.Results.ToArray());
            Assert.Equal(3, report.Summary.Total);
            Assert.Equal(1, report.Summary.WithinBudget);
            Assert.Equal(1, report.Summary.OverBudget);
            Assert.Equal(1, report.Summary.NotBudgeted);
        }

        [Fact]
        public void BenchmarkReportNormalizesSchemaAndResults()
        {
            var report = new BrickBenchmarkReport(DateTimeOffset.UnixEpoch, null, null);

            Assert.Equal(string.Empty, report.Schema);
            Assert.False(report.IsCurrentSchema);
            Assert.Empty(report.Results);
            Assert.Equal(0, report.Summary.Total);
        }

        [Fact]
        public void BenchmarkReportJsonSerializerWritesSortedResultsAndRequiresReport()
        {
            var report = new BrickBenchmarkReport(
                DateTimeOffset.UnixEpoch,
                new[] { Result("zeta", BrickBenchmarkStatus.OverBudget), Result("alpha", BrickBenchmarkStatus.WithinBudget), Result("none", BrickBenchmarkStatus.NotBudgeted) });

            using var json = JsonDocument.Parse(BrickBenchmarkReportJsonSerializer.Serialize(report));
            var root = json.RootElement;
            var results = root.GetProperty("results").EnumerateArray().ToArray();

            Assert.Equal(BrickBenchmarkReport.CurrentSchema, root.GetProperty("schema").GetString());
            Assert.Equal(3, root.GetProperty("summary").GetProperty("total").GetInt32());
            Assert.Equal(1, root.GetProperty("summary").GetProperty("withinBudget").GetInt32());
            Assert.Equal(1, root.GetProperty("summary").GetProperty("overBudget").GetInt32());
            Assert.Equal(1, root.GetProperty("summary").GetProperty("notBudgeted").GetInt32());
            Assert.Equal("alpha", results[0].GetProperty("id").GetString());
            Assert.Equal("none", results[1].GetProperty("id").GetString());
            Assert.Equal("zeta", results[2].GetProperty("id").GetString());
            Assert.Equal("RuleEvaluation", results[0].GetProperty("subject").GetString());
            Assert.Equal("WithinBudget", results[0].GetProperty("status").GetString());
            Assert.True(results[0].GetProperty("operationsPerSecond").GetDouble() > 0);
            Assert.Equal("central budget", results[0].GetProperty("budgetRationale").GetString());
            Assert.False(results[1].TryGetProperty("budgetMaxElapsedPerOperationTicks", out _));
            Assert.False(results[1].TryGetProperty("budgetRationale", out _));
            Assert.Throws<ArgumentNullException>(() => BrickBenchmarkReportJsonSerializer.Serialize(null));
        }

        private static BrickBenchmarkResult Result(string id, BrickBenchmarkStatus status)
        {
            var budget = status == BrickBenchmarkStatus.NotBudgeted
                ? null
                : new BrickBenchmarkBudget(TimeSpan.FromTicks(status == BrickBenchmarkStatus.OverBudget ? 1 : 10), "central budget");
            var benchmarkCase = new BrickBenchmarkCase(id, id, BrickBenchmarkSubject.RuleEvaluation, 1, budget);
            var elapsed = status == BrickBenchmarkStatus.OverBudget ? TimeSpan.FromTicks(5) : TimeSpan.FromTicks(5);

            return new BrickBenchmarkResult(benchmarkCase, 1, 1, elapsed);
        }

        private sealed class FixedBenchmarkClock : IBrickBenchmarkClock
        {
            private readonly TimeSpan _elapsed;

            public FixedBenchmarkClock(TimeSpan elapsed)
            {
                _elapsed = elapsed;
            }

            public int MeasureCalls { get; private set; }

            public TimeSpan Measure(Action operation)
            {
                MeasureCalls++;
                operation();
                return _elapsed;
            }
        }
    }
}
