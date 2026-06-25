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
        public const string DependencyKind = "ReflectionAccess";

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

        public BrickElement AccessSite { get; }
        public BrickElement Target { get; }
        public string AccessPattern { get; }
        public BrickReflectionConfidence Confidence { get; }
        public string Justification { get; }
        public BrickEvidenceLevel EvidenceLevel { get; }

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
