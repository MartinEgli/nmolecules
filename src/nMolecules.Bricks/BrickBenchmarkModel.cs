using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Identifies the central Bricks capability measured by a benchmark case.
    /// </summary>
    public enum BrickBenchmarkSubject
    {
        /// <summary>Measures deterministic rule evaluation.</summary>
        RuleEvaluation = 0,
        /// <summary>Measures role resolution and role assignment behavior.</summary>
        RoleResolution = 1,
        /// <summary>Measures policy composition and import handling.</summary>
        PolicyComposition = 2,
        /// <summary>Measures projection of violations into reportable state.</summary>
        ViolationProjection = 3,
        /// <summary>Measures runtime dependency evaluation surfaces.</summary>
        RuntimeDependencyEvaluation = 4,
        /// <summary>Measures report serialization cost.</summary>
        ReportSerialization = 5
    }

    /// <summary>
    /// Describes whether a benchmark result satisfies its configured budget.
    /// </summary>
    public enum BrickBenchmarkStatus
    {
        /// <summary>The measured elapsed time per operation is within the budget.</summary>
        WithinBudget = 0,
        /// <summary>The measured elapsed time per operation exceeds the budget.</summary>
        OverBudget = 1,
        /// <summary>No elapsed-time budget was configured for the benchmark case.</summary>
        NotBudgeted = 2
    }

    /// <summary>
    /// Defines the expected maximum elapsed time per operation for a benchmark case.
    /// </summary>
    public sealed class BrickBenchmarkBudget
    {
        /// <summary>
        /// Creates a benchmark budget.
        /// </summary>
        /// <param name="maxElapsedPerOperation">Maximum allowed elapsed time per measured operation.</param>
        /// <param name="rationale">Optional explanation for the budget.</param>
        public BrickBenchmarkBudget(TimeSpan maxElapsedPerOperation, string rationale = null)
        {
            MaxElapsedPerOperation = maxElapsedPerOperation < TimeSpan.Zero ? TimeSpan.Zero : maxElapsedPerOperation;
            Rationale = rationale ?? string.Empty;
        }

        /// <summary>Maximum allowed elapsed time per measured operation.</summary>
        public TimeSpan MaxElapsedPerOperation { get; }
        /// <summary>Explanation for why this budget exists.</summary>
        public string Rationale { get; }
        /// <summary>Indicates whether this budget contains a positive elapsed-time limit.</summary>
        public bool HasElapsedBudget => MaxElapsedPerOperation > TimeSpan.Zero;
    }

    /// <summary>
    /// Describes a deterministic benchmark case for a central Bricks capability.
    /// </summary>
    public sealed class BrickBenchmarkCase
    {
        /// <summary>
        /// Creates a benchmark case.
        /// </summary>
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

        /// <summary>Stable benchmark case identifier.</summary>
        public string Id { get; }
        /// <summary>Human-readable benchmark case name.</summary>
        public string DisplayName { get; }
        /// <summary>Central Bricks capability measured by the case.</summary>
        public BrickBenchmarkSubject Subject { get; }
        /// <summary>Number of logical operations performed by one invocation of the measured action.</summary>
        public int OperationCount { get; }
        /// <summary>Optional elapsed-time budget for the case.</summary>
        public BrickBenchmarkBudget Budget { get; }
    }

    /// <summary>
    /// Captures the measured result of one benchmark case.
    /// </summary>
    public sealed class BrickBenchmarkResult
    {
        /// <summary>
        /// Creates a benchmark result from elapsed time and operation counts.
        /// </summary>
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

        /// <summary>Benchmark case that produced the result.</summary>
        public BrickBenchmarkCase Case { get; }
        /// <summary>Number of times the measured action was executed.</summary>
        public int Iterations { get; }
        /// <summary>Total number of logical operations measured across all iterations.</summary>
        public long TotalOperations { get; }
        /// <summary>Total elapsed time across all iterations.</summary>
        public TimeSpan Elapsed { get; }
        /// <summary>Average elapsed time per logical operation.</summary>
        public TimeSpan ElapsedPerOperation { get; }
        /// <summary>Calculated throughput in logical operations per second.</summary>
        public double OperationsPerSecond { get; }
        /// <summary>Budget status calculated from elapsed time per operation.</summary>
        public BrickBenchmarkStatus Status { get; }
        /// <summary>Indicates whether the result is within its configured budget.</summary>
        public bool IsWithinBudget => Status == BrickBenchmarkStatus.WithinBudget;
        /// <summary>Amount by which the elapsed time per operation exceeds the budget, or zero.</summary>
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

    /// <summary>
    /// Measures the elapsed time of a benchmark operation.
    /// </summary>
    public interface IBrickBenchmarkClock
    {
        /// <summary>
        /// Measures the elapsed time required to execute <paramref name="operation"/>.
        /// </summary>
        TimeSpan Measure(Action operation);
    }

    /// <summary>
    /// Benchmark clock backed by <see cref="Stopwatch"/>.
    /// </summary>
    public sealed class BrickStopwatchBenchmarkClock : IBrickBenchmarkClock
    {
        /// <inheritdoc />
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

    /// <summary>
    /// Executes benchmark cases against measured operations.
    /// </summary>
    public static class BrickBenchmarkRunner
    {
        /// <summary>
        /// Runs a benchmark case for the requested number of iterations.
        /// </summary>
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

    /// <summary>
    /// Provides stable benchmark cases for central Bricks capabilities.
    /// </summary>
    public static class BrickBuiltInBenchmarkCases
    {
        /// <summary>Built-in benchmark case for rule evaluation.</summary>
        public static BrickBenchmarkCase RuleEvaluation => Case(
            "bricks.rule-evaluation",
            "Rule evaluation",
            BrickBenchmarkSubject.RuleEvaluation,
            1000,
            TimeSpan.FromTicks(2500));

        /// <summary>Built-in benchmark case for role resolution.</summary>
        public static BrickBenchmarkCase RoleResolution => Case(
            "bricks.role-resolution",
            "Role resolution",
            BrickBenchmarkSubject.RoleResolution,
            1000,
            TimeSpan.FromTicks(3000));

        /// <summary>Built-in benchmark case for policy composition.</summary>
        public static BrickBenchmarkCase PolicyComposition => Case(
            "bricks.policy-composition",
            "Policy composition",
            BrickBenchmarkSubject.PolicyComposition,
            100,
            TimeSpan.FromTicks(20000));

        /// <summary>Built-in benchmark case for violation projection.</summary>
        public static BrickBenchmarkCase ViolationProjection => Case(
            "bricks.violation-projection",
            "Violation state projection",
            BrickBenchmarkSubject.ViolationProjection,
            1000,
            TimeSpan.FromTicks(5000));

        /// <summary>Built-in benchmark case for runtime dependency evaluation.</summary>
        public static BrickBenchmarkCase RuntimeDependencyEvaluation => Case(
            "bricks.runtime-dependency-evaluation",
            "Runtime dependency evaluation",
            BrickBenchmarkSubject.RuntimeDependencyEvaluation,
            1000,
            TimeSpan.FromTicks(5000));

        /// <summary>Built-in benchmark case for report serialization.</summary>
        public static BrickBenchmarkCase ReportSerialization => Case(
            "bricks.report-serialization",
            "Report serialization",
            BrickBenchmarkSubject.ReportSerialization,
            100,
            TimeSpan.FromTicks(50000));

        /// <summary>All built-in benchmark cases in stable output order.</summary>
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

    /// <summary>
    /// Versioned benchmark report for central Bricks benchmark results.
    /// </summary>
    public sealed class BrickBenchmarkReport
    {
        /// <summary>Current JSON schema identifier for benchmark reports.</summary>
        public const string CurrentSchema = "NMolecules.Bricks.Benchmark/1.0";

        /// <summary>
        /// Creates a benchmark report using the current schema.
        /// </summary>
        public BrickBenchmarkReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickBenchmarkResult> results)
            : this(generatedAt, results, CurrentSchema)
        {
        }

        /// <summary>
        /// Creates a benchmark report with an explicit schema identifier.
        /// </summary>
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

        /// <summary>Schema identifier used to serialize the report.</summary>
        public string Schema { get; }
        /// <summary>Time the report was generated.</summary>
        public DateTimeOffset GeneratedAt { get; }
        /// <summary>Benchmark results sorted by case identifier.</summary>
        public IReadOnlyList<BrickBenchmarkResult> Results { get; }
        /// <summary>Aggregate status counts for the report.</summary>
        public BrickBenchmarkSummary Summary { get; }
        /// <summary>Indicates whether the report uses <see cref="CurrentSchema"/>.</summary>
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }

    /// <summary>
    /// Summarizes budget status counts for a benchmark report.
    /// </summary>
    public sealed class BrickBenchmarkSummary
    {
        /// <summary>
        /// Creates a benchmark summary.
        /// </summary>
        public BrickBenchmarkSummary(int total, int withinBudget, int overBudget, int notBudgeted)
        {
            Total = total;
            WithinBudget = withinBudget;
            OverBudget = overBudget;
            NotBudgeted = notBudgeted;
        }

        /// <summary>Total number of benchmark results.</summary>
        public int Total { get; }
        /// <summary>Number of results within budget.</summary>
        public int WithinBudget { get; }
        /// <summary>Number of results over budget.</summary>
        public int OverBudget { get; }
        /// <summary>Number of results without an elapsed-time budget.</summary>
        public int NotBudgeted { get; }

        internal static BrickBenchmarkSummary FromResults(IReadOnlyList<BrickBenchmarkResult> results) =>
            new BrickBenchmarkSummary(
                results.Count,
                results.Count(result => result.Status == BrickBenchmarkStatus.WithinBudget),
                results.Count(result => result.Status == BrickBenchmarkStatus.OverBudget),
                results.Count(result => result.Status == BrickBenchmarkStatus.NotBudgeted));
    }

    /// <summary>
    /// Serializes benchmark reports to the versioned JSON schema.
    /// </summary>
    public static class BrickBenchmarkReportJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes a benchmark report to compact camel-case JSON.
        /// </summary>
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
