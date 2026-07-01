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
        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public const string CurrentSchema = "NMolecules.Bricks.Report/1.0";

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickReportDocument(DateTimeOffset generatedAt, IEnumerable<BrickViolation> violations)
            : this(generatedAt, violations, CurrentSchema)
        {
        }

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickReportDocument(DateTimeOffset generatedAt, IEnumerable<BrickViolation> violations, string schema)
        {
            GeneratedAt = generatedAt;
            Violations = (violations ?? Enumerable.Empty<BrickViolation>()).ToArray();
            Schema = schema ?? string.Empty;
            Summary = BrickReportSummary.FromViolations(Violations);
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
        /// Gets the Violations value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickViolation> Violations { get; }
        /// <summary>
        /// Gets the Summary value used by Bricks developer tooling.
        /// </summary>
        public BrickReportSummary Summary { get; }
        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }
}
