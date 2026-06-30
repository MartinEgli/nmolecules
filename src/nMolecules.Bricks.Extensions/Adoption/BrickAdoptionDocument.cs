using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Defines the document shape used to exchange adoption document data between Bricks tools.
/// </summary>
public sealed class BrickAdoptionDocument
    {
        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public const string CurrentSchema = "NMolecules.Bricks.Adoption/1.0";

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickAdoptionDocument(
            DateTimeOffset generatedAt,
            IEnumerable<BrickBaselineEntry> baselines,
            IEnumerable<BrickSuppression> suppressions)
            : this(generatedAt, baselines, suppressions, CurrentSchema)
        {
        }

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickAdoptionDocument(
            DateTimeOffset generatedAt,
            IEnumerable<BrickBaselineEntry> baselines,
            IEnumerable<BrickSuppression> suppressions,
            string schema)
        {
            GeneratedAt = generatedAt;
            Baselines = (baselines ?? Enumerable.Empty<BrickBaselineEntry>())
                .OrderBy(baseline => baseline.RuleId.Value, StringComparer.Ordinal)
                .ThenBy(baseline => baseline.SourcePattern, StringComparer.Ordinal)
                .ThenBy(baseline => baseline.TargetPattern, StringComparer.Ordinal)
                .ToArray();
            Suppressions = (suppressions ?? Enumerable.Empty<BrickSuppression>())
                .OrderBy(suppression => suppression.RuleId.Value, StringComparer.Ordinal)
                .ThenBy(suppression => suppression.Selector.Pattern, StringComparer.Ordinal)
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
        /// Gets the Baselines value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickBaselineEntry> Baselines { get; }
        /// <summary>
        /// Gets the Suppressions value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickSuppression> Suppressions { get; }
        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
        /// <summary>
        /// Gets a value indicating whether Has Entries applies.
        /// </summary>
        public bool HasEntries => Baselines.Count > 0 || Suppressions.Count > 0;
    }
}
