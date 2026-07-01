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
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
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

        /// <summary>
        /// Gets the Source value used by Bricks developer tooling.
        /// </summary>
        public BrickElement Source { get; }
        /// <summary>
        /// Gets the Target value used by Bricks developer tooling.
        /// </summary>
        public BrickElement Target { get; }
        /// <summary>
        /// Gets the Kind Id value used by Bricks developer tooling.
        /// </summary>
        public BrickDependencyKindId KindId { get; }
        /// <summary>
        /// Gets the Layer value used by Bricks developer tooling.
        /// </summary>
        public BrickDependencyLayer Layer { get; }
        /// <summary>
        /// Gets the Strength value used by Bricks developer tooling.
        /// </summary>
        public BrickDependencyStrength Strength { get; }
        /// <summary>
        /// Gets the Evidence Level value used by Bricks developer tooling.
        /// </summary>
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
