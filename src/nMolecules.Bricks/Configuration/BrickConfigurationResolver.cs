using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents configuration resolver data used by configuration source precedence and resolved Bricks
/// settings.
/// </summary>
public static class BrickConfigurationResolver
    {
        public static BrickConfigurationResolution Resolve(IEnumerable<BrickConfigurationEntry> entries)
        {
            var indexedEntries = (entries ?? Enumerable.Empty<BrickConfigurationEntry>())
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.Key))
                .Select((entry, index) => new IndexedEntry(entry, index))
                .ToArray();

            var resolved = indexedEntries
                .GroupBy(indexed => indexed.Entry.Key, StringComparer.Ordinal)
                .Select(group => ResolveGroup(group))
                .ToArray();

            return new BrickConfigurationResolution(resolved);
        }

        private static BrickConfigurationResolvedEntry ResolveGroup(IEnumerable<IndexedEntry> group)
        {
            var candidates = group.ToArray();
            var effective = candidates
                .OrderByDescending(candidate => candidate.Entry.Source.Precedence)
                .ThenByDescending(candidate => candidate.Index)
                .First();
            var shadowed = candidates
                .Where(candidate => candidate.Index != effective.Index)
                .OrderBy(candidate => candidate.Index)
                .Select(candidate => candidate.Entry)
                .ToArray();

            return new BrickConfigurationResolvedEntry(effective.Entry.Key, effective.Entry, shadowed);
        }

        private sealed class IndexedEntry
        {
            public IndexedEntry(BrickConfigurationEntry entry, int index)
            {
                Entry = entry;
                Index = index;
            }

            public BrickConfigurationEntry Entry { get; }
            public int Index { get; }
        }
    }
}
