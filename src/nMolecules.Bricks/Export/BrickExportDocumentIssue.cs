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
        public BrickExportDocumentIssue(RuleId ruleId, BrickSeverity severity, string message)
        {
            RuleId = ruleId;
            Severity = severity;
            Message = message ?? string.Empty;
        }

        public RuleId RuleId { get; }
        public BrickSeverity Severity { get; }
        public string Message { get; }
    }
}
