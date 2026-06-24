using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
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
