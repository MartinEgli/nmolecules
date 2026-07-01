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
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
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

        /// <summary>
        /// Gets the diagnostic rule id used when reporting this Bricks validation condition.
        /// </summary>
        public RuleId RuleId { get; }
        /// <summary>
        /// Gets the Source Pattern value used by Bricks developer tooling.
        /// </summary>
        public string SourcePattern { get; }
        /// <summary>
        /// Gets the Target Pattern value used by Bricks developer tooling.
        /// </summary>
        public string TargetPattern { get; }
        /// <summary>
        /// Gets the Justification value used by Bricks developer tooling.
        /// </summary>
        public string Justification { get; }
        /// <summary>
        /// Gets the Owner value used by Bricks developer tooling.
        /// </summary>
        public string Owner { get; }
        /// <summary>
        /// Gets the timestamp associated with this Bricks model object.
        /// </summary>
        public DateTimeOffset? ExpiresAt { get; }

        /// <summary>
        /// Determines whether this Bricks model object satisfies the requested condition.
        /// </summary>
        public bool IsExpired(DateTimeOffset now) => ExpiresAt.HasValue && now > ExpiresAt.Value;
    }
}
