using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents runtime activation data used by runtime dependency registration, activation, and wiring
/// checks.
/// </summary>
public sealed class BrickRuntimeActivation
    {
        /// <summary>
        /// Gets the Dependency Kind value used by Bricks developer tooling.
        /// </summary>
        public const string DependencyKind = "RuntimeActivation";

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
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

        /// <summary>
        /// Gets the Activation Site value used by Bricks developer tooling.
        /// </summary>
        public BrickElement ActivationSite { get; }
        /// <summary>
        /// Gets the Activated Type value used by Bricks developer tooling.
        /// </summary>
        public BrickElement ActivatedType { get; }
        /// <summary>
        /// Gets the Activation Pattern value used by Bricks developer tooling.
        /// </summary>
        public string ActivationPattern { get; }
        /// <summary>
        /// Gets the Justification value used by Bricks developer tooling.
        /// </summary>
        public string Justification { get; }
        /// <summary>
        /// Gets the Evidence Level value used by Bricks developer tooling.
        /// </summary>
        public BrickEvidenceLevel EvidenceLevel { get; }

        /// <summary>
        /// Converts this Bricks model object to the corresponding core representation.
        /// </summary>
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
