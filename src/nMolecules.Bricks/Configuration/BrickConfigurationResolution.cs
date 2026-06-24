using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickConfigurationResolution
    {
        public BrickConfigurationResolution(IEnumerable<BrickConfigurationResolvedEntry> entries)
        {
            Entries = (entries ?? Enumerable.Empty<BrickConfigurationResolvedEntry>())
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .ToArray();
        }

        public IReadOnlyList<BrickConfigurationResolvedEntry> Entries { get; }
        public bool HasEntries => Entries.Count > 0;
    }
}
