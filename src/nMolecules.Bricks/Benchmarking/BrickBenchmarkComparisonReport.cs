using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
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
}
