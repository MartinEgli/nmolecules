using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents configuration resolved entry data used by configuration source precedence and resolved Bricks
/// settings.
/// </summary>
public sealed class BrickConfigurationResolvedEntry
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickConfigurationResolvedEntry(
            string key,
            BrickConfigurationEntry effectiveEntry,
            IEnumerable<BrickConfigurationEntry> shadowedEntries)
        {
            Key = key ?? string.Empty;
            EffectiveEntry = effectiveEntry ?? throw new ArgumentNullException(nameof(effectiveEntry));
            ShadowedEntries = (shadowedEntries ?? Enumerable.Empty<BrickConfigurationEntry>()).ToArray();
        }

        /// <summary>
        /// Gets the Key value used by Bricks developer tooling.
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// Gets the Effective Entry value used by Bricks developer tooling.
        /// </summary>
        public BrickConfigurationEntry EffectiveEntry { get; }
        /// <summary>
        /// Gets the Shadowed Entries value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickConfigurationEntry> ShadowedEntries { get; }
    }
}
