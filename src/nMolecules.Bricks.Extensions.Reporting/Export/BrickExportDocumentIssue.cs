using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Describes a export document issue issue with enough context for diagnostics, governance, or remediation.
/// </summary>
public sealed class BrickExportDocumentIssue
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickExportDocumentIssue(RuleId ruleId, BrickSeverity severity, string message)
        {
            RuleId = ruleId;
            Severity = severity;
            Message = message ?? string.Empty;
        }

        /// <summary>
        /// Gets the diagnostic rule id used when reporting this Bricks validation condition.
        /// </summary>
        public RuleId RuleId { get; }
        /// <summary>
        /// Gets the Severity value used by Bricks developer tooling.
        /// </summary>
        public BrickSeverity Severity { get; }
        /// <summary>
        /// Gets the Message value used by Bricks developer tooling.
        /// </summary>
        public string Message { get; }
    }
}
