using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents reflection access data used by reflection-based architecture assessment.
/// </summary>
public sealed class BrickReflectionAccess
    {
        /// <summary>
        /// Gets the Dependency Kind value used by Bricks developer tooling.
        /// </summary>
        public const string DependencyKind = "ReflectionAccess";

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickReflectionAccess(
            BrickElement accessSite,
            BrickElement target,
            string accessPattern,
            BrickReflectionConfidence confidence = BrickReflectionConfidence.Low,
            string justification = null,
            BrickEvidenceLevel evidenceLevel = BrickEvidenceLevel.RuntimeInferred)
        {
            AccessSite = accessSite ?? throw new ArgumentNullException(nameof(accessSite));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            AccessPattern = accessPattern ?? string.Empty;
            Confidence = confidence;
            Justification = justification;
            EvidenceLevel = evidenceLevel;
        }

        /// <summary>
        /// Gets the Access Site value used by Bricks developer tooling.
        /// </summary>
        public BrickElement AccessSite { get; }
        /// <summary>
        /// Gets the Target value used by Bricks developer tooling.
        /// </summary>
        public BrickElement Target { get; }
        /// <summary>
        /// Gets the Access Pattern value used by Bricks developer tooling.
        /// </summary>
        public string AccessPattern { get; }
        /// <summary>
        /// Gets the Confidence value used by Bricks developer tooling.
        /// </summary>
        public BrickReflectionConfidence Confidence { get; }
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
                AccessSite,
                Target,
                BrickDependencyKindId.From(DependencyKind),
                BrickScope.Type,
                BrickDependencyLayer.Runtime,
                BrickDependencyStrength.Inferred,
                EvidenceLevel,
                detail: AccessPattern);
    }
}
