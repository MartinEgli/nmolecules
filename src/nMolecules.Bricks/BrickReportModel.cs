using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
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

    public sealed class BrickReportSummary
    {
        public BrickReportSummary(
            int total,
            int active,
            int suppressed,
            int baselined,
            int expiredSuppressions,
            int expiredBaselineEntries)
        {
            Total = total;
            Active = active;
            Suppressed = suppressed;
            Baselined = baselined;
            ExpiredSuppressions = expiredSuppressions;
            ExpiredBaselineEntries = expiredBaselineEntries;
        }

        public int Total { get; }
        public int Active { get; }
        public int Suppressed { get; }
        public int Baselined { get; }
        public int ExpiredSuppressions { get; }
        public int ExpiredBaselineEntries { get; }

        internal static BrickReportSummary FromViolations(IReadOnlyList<BrickViolation> violations) =>
            new BrickReportSummary(
                violations.Count,
                violations.Count(violation => violation.State == BrickViolationState.Active),
                violations.Count(violation => violation.State == BrickViolationState.Suppressed),
                violations.Count(violation => violation.State == BrickViolationState.Baselined),
                violations.Count(violation => violation.State == BrickViolationState.ExpiredSuppression),
                violations.Count(violation => violation.State == BrickViolationState.ExpiredBaseline));
    }
}
