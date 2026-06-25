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
        public BrickDependencyGraphNode(BrickElement element)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
        }

        public BrickElement Element { get; }
    }
}
