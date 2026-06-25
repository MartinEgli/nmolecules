using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents suppression data used by adoption workflows, baselines, suppressions, and violation lifecycle
/// state.
/// </summary>
public sealed class BrickSuppression
    {
        public BrickSuppression(
            RuleId ruleId,
            BrickElementSelector? selector,
            string justification,
            string owner = null,
            DateTimeOffset? expiresAt = null)
        {
            RuleId = ruleId;
            Selector = selector ?? new BrickElementSelector(BrickElementKind.Unknown, string.Empty);
            Justification = justification ?? string.Empty;
            Owner = owner;
            ExpiresAt = expiresAt;
        }

        public RuleId RuleId { get; }
        public BrickElementSelector Selector { get; }
        public string Justification { get; }
        public string Owner { get; }
        public DateTimeOffset? ExpiresAt { get; }

        public bool IsExpired(DateTimeOffset now) => ExpiresAt.HasValue && now > ExpiresAt.Value;
    }
}
