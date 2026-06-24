using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickDependency
    {
        public BrickDependency(
            BrickElement source,
            BrickElement target,
            BrickDependencyKindId kindId,
            BrickScope scope,
            BrickDependencyLayer layer,
            BrickDependencyStrength strength,
            BrickEvidenceLevel evidenceLevel,
            BrickSourceLocation? location = null,
            string detail = null)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            KindId = kindId;
            Scope = scope;
            Layer = layer;
            Strength = strength;
            EvidenceLevel = evidenceLevel;
            Location = location;
            Detail = detail;
        }

        public BrickElement Source { get; }
        public BrickElement Target { get; }
        public BrickDependencyKindId KindId { get; }
        public BrickScope Scope { get; }
        public BrickDependencyLayer Layer { get; }
        public BrickDependencyStrength Strength { get; }
        public BrickEvidenceLevel EvidenceLevel { get; }
        public BrickSourceLocation? Location { get; }
        public string Detail { get; }
    }
}
