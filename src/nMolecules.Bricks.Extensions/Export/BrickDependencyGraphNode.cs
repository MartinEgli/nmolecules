using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents dependency graph node data used by exportable role maps, dependency graphs, and resolution
/// traces.
/// </summary>
public sealed class BrickDependencyGraphNode
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickDependencyGraphNode(BrickElement element)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
        }

        /// <summary>
        /// Gets the Element value used by Bricks developer tooling.
        /// </summary>
        public BrickElement Element { get; }
    }
}
