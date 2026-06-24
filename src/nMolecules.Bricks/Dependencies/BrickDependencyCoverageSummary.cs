using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
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

        public int Total { get; }
        public int Covered { get; }
        public int PartiallyObservable { get; }
        public int NotObservable { get; }
        public int InsufficientEvidence { get; }
        public double AverageCoverageRatio { get; }

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
