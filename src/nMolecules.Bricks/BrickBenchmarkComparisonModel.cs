using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
    public enum BrickBenchmarkComparisonStatus
    {
        Stable = 0,
        Improved = 1,
        Regressed = 2,
        NoBaseline = 3
    }

    public sealed class BrickBenchmarkComparisonThreshold
    {
        public BrickBenchmarkComparisonThreshold(
            double maxAllowedSlowdownRatio = 0.10,
            double minSignificantImprovementRatio = 0.05,
            string rationale = null)
        {
            MaxAllowedSlowdownRatio = NormalizeRatio(maxAllowedSlowdownRatio);
            MinSignificantImprovementRatio = NormalizeRatio(minSignificantImprovementRatio);
            Rationale = rationale ?? string.Empty;
        }

        public double MaxAllowedSlowdownRatio { get; }
        public double MinSignificantImprovementRatio { get; }
        public string Rationale { get; }

        private static double NormalizeRatio(double ratio) =>
            double.IsNaN(ratio) || double.IsInfinity(ratio) || ratio < 0 ? 0 : ratio;
    }

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

        public string Id { get; }
        public string DisplayName { get; }
        public BrickBenchmarkSubject Subject { get; }
        public BrickBenchmarkResult Baseline { get; }
        public BrickBenchmarkResult Current { get; }
        public BrickBenchmarkComparisonThreshold Threshold { get; }
        public long ElapsedPerOperationDeltaTicks { get; }
        public double ElapsedPerOperationDeltaRatio { get; }
        public BrickBenchmarkComparisonStatus Status { get; }
        public bool IsRegression => Status == BrickBenchmarkComparisonStatus.Regressed;

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

    public sealed class BrickBenchmarkComparisonReport
    {
        public const string CurrentSchema = "NMolecules.Bricks.BenchmarkComparison/1.0";

        public BrickBenchmarkComparisonReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickBenchmarkComparison> comparisons)
            : this(generatedAt, comparisons, CurrentSchema)
        {
        }

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

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickBenchmarkComparison> Comparisons { get; }
        public BrickBenchmarkComparisonSummary Summary { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
        public bool HasRegressions => Summary.Regressed > 0;

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

    public sealed class BrickBenchmarkComparisonSummary
    {
        public BrickBenchmarkComparisonSummary(int total, int stable, int improved, int regressed, int noBaseline)
        {
            Total = total;
            Stable = stable;
            Improved = improved;
            Regressed = regressed;
            NoBaseline = noBaseline;
        }

        public int Total { get; }
        public int Stable { get; }
        public int Improved { get; }
        public int Regressed { get; }
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

    public static class BrickBenchmarkComparisonReportJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

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
