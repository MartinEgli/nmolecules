using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
    public enum BrickBenchmarkSubject
    {
        RuleEvaluation = 0,
        RoleResolution = 1,
        PolicyComposition = 2,
        ViolationProjection = 3,
        RuntimeDependencyEvaluation = 4,
        ReportSerialization = 5
    }

    public enum BrickBenchmarkStatus
    {
        WithinBudget = 0,
        OverBudget = 1,
        NotBudgeted = 2
    }

    public sealed class BrickBenchmarkBudget
    {
        public BrickBenchmarkBudget(TimeSpan maxElapsedPerOperation, string rationale = null)
        {
            MaxElapsedPerOperation = maxElapsedPerOperation < TimeSpan.Zero ? TimeSpan.Zero : maxElapsedPerOperation;
            Rationale = rationale ?? string.Empty;
        }

        public TimeSpan MaxElapsedPerOperation { get; }
        public string Rationale { get; }
        public bool HasElapsedBudget => MaxElapsedPerOperation > TimeSpan.Zero;
    }

    public sealed class BrickBenchmarkCase
    {
        public BrickBenchmarkCase(
            string id,
            string displayName,
            BrickBenchmarkSubject subject,
            int operationCount,
            BrickBenchmarkBudget budget)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Subject = subject;
            OperationCount = operationCount <= 0 ? 1 : operationCount;
            Budget = budget;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public BrickBenchmarkSubject Subject { get; }
        public int OperationCount { get; }
        public BrickBenchmarkBudget Budget { get; }
    }

    public sealed class BrickBenchmarkResult
    {
        public BrickBenchmarkResult(
            BrickBenchmarkCase benchmarkCase,
            int iterations,
            long totalOperations,
            TimeSpan elapsed)
        {
            Case = benchmarkCase ?? throw new ArgumentNullException(nameof(benchmarkCase));
            Iterations = iterations <= 0 ? 1 : iterations;
            TotalOperations = totalOperations <= 0 ? 1 : totalOperations;
            Elapsed = elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
            ElapsedPerOperation = TimeSpan.FromTicks(Elapsed.Ticks / TotalOperations);
            OperationsPerSecond = Elapsed.TotalSeconds > 0
                ? TotalOperations / Elapsed.TotalSeconds
                : double.PositiveInfinity;
            Status = ResolveStatus(Case.Budget, ElapsedPerOperation);
            BudgetExceededBy = Status == BrickBenchmarkStatus.OverBudget
                ? ElapsedPerOperation - Case.Budget.MaxElapsedPerOperation
                : TimeSpan.Zero;
        }

        public BrickBenchmarkCase Case { get; }
        public int Iterations { get; }
        public long TotalOperations { get; }
        public TimeSpan Elapsed { get; }
        public TimeSpan ElapsedPerOperation { get; }
        public double OperationsPerSecond { get; }
        public BrickBenchmarkStatus Status { get; }
        public bool IsWithinBudget => Status == BrickBenchmarkStatus.WithinBudget;
        public TimeSpan BudgetExceededBy { get; }

        private static BrickBenchmarkStatus ResolveStatus(
            BrickBenchmarkBudget budget,
            TimeSpan elapsedPerOperation)
        {
            if (budget == null || !budget.HasElapsedBudget)
            {
                return BrickBenchmarkStatus.NotBudgeted;
            }

            return elapsedPerOperation <= budget.MaxElapsedPerOperation
                ? BrickBenchmarkStatus.WithinBudget
                : BrickBenchmarkStatus.OverBudget;
        }
    }

    public interface IBrickBenchmarkClock
    {
        TimeSpan Measure(Action operation);
    }

    public sealed class BrickStopwatchBenchmarkClock : IBrickBenchmarkClock
    {
        public TimeSpan Measure(Action operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            var stopwatch = Stopwatch.StartNew();
            operation();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }

    public static class BrickBenchmarkRunner
    {
        public static BrickBenchmarkResult Run(
            BrickBenchmarkCase benchmarkCase,
            Action operation,
            int iterations = 1,
            IBrickBenchmarkClock clock = null)
        {
            if (benchmarkCase == null)
            {
                throw new ArgumentNullException(nameof(benchmarkCase));
            }

            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (iterations <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations));
            }

            var activeClock = clock ?? new BrickStopwatchBenchmarkClock();
            var elapsed = TimeSpan.Zero;
            for (var i = 0; i < iterations; i++)
            {
                elapsed += activeClock.Measure(operation);
            }

            return new BrickBenchmarkResult(
                benchmarkCase,
                iterations,
                (long)benchmarkCase.OperationCount * iterations,
                elapsed);
        }
    }

    public static class BrickBuiltInBenchmarkCases
    {
        public static BrickBenchmarkCase RuleEvaluation => Case(
            "bricks.rule-evaluation",
            "Rule evaluation",
            BrickBenchmarkSubject.RuleEvaluation,
            1000,
            TimeSpan.FromTicks(2500));

        public static BrickBenchmarkCase RoleResolution => Case(
            "bricks.role-resolution",
            "Role resolution",
            BrickBenchmarkSubject.RoleResolution,
            1000,
            TimeSpan.FromTicks(3000));

        public static BrickBenchmarkCase PolicyComposition => Case(
            "bricks.policy-composition",
            "Policy composition",
            BrickBenchmarkSubject.PolicyComposition,
            100,
            TimeSpan.FromTicks(20000));

        public static BrickBenchmarkCase ViolationProjection => Case(
            "bricks.violation-projection",
            "Violation state projection",
            BrickBenchmarkSubject.ViolationProjection,
            1000,
            TimeSpan.FromTicks(5000));

        public static BrickBenchmarkCase RuntimeDependencyEvaluation => Case(
            "bricks.runtime-dependency-evaluation",
            "Runtime dependency evaluation",
            BrickBenchmarkSubject.RuntimeDependencyEvaluation,
            1000,
            TimeSpan.FromTicks(5000));

        public static BrickBenchmarkCase ReportSerialization => Case(
            "bricks.report-serialization",
            "Report serialization",
            BrickBenchmarkSubject.ReportSerialization,
            100,
            TimeSpan.FromTicks(50000));

        public static IReadOnlyList<BrickBenchmarkCase> All => new[]
        {
            RuleEvaluation,
            RoleResolution,
            PolicyComposition,
            ViolationProjection,
            RuntimeDependencyEvaluation,
            ReportSerialization
        };

        private static BrickBenchmarkCase Case(
            string id,
            string displayName,
            BrickBenchmarkSubject subject,
            int operationCount,
            TimeSpan budget) =>
            new BrickBenchmarkCase(
                id,
                displayName,
                subject,
                operationCount,
                new BrickBenchmarkBudget(budget, "Central Bricks performance budget."));
    }

    public sealed class BrickBenchmarkReport
    {
        public const string CurrentSchema = "NMolecules.Bricks.Benchmark/1.0";

        public BrickBenchmarkReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickBenchmarkResult> results)
            : this(generatedAt, results, CurrentSchema)
        {
        }

        public BrickBenchmarkReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickBenchmarkResult> results,
            string schema)
        {
            GeneratedAt = generatedAt;
            Results = (results ?? Enumerable.Empty<BrickBenchmarkResult>())
                .OrderBy(result => result.Case.Id, StringComparer.Ordinal)
                .ToArray();
            Schema = schema ?? string.Empty;
            Summary = BrickBenchmarkSummary.FromResults(Results);
        }

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickBenchmarkResult> Results { get; }
        public BrickBenchmarkSummary Summary { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }

    public sealed class BrickBenchmarkSummary
    {
        public BrickBenchmarkSummary(int total, int withinBudget, int overBudget, int notBudgeted)
        {
            Total = total;
            WithinBudget = withinBudget;
            OverBudget = overBudget;
            NotBudgeted = notBudgeted;
        }

        public int Total { get; }
        public int WithinBudget { get; }
        public int OverBudget { get; }
        public int NotBudgeted { get; }

        internal static BrickBenchmarkSummary FromResults(IReadOnlyList<BrickBenchmarkResult> results) =>
            new BrickBenchmarkSummary(
                results.Count,
                results.Count(result => result.Status == BrickBenchmarkStatus.WithinBudget),
                results.Count(result => result.Status == BrickBenchmarkStatus.OverBudget),
                results.Count(result => result.Status == BrickBenchmarkStatus.NotBudgeted));
    }

    public static class BrickBenchmarkReportJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        public static string Serialize(BrickBenchmarkReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            return JsonSerializer.Serialize(ToDto(report), Options);
        }

        private static ReportDto ToDto(BrickBenchmarkReport report) =>
            new ReportDto
            {
                Schema = report.Schema,
                GeneratedAt = report.GeneratedAt,
                Summary = new SummaryDto
                {
                    Total = report.Summary.Total,
                    WithinBudget = report.Summary.WithinBudget,
                    OverBudget = report.Summary.OverBudget,
                    NotBudgeted = report.Summary.NotBudgeted
                },
                Results = report.Results.Select(ToDto).ToArray()
            };

        private static ResultDto ToDto(BrickBenchmarkResult result) =>
            new ResultDto
            {
                Id = result.Case.Id,
                DisplayName = result.Case.DisplayName,
                Subject = result.Case.Subject.ToString(),
                Iterations = result.Iterations,
                TotalOperations = result.TotalOperations,
                ElapsedTicks = result.Elapsed.Ticks,
                ElapsedPerOperationTicks = result.ElapsedPerOperation.Ticks,
                OperationsPerSecond = result.OperationsPerSecond,
                Status = result.Status.ToString(),
                BudgetMaxElapsedPerOperationTicks = result.Case.Budget == null || !result.Case.Budget.HasElapsedBudget
                    ? (long?)null
                    : result.Case.Budget.MaxElapsedPerOperation.Ticks,
                BudgetExceededByTicks = result.BudgetExceededBy.Ticks,
                BudgetRationale = result.Case.Budget == null ? null : result.Case.Budget.Rationale
            };

        private sealed class ReportDto
        {
            public string Schema { get; set; }
            public DateTimeOffset GeneratedAt { get; set; }
            public SummaryDto Summary { get; set; }
            public ResultDto[] Results { get; set; }
        }

        private sealed class SummaryDto
        {
            public int Total { get; set; }
            public int WithinBudget { get; set; }
            public int OverBudget { get; set; }
            public int NotBudgeted { get; set; }
        }

        private sealed class ResultDto
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string Subject { get; set; }
            public int Iterations { get; set; }
            public long TotalOperations { get; set; }
            public long ElapsedTicks { get; set; }
            public long ElapsedPerOperationTicks { get; set; }
            public double OperationsPerSecond { get; set; }
            public string Status { get; set; }
            public long? BudgetMaxElapsedPerOperationTicks { get; set; }
            public long BudgetExceededByTicks { get; set; }
            public string BudgetRationale { get; set; }
        }
    }
}
