using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents model context data used by role dimensions, assignments, resolution, conflicts, and role
/// packs.
/// </summary>
public sealed class BrickModelContext
    {
        public BrickModelContext(BrickPolicy policy, IEnumerable<BrickElement> elements)
        {
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));
            Elements = (elements ?? Enumerable.Empty<BrickElement>()).ToArray();
        }

        public BrickPolicy Policy { get; }
        public IReadOnlyList<BrickElement> Elements { get; }
    }
}
