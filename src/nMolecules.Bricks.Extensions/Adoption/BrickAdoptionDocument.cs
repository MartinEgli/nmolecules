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
        public const string CurrentSchema = "NMolecules.Bricks.Adoption/1.0";

        public BrickAdoptionDocument(
            DateTimeOffset generatedAt,
            IEnumerable<BrickBaselineEntry> baselines,
            IEnumerable<BrickSuppression> suppressions)
            : this(generatedAt, baselines, suppressions, CurrentSchema)
        {
        }

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

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickBaselineEntry> Baselines { get; }
        public IReadOnlyList<BrickSuppression> Suppressions { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
        public bool HasEntries => Baselines.Count > 0 || Suppressions.Count > 0;
    }
}
