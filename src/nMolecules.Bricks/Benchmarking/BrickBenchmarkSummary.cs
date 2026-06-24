using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
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
}
