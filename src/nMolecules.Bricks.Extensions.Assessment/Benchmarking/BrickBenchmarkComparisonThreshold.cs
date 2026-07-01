using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
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
}
