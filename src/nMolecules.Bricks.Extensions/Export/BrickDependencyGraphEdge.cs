using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents dependency graph edge data used by exportable role maps, dependency graphs, and resolution
/// traces.
/// </summary>
public sealed class BrickDependencyGraphEdge
    {
        public BrickDependencyGraphEdge(
            BrickElement source,
            BrickElement target,
            BrickDependencyKindId kindId,
            BrickDependencyLayer layer,
            BrickDependencyStrength strength,
            BrickEvidenceLevel evidenceLevel)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            KindId = kindId;
            Layer = layer;
            Strength = strength;
            EvidenceLevel = evidenceLevel;
        }

        public BrickElement Source { get; }
        public BrickElement Target { get; }
        public BrickDependencyKindId KindId { get; }
        public BrickDependencyLayer Layer { get; }
        public BrickDependencyStrength Strength { get; }
        public BrickEvidenceLevel EvidenceLevel { get; }

        internal static BrickDependencyGraphEdge FromDependency(BrickDependency dependency) =>
            new BrickDependencyGraphEdge(
                dependency.Source,
                dependency.Target,
                dependency.KindId,
                dependency.Layer,
                dependency.Strength,
                dependency.EvidenceLevel);
    }
}
