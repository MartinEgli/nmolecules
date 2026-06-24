using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickConfigurationResolvedEntry
    {
        public BrickConfigurationResolvedEntry(
            string key,
            BrickConfigurationEntry effectiveEntry,
            IEnumerable<BrickConfigurationEntry> shadowedEntries)
        {
            Key = key ?? string.Empty;
            EffectiveEntry = effectiveEntry ?? throw new ArgumentNullException(nameof(effectiveEntry));
            ShadowedEntries = (shadowedEntries ?? Enumerable.Empty<BrickConfigurationEntry>()).ToArray();
        }

        public string Key { get; }
        public BrickConfigurationEntry EffectiveEntry { get; }
        public IReadOnlyList<BrickConfigurationEntry> ShadowedEntries { get; }
    }
}
