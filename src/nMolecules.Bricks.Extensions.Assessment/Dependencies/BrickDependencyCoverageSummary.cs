using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Summarizes dependency coverage summary results so callers can display the important outcome without
/// reading every detail.
/// </summary>
public sealed class BrickDependencyCoverageSummary
    {
        private BrickDependencyCoverageSummary(
            int total,
            int covered,
            int partiallyObservable,
            int notObservable,
            int insufficientEvidence,
            double averageCoverageRatio)
        {
            Total = total;
            Covered = covered;
            PartiallyObservable = partiallyObservable;
            NotObservable = notObservable;
            InsufficientEvidence = insufficientEvidence;
            AverageCoverageRatio = averageCoverageRatio;
        }

        /// <summary>
        /// Gets the Total value used by Bricks developer tooling.
        /// </summary>
        public int Total { get; }
        /// <summary>
        /// Gets the Covered value used by Bricks developer tooling.
        /// </summary>
        public int Covered { get; }
        /// <summary>
        /// Gets the Partially Observable value used by Bricks developer tooling.
        /// </summary>
        public int PartiallyObservable { get; }
        /// <summary>
        /// Gets the Not Observable value used by Bricks developer tooling.
        /// </summary>
        public int NotObservable { get; }
        /// <summary>
        /// Gets the Insufficient Evidence value used by Bricks developer tooling.
        /// </summary>
        public int InsufficientEvidence { get; }
        /// <summary>
        /// Gets the Average Coverage Ratio value used by Bricks developer tooling.
        /// </summary>
        public double AverageCoverageRatio { get; }

        /// <summary>
        /// Creates a Bricks configuration object from external key-value properties.
        /// </summary>
        public static BrickDependencyCoverageSummary FromResults(
            IEnumerable<BrickDependencyCoverageResult> results)
        {
            var items = (results ?? Enumerable.Empty<BrickDependencyCoverageResult>()).ToArray();
            return new BrickDependencyCoverageSummary(
                items.Length,
                items.Count(result => result.Status == BrickDependencyCoverageStatus.Covered),
                items.Count(result => result.Status == BrickDependencyCoverageStatus.PartiallyObservable),
                items.Count(result => result.Status == BrickDependencyCoverageStatus.NotObservable),
                items.Count(result => result.Status == BrickDependencyCoverageStatus.InsufficientEvidence),
                items.Length == 0 ? 0d : items.Average(result => result.CoverageRatio));
        }
    }
}
