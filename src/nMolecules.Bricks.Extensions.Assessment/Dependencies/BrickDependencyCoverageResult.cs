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
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
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

        /// <summary>
        /// Gets the Target value used by Bricks developer tooling.
        /// </summary>
        public BrickDependencyCoverageTarget Target { get; }
        /// <summary>
        /// Gets the Analyzed Dependencies value used by Bricks developer tooling.
        /// </summary>
        public int AnalyzedDependencies { get; }
        /// <summary>
        /// Gets the Observable Dependencies value used by Bricks developer tooling.
        /// </summary>
        public int ObservableDependencies { get; }
        /// <summary>
        /// Gets the Unobservable Dependencies value used by Bricks developer tooling.
        /// </summary>
        public int UnobservableDependencies { get; }
        /// <summary>
        /// Gets the Coverage Ratio value used by Bricks developer tooling.
        /// </summary>
        public double CoverageRatio { get; }
        /// <summary>
        /// Gets the Observed Evidence Level value used by Bricks developer tooling.
        /// </summary>
        public BrickEvidenceLevel ObservedEvidenceLevel { get; }
        /// <summary>
        /// Gets the Notes value used by Bricks developer tooling.
        /// </summary>
        public string Notes { get; }
        /// <summary>
        /// Gets the Meets Evidence Requirement value used by Bricks developer tooling.
        /// </summary>
        public bool MeetsEvidenceRequirement { get; }
        /// <summary>
        /// Gets the Status value used by Bricks developer tooling.
        /// </summary>
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
