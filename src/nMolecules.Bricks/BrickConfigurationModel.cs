using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    public sealed class BrickConfigurationSource
    {
        public static BrickConfigurationSource Generated { get; } = new BrickConfigurationSource(
            "generated",
            BrickConfigurationSourceKind.Generated,
            "Generated configuration.");

        public BrickConfigurationSource(
            string id,
            BrickConfigurationSourceKind kind,
            string description = null)
        {
            Id = id ?? string.Empty;
            Kind = kind;
            Description = description ?? string.Empty;
        }

        public string Id { get; }
        public BrickConfigurationSourceKind Kind { get; }
        public string Description { get; }
        public int Precedence => BrickConfigurationPrecedence.Rank(Kind);
    }

    public sealed class BrickConfigurationEntry
    {
        public BrickConfigurationEntry(
            string key,
            string value,
            BrickConfigurationSource source)
        {
            Key = key ?? string.Empty;
            Value = value ?? string.Empty;
            Source = source ?? BrickConfigurationSource.Generated;
        }

        public string Key { get; }
        public string Value { get; }
        public BrickConfigurationSource Source { get; }
    }

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

    public static class BrickConfigurationPrecedence
    {
        public static int Rank(BrickConfigurationSourceKind kind)
        {
            switch (kind)
            {
                case BrickConfigurationSourceKind.SourceAnnotation:
                    return 6;
                case BrickConfigurationSourceKind.AnalyzerConfig:
                    return 5;
                case BrickConfigurationSourceKind.MSBuild:
                    return 4;
                case BrickConfigurationSourceKind.PolicyFile:
                    return 3;
                case BrickConfigurationSourceKind.Package:
                    return 2;
                default:
                    return 1;
            }
        }
    }
}
