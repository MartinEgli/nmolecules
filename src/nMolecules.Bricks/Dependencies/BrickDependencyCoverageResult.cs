using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents the result of dependency coverage result processing in the Bricks pipeline.
/// </summary>
public sealed class BrickDependencyCoverageResult
    {
        public BrickDependencyCoverageResult(
            BrickDependencyCoverageTarget target,
            int analyzedDependencies,
            int observableDependencies,
            BrickEvidenceLevel observedEvidenceLevel,
            string notes = null)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            AnalyzedDependencies = Math.Max(0, analyzedDependencies);
            ObservableDependencies = Math.Min(AnalyzedDependencies, Math.Max(0, observableDependencies));
            UnobservableDependencies = Math.Max(0, AnalyzedDependencies - ObservableDependencies);
            CoverageRatio = AnalyzedDependencies == 0
                ? 0d
                : (double)ObservableDependencies / AnalyzedDependencies;
            ObservedEvidenceLevel = observedEvidenceLevel;
            Notes = notes ?? string.Empty;
            MeetsEvidenceRequirement = Target.IsSatisfiedBy(ObservedEvidenceLevel);
            Status = ResolveStatus();
        }

        public BrickDependencyCoverageTarget Target { get; }
        public int AnalyzedDependencies { get; }
        public int ObservableDependencies { get; }
        public int UnobservableDependencies { get; }
        public double CoverageRatio { get; }
        public BrickEvidenceLevel ObservedEvidenceLevel { get; }
        public string Notes { get; }
        public bool MeetsEvidenceRequirement { get; }
        public BrickDependencyCoverageStatus Status { get; }

        private BrickDependencyCoverageStatus ResolveStatus()
        {
            if (AnalyzedDependencies == 0 || ObservableDependencies == 0)
            {
                return BrickDependencyCoverageStatus.NotObservable;
            }

            if (!MeetsEvidenceRequirement)
            {
                return BrickDependencyCoverageStatus.InsufficientEvidence;
            }

            return ObservableDependencies >= AnalyzedDependencies
                ? BrickDependencyCoverageStatus.Covered
                : BrickDependencyCoverageStatus.PartiallyObservable;
        }
    }
}
