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
        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public const string CurrentSchema = "NMolecules.Bricks.ResolutionTrace/1.0";

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickResolutionTraceDocument(DateTimeOffset generatedAt, IEnumerable<BrickResolutionTraceEntry> entries)
            : this(generatedAt, entries, CurrentSchema)
        {
        }

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickResolutionTraceDocument(DateTimeOffset generatedAt, IEnumerable<BrickResolutionTraceEntry> entries, string schema)
        {
            GeneratedAt = generatedAt;
            Entries = (entries ?? Enumerable.Empty<BrickResolutionTraceEntry>())
                .OrderBy(entry => entry.Element.Id.Value, StringComparer.Ordinal)
                .ToArray();
            Schema = schema ?? string.Empty;
        }

        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public string Schema { get; }
        /// <summary>
        /// Gets the timestamp associated with this Bricks model object.
        /// </summary>
        public DateTimeOffset GeneratedAt { get; }
        /// <summary>
        /// Gets the Entries value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickResolutionTraceEntry> Entries { get; }
        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);

        /// <summary>
        /// Creates a Bricks configuration object from external key-value properties.
        /// </summary>
        public static BrickResolutionTraceDocument FromTraces(DateTimeOffset generatedAt, IEnumerable<BrickResolutionTrace> traces, string schema = CurrentSchema) =>
            new BrickResolutionTraceDocument(
                generatedAt,
                (traces ?? Enumerable.Empty<BrickResolutionTrace>()).Select(BrickResolutionTraceEntry.FromTrace),
                schema);
    }
}
