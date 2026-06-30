using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Describes a policy document issue with enough context for diagnostics, governance or remediation.
    /// </summary>
    public sealed class BrickPolicyDocumentIssue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickPolicyDocumentIssue"/> class.
        /// </summary>
        /// <param name="ruleId">The rule or diagnostic id that identifies the issue type.</param>
        /// <param name="severity">The issue severity.</param>
        /// <param name="message">The human-readable issue message.</param>
        public BrickPolicyDocumentIssue(RuleId ruleId, BrickSeverity severity, string message)
        {
            RuleId = ruleId;
            Severity = severity;
            Message = message ?? string.Empty;
        }

        /// <summary>
        /// Gets the rule or diagnostic id that identifies the issue type.
        /// </summary>
        public RuleId RuleId { get; }

        /// <summary>
        /// Gets the issue severity.
        /// </summary>
        public BrickSeverity Severity { get; }

        /// <summary>
        /// Gets the human-readable issue message.
        /// </summary>
        public string Message { get; }
    }
}
