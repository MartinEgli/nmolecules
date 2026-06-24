using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
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
}
