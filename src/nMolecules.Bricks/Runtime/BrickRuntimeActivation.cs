using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickRuntimeActivation
    {
        public const string DependencyKind = "RuntimeActivation";

        public BrickRuntimeActivation(
            BrickElement activationSite,
            BrickElement activatedType,
            string activationPattern,
            string justification = null,
            BrickEvidenceLevel evidenceLevel = BrickEvidenceLevel.RuntimeInferred)
        {
            ActivationSite = activationSite ?? throw new ArgumentNullException(nameof(activationSite));
            ActivatedType = activatedType ?? throw new ArgumentNullException(nameof(activatedType));
            ActivationPattern = activationPattern ?? string.Empty;
            Justification = justification;
            EvidenceLevel = evidenceLevel;
        }

        public BrickElement ActivationSite { get; }
        public BrickElement ActivatedType { get; }
        public string ActivationPattern { get; }
        public string Justification { get; }
        public BrickEvidenceLevel EvidenceLevel { get; }

        public BrickDependency ToDependency() =>
            new BrickDependency(
                ActivationSite,
                ActivatedType,
                BrickDependencyKindId.From(DependencyKind),
                BrickScope.Type,
                BrickDependencyLayer.Runtime,
                BrickDependencyStrength.Inferred,
                EvidenceLevel,
                detail: ActivationPattern);
    }
}
