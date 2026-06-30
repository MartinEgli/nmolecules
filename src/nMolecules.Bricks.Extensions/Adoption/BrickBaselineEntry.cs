using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents baseline entry data used by adoption workflows, baselines, suppressions, and violation
/// lifecycle state.
/// </summary>
public sealed class BrickBaselineEntry
    {
        public BrickBaselineEntry(
            RuleId ruleId,
            string sourcePattern,
            string targetPattern,
            string justification = null,
            string owner = null,
            DateTimeOffset? expiresAt = null)
        {
            RuleId = ruleId;
            SourcePattern = sourcePattern ?? string.Empty;
            TargetPattern = targetPattern ?? string.Empty;
            Justification = justification;
            Owner = owner;
            ExpiresAt = expiresAt;
        }

        public RuleId RuleId { get; }
        public string SourcePattern { get; }
        public string TargetPattern { get; }
        public string Justification { get; }
        public string Owner { get; }
        public DateTimeOffset? ExpiresAt { get; }

        public bool IsExpired(DateTimeOffset now) => ExpiresAt.HasValue && now > ExpiresAt.Value;
    }
}
