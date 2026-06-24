using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Describes how a current benchmark result compares with a previous baseline.
    /// </summary>
    public enum BrickBenchmarkComparisonStatus
    {
        /// <summary>The current result remains within the configured comparison threshold.</summary>
        Stable = 0,
        /// <summary>The current result is meaningfully faster than the baseline.</summary>
        Improved = 1,
        /// <summary>The current result is slower than the baseline by more than the allowed threshold.</summary>
        Regressed = 2,
        /// <summary>No matching baseline result exists for the current benchmark case.</summary>
        NoBaseline = 3
    }

    /// <summary>
    /// Defines slowdown and improvement ratios used when comparing benchmark runs.
    /// </summary>
    public sealed class BrickBenchmarkComparisonThreshold
    {
        /// <summary>
        /// Creates a benchmark comparison threshold.
        /// </summary>
        /// <param name="maxAllowedSlowdownRatio">Maximum tolerated slowdown ratio before a result is marked regressed.</param>
        /// <param name="minSignificantImprovementRatio">Minimum improvement ratio required before a result is marked improved.</param>
        /// <param name="rationale">Optional explanation for why the threshold was chosen.</param>
        public BrickBenchmarkComparisonThreshold(
            double maxAllowedSlowdownRatio = 0.10,
            double minSignificantImprovementRatio = 0.05,
            string rationale = null)
        {
            MaxAllowedSlowdownRatio = NormalizeRatio(maxAllowedSlowdownRatio);
            MinSignificantImprovementRatio = NormalizeRatio(minSignificantImprovementRatio);
            Rationale = rationale ?? string.Empty;
        }

        /// <summary>Maximum tolerated slowdown ratio before a comparison is marked regressed.</summary>
        public double MaxAllowedSlowdownRatio { get; }
        /// <summary>Minimum speedup ratio before a comparison is marked improved.</summary>
        public double MinSignificantImprovementRatio { get; }
        /// <summary>Explanation for the selected comparison threshold.</summary>
        public string Rationale { get; }

        private static double NormalizeRatio(double ratio) =>
            double.IsNaN(ratio) || double.IsInfinity(ratio) || ratio < 0 ? 0 : ratio;
    }

    /// <summary>
    /// Compares one current benchmark result with an optional baseline result.
    /// </summary>
    public sealed class BrickBenchmarkComparison
    {
        private BrickBenchmarkComparison(
            BrickBenchmarkResult baseline,
            BrickBenchmarkResult current,
            BrickBenchmarkComparisonThreshold threshold)
        {
            Baseline = baseline;
            Current = current ?? throw new ArgumentNullException(nameof(current));
            Threshold = threshold ?? new BrickBenchmarkComparisonThreshold();
            Id = Current.Case.Id;
            DisplayName = Current.Case.DisplayName;
            Subject = Current.Case.Subject;
            ElapsedPerOperationDeltaTicks = Baseline == null
                ? 0
                : Current.ElapsedPerOperation.Ticks - Baseline.ElapsedPerOperation.Ticks;
            ElapsedPerOperationDeltaRatio = ResolveDeltaRatio(Baseline, Current);
            Status = ResolveStatus(Baseline, ElapsedPerOperationDeltaRatio, Threshold);
        }

        /// <summary>Stable benchmark case identifier copied from the current result.</summary>
        public string Id { get; }
        /// <summary>Human-readable benchmark case name copied from the current result.</summary>
        public string DisplayName { get; }
        /// <summary>Central Bricks subject measured by the benchmark case.</summary>
        public BrickBenchmarkSubject Subject { get; }
        /// <summary>Previous benchmark result used as baseline, or <c>null</c> for first-run comparisons.</summary>
        public BrickBenchmarkResult Baseline { get; }
        /// <summary>Current benchmark result being evaluated.</summary>
        public BrickBenchmarkResult Current { get; }
        /// <summary>Threshold used to classify the comparison.</summary>
        public BrickBenchmarkComparisonThreshold Threshold { get; }
        /// <summary>Difference between current and baseline elapsed ticks per operation.</summary>
        public long ElapsedPerOperationDeltaTicks { get; }
        /// <summary>Relative elapsed-per-operation change compared to the baseline.</summary>
        public double ElapsedPerOperationDeltaRatio { get; }
        /// <summary>Classification of the current result against the baseline.</summary>
        public BrickBenchmarkComparisonStatus Status { get; }
        /// <summary>Indicates whether the current result is a performance regression.</summary>
        public bool IsRegression => Status == BrickBenchmarkComparisonStatus.Regressed;

        /// <summary>
        /// Compares a current benchmark result with an optional baseline using the supplied threshold.
        /// </summary>
        public static BrickBenchmarkComparison Compare(
            BrickBenchmarkResult baseline,
            BrickBenchmarkResult current,
            BrickBenchmarkComparisonThreshold threshold = null) =>
            new BrickBenchmarkComparison(baseline, current, threshold);

        private static double ResolveDeltaRatio(BrickBenchmarkResult baseline, BrickBenchmarkResult current)
        {
            if (baseline == null)
            {
                return 0;
            }

            if (baseline.ElapsedPerOperation.Ticks == 0)
            {
                return current.ElapsedPerOperation.Ticks == 0 ? 0 : double.PositiveInfinity;
            }

            return (double)(current.ElapsedPerOperation.Ticks - baseline.ElapsedPerOperation.Ticks)
                / baseline.ElapsedPerOperation.Ticks;
        }

        private static BrickBenchmarkComparisonStatus ResolveStatus(
            BrickBenchmarkResult baseline,
            double deltaRatio,
            BrickBenchmarkComparisonThreshold threshold)
        {
            if (baseline == null)
            {
                return BrickBenchmarkComparisonStatus.NoBaseline;
            }

            if (deltaRatio > threshold.MaxAllowedSlowdownRatio)
            {
                return BrickBenchmarkComparisonStatus.Regressed;
            }

            if (deltaRatio < -threshold.MinSignificantImprovementRatio)
            {
                return BrickBenchmarkComparisonStatus.Improved;
            }

            return BrickBenchmarkComparisonStatus.Stable;
        }
    }

    /// <summary>
    /// Versioned report containing benchmark comparisons for one benchmark run.
    /// </summary>
    public sealed class BrickBenchmarkComparisonReport
    {
        /// <summary>Current JSON schema identifier for benchmark comparison reports.</summary>
        public const string CurrentSchema = "NMolecules.Bricks.BenchmarkComparison/1.0";

        /// <summary>
        /// Creates a benchmark comparison report using the current schema.
        /// </summary>
        public BrickBenchmarkComparisonReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickBenchmarkComparison> comparisons)
            : this(generatedAt, comparisons, CurrentSchema)
        {
        }

        /// <summary>
        /// Creates a benchmark comparison report with an explicit schema identifier.
        /// </summary>
        public BrickBenchmarkComparisonReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickBenchmarkComparison> comparisons,
            string schema)
        {
            GeneratedAt = generatedAt;
            Schema = schema ?? string.Empty;
            Comparisons = (comparisons ?? Enumerable.Empty<BrickBenchmarkComparison>())
                .OrderBy(comparison => comparison.Id, StringComparer.Ordinal)
                .ToArray();
            Summary = BrickBenchmarkComparisonSummary.FromComparisons(Comparisons);
        }

        /// <summary>Schema identifier used to serialize the report.</summary>
        public string Schema { get; }
        /// <summary>Time the comparison report was generated.</summary>
        public DateTimeOffset GeneratedAt { get; }
        /// <summary>Comparisons sorted by benchmark case identifier.</summary>
        public IReadOnlyList<BrickBenchmarkComparison> Comparisons { get; }
        /// <summary>Aggregate counts for comparison statuses.</summary>
        public BrickBenchmarkComparisonSummary Summary { get; }
        /// <summary>Indicates whether the report uses <see cref="CurrentSchema"/>.</summary>
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
        /// <summary>Indicates whether any comparison is classified as regressed.</summary>
        public bool HasRegressions => Summary.Regressed > 0;

        /// <summary>
        /// Creates a comparison report by matching current and baseline benchmark results by case identifier.
        /// </summary>
        public static BrickBenchmarkComparisonReport Compare(
            BrickBenchmarkReport baselineReport,
            BrickBenchmarkReport currentReport,
            BrickBenchmarkComparisonThreshold threshold = null)
        {
            if (currentReport == null)
            {
                throw new ArgumentNullException(nameof(currentReport));
            }

            var baselineById = (baselineReport == null
                    ? Enumerable.Empty<BrickBenchmarkResult>()
                    : baselineReport.Results)
                .ToDictionary(result => result.Case.Id, StringComparer.Ordinal);
            var comparisons = currentReport.Results.Select(current =>
            {
                baselineById.TryGetValue(current.Case.Id, out var baseline);
                return BrickBenchmarkComparison.Compare(baseline, current, threshold);
            });

            return new BrickBenchmarkComparisonReport(currentReport.GeneratedAt, comparisons);
        }
    }

    /// <summary>
    /// Summarizes benchmark comparison statuses for a report.
    /// </summary>
    public sealed class BrickBenchmarkComparisonSummary
    {
        /// <summary>
        /// Creates a benchmark comparison summary.
        /// </summary>
        public BrickBenchmarkComparisonSummary(int total, int stable, int improved, int regressed, int noBaseline)
        {
            Total = total;
            Stable = stable;
            Improved = improved;
            Regressed = regressed;
            NoBaseline = noBaseline;
        }

        /// <summary>Total number of comparisons.</summary>
        public int Total { get; }
        /// <summary>Number of comparisons classified as stable.</summary>
        public int Stable { get; }
        /// <summary>Number of comparisons classified as improved.</summary>
        public int Improved { get; }
        /// <summary>Number of comparisons classified as regressed.</summary>
        public int Regressed { get; }
        /// <summary>Number of current results without a matching baseline.</summary>
        public int NoBaseline { get; }

        internal static BrickBenchmarkComparisonSummary FromComparisons(
            IReadOnlyList<BrickBenchmarkComparison> comparisons) =>
            new BrickBenchmarkComparisonSummary(
                comparisons.Count,
                comparisons.Count(comparison => comparison.Status == BrickBenchmarkComparisonStatus.Stable),
                comparisons.Count(comparison => comparison.Status == BrickBenchmarkComparisonStatus.Improved),
                comparisons.Count(comparison => comparison.Status == BrickBenchmarkComparisonStatus.Regressed),
                comparisons.Count(comparison => comparison.Status == BrickBenchmarkComparisonStatus.NoBaseline));
    }

    /// <summary>
    /// Serializes benchmark comparison reports to the versioned JSON schema.
    /// </summary>
    public static class BrickBenchmarkComparisonReportJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes a benchmark comparison report to compact camel-case JSON.
        /// </summary>
        public static string Serialize(BrickBenchmarkComparisonReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            return JsonSerializer.Serialize(ToDto(report), Options);
        }

        private static ReportDto ToDto(BrickBenchmarkComparisonReport report) =>
            new ReportDto
            {
                Schema = report.Schema,
                GeneratedAt = report.GeneratedAt,
                HasRegressions = report.HasRegressions,
                Summary = new SummaryDto
                {
                    Total = report.Summary.Total,
                    Stable = report.Summary.Stable,
                    Improved = report.Summary.Improved,
                    Regressed = report.Summary.Regressed,
                    NoBaseline = report.Summary.NoBaseline
                },
                Comparisons = report.Comparisons.Select(ToDto).ToArray()
            };

        private static ComparisonDto ToDto(BrickBenchmarkComparison comparison) =>
            new ComparisonDto
            {
                Id = comparison.Id,
                DisplayName = comparison.DisplayName,
                Subject = comparison.Subject.ToString(),
                Status = comparison.Status.ToString(),
                BaselineElapsedPerOperationTicks = comparison.Baseline == null
                    ? (long?)null
                    : comparison.Baseline.ElapsedPerOperation.Ticks,
                CurrentElapsedPerOperationTicks = comparison.Current.ElapsedPerOperation.Ticks,
                ElapsedPerOperationDeltaTicks = comparison.ElapsedPerOperationDeltaTicks,
                ElapsedPerOperationDeltaRatio = comparison.ElapsedPerOperationDeltaRatio,
                MaxAllowedSlowdownRatio = comparison.Threshold.MaxAllowedSlowdownRatio,
                MinSignificantImprovementRatio = comparison.Threshold.MinSignificantImprovementRatio,
                ThresholdRationale = comparison.Threshold.Rationale
            };

        private sealed class ReportDto
        {
            public string Schema { get; set; }
            public DateTimeOffset GeneratedAt { get; set; }
            public bool HasRegressions { get; set; }
            public SummaryDto Summary { get; set; }
            public ComparisonDto[] Comparisons { get; set; }
        }

        private sealed class SummaryDto
        {
            public int Total { get; set; }
            public int Stable { get; set; }
            public int Improved { get; set; }
            public int Regressed { get; set; }
            public int NoBaseline { get; set; }
        }

        private sealed class ComparisonDto
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string Subject { get; set; }
            public string Status { get; set; }
            public long? BaselineElapsedPerOperationTicks { get; set; }
            public long CurrentElapsedPerOperationTicks { get; set; }
            public long ElapsedPerOperationDeltaTicks { get; set; }
            public double ElapsedPerOperationDeltaRatio { get; set; }
            public double MaxAllowedSlowdownRatio { get; set; }
            public double MinSignificantImprovementRatio { get; set; }
            public string ThresholdRationale { get; set; }
        }
    }
}
