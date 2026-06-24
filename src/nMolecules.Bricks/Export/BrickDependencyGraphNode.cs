using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickDependencyGraphNode
    {
        public BrickDependencyGraphNode(BrickElement element)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
        }

        public BrickElement Element { get; }
    }
}
