using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents dependency coverage target data used by dependency coverage targets and coverage reporting.
/// </summary>
public sealed class BrickDependencyCoverageTarget
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickDependencyCoverageTarget(
            BrickDependencyKindId kindId,
            BrickDependencyLayer layer,
            BrickEvidenceLevel minimumEvidenceLevel,
            bool required,
            string rationale = null)
        {
            KindId = kindId;
            Layer = layer;
            MinimumEvidenceLevel = minimumEvidenceLevel;
            Required = required;
            Rationale = rationale ?? string.Empty;
        }

        /// <summary>
        /// Gets the Kind Id value used by Bricks developer tooling.
        /// </summary>
        public BrickDependencyKindId KindId { get; }
        /// <summary>
        /// Gets the Layer value used by Bricks developer tooling.
        /// </summary>
        public BrickDependencyLayer Layer { get; }
        /// <summary>
        /// Gets the Minimum Evidence Level value used by Bricks developer tooling.
        /// </summary>
        public BrickEvidenceLevel MinimumEvidenceLevel { get; }
        /// <summary>
        /// Gets a value indicating whether Required applies.
        /// </summary>
        public bool Required { get; }
        /// <summary>
        /// Gets the Rationale value used by Bricks developer tooling.
        /// </summary>
        public string Rationale { get; }

        /// <summary>
        /// Determines whether this Bricks model object satisfies the requested condition.
        /// </summary>
        public bool IsSatisfiedBy(BrickEvidenceLevel observedEvidenceLevel) =>
            EvidenceRank(observedEvidenceLevel) <= EvidenceRank(MinimumEvidenceLevel);

        private static int EvidenceRank(BrickEvidenceLevel evidenceLevel)
        {
            switch (evidenceLevel)
            {
                case BrickEvidenceLevel.CompilerConfirmed:
                    return 0;
                case BrickEvidenceLevel.AnalyzerInferred:
                    return 1;
                case BrickEvidenceLevel.ConfigurationDeclared:
                    return 2;
                case BrickEvidenceLevel.RuntimeInferred:
                    return 3;
                default:
                    return 4;
            }
        }
    }
}
