using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Summarizes report summary results so callers can display the important outcome without reading every
/// detail.
/// </summary>
public sealed class BrickReportSummary
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
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

        /// <summary>
        /// Gets the Total value used by Bricks developer tooling.
        /// </summary>
        public int Total { get; }
        /// <summary>
        /// Gets the Active value used by Bricks developer tooling.
        /// </summary>
        public int Active { get; }
        /// <summary>
        /// Gets the Suppressed value used by Bricks developer tooling.
        /// </summary>
        public int Suppressed { get; }
        /// <summary>
        /// Gets the Baselined value used by Bricks developer tooling.
        /// </summary>
        public int Baselined { get; }
        /// <summary>
        /// Gets the Expired Suppressions value used by Bricks developer tooling.
        /// </summary>
        public int ExpiredSuppressions { get; }
        /// <summary>
        /// Gets the Expired Baseline Entries value used by Bricks developer tooling.
        /// </summary>
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
