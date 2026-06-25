using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Defines the document shape used to exchange report document data between Bricks tools.
/// </summary>
public sealed class BrickReportDocument
    {
        public const string CurrentSchema = "NMolecules.Bricks.Report/1.0";

        public BrickReportDocument(DateTimeOffset generatedAt, IEnumerable<BrickViolation> violations)
            : this(generatedAt, violations, CurrentSchema)
        {
        }

        public BrickReportDocument(DateTimeOffset generatedAt, IEnumerable<BrickViolation> violations, string schema)
        {
            GeneratedAt = generatedAt;
            Violations = (violations ?? Enumerable.Empty<BrickViolation>()).ToArray();
            Schema = schema ?? string.Empty;
            Summary = BrickReportSummary.FromViolations(Violations);
        }

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickViolation> Violations { get; }
        public BrickReportSummary Summary { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }
}
