using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents configuration resolution data used by configuration source precedence and resolved Bricks
/// settings.
/// </summary>
public sealed class BrickConfigurationResolution
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickConfigurationResolution(IEnumerable<BrickConfigurationResolvedEntry> entries)
        {
            Entries = (entries ?? Enumerable.Empty<BrickConfigurationResolvedEntry>())
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .ToArray();
        }

        /// <summary>
        /// Gets the Entries value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickConfigurationResolvedEntry> Entries { get; }
        /// <summary>
        /// Gets a value indicating whether Has Entries applies.
        /// </summary>
        public bool HasEntries => Entries.Count > 0;
    }
}
