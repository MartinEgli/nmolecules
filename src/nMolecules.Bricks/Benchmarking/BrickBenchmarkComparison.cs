using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
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
}
