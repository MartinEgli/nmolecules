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

        public BrickDependencyKindId KindId { get; }
        public BrickDependencyLayer Layer { get; }
        public BrickEvidenceLevel MinimumEvidenceLevel { get; }
        public bool Required { get; }
        public string Rationale { get; }

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
