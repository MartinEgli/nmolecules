using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Defines the document shape used to exchange resolution trace document data between Bricks tools.
/// </summary>
public sealed class BrickResolutionTraceDocument
    {
        public const string CurrentSchema = "NMolecules.Bricks.ResolutionTrace/1.0";

        public BrickResolutionTraceDocument(DateTimeOffset generatedAt, IEnumerable<BrickResolutionTraceEntry> entries)
            : this(generatedAt, entries, CurrentSchema)
        {
        }

        public BrickResolutionTraceDocument(DateTimeOffset generatedAt, IEnumerable<BrickResolutionTraceEntry> entries, string schema)
        {
            GeneratedAt = generatedAt;
            Entries = (entries ?? Enumerable.Empty<BrickResolutionTraceEntry>())
                .OrderBy(entry => entry.Element.Id.Value, StringComparer.Ordinal)
                .ToArray();
            Schema = schema ?? string.Empty;
        }

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickResolutionTraceEntry> Entries { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);

        public static BrickResolutionTraceDocument FromTraces(DateTimeOffset generatedAt, IEnumerable<BrickResolutionTrace> traces, string schema = CurrentSchema) =>
            new BrickResolutionTraceDocument(
                generatedAt,
                (traces ?? Enumerable.Empty<BrickResolutionTrace>()).Select(BrickResolutionTraceEntry.FromTrace),
                schema);
    }
}
