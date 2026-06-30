using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Captures the safety boundary between deterministic Bricks enforcement and AI assistance.
    /// </summary>
    public sealed class BrickAiTrustBoundary
    {
        /// <summary>
        /// Creates a trust boundary configuration for AI comments, rule proposals, and automation.
        /// </summary>
        /// <remarks>
        /// Automatic enforcement is only preserved when silent policy mutation is explicitly allowed too.
        /// This keeps unsafe one-flag escalation from enabling build-breaking changes by accident.
        /// </remarks>
        public BrickAiTrustBoundary(
            BrickAiMode mode,
            BrickAiCommentFormat commentFormat,
            bool allowRuleProposal,
            bool allowAutoEnforcement,
            bool allowSilentPolicyMutation)
        {
            Mode = mode;
            CommentFormat = commentFormat;
            AllowRuleProposal = allowRuleProposal;
            AllowAutoEnforcement = allowAutoEnforcement && allowSilentPolicyMutation;
            AllowsSilentPolicyMutation = allowSilentPolicyMutation && AllowAutoEnforcement;
        }

        /// <summary>Safe default: AI assistance off, no proposals, no automatic enforcement.</summary>
        public static BrickAiTrustBoundary Default { get; } = new BrickAiTrustBoundary(
            BrickAiMode.Off,
            BrickAiCommentFormat.Markdown,
            false,
            false,
            false);

        /// <summary>Selected AI assistance mode.</summary>
        public BrickAiMode Mode { get; }
        /// <summary>Selected AI comment output format.</summary>
        public BrickAiCommentFormat CommentFormat { get; }
        /// <summary>Whether AI may create advisory rule proposals.</summary>
        public bool AllowRuleProposal { get; }
        /// <summary>Whether automatic enforcement is allowed by this boundary.</summary>
        public bool AllowAutoEnforcement { get; }
        /// <summary>Whether policy mutations may happen without explicit review.</summary>
        public bool AllowsSilentPolicyMutation { get; }
        /// <summary>Whether a rule can be activated without review.</summary>
        public bool CanActivateRuleWithoutReview => AllowAutoEnforcement && AllowsSilentPolicyMutation;
        /// <summary>Whether a suppression can be created without review.</summary>
        public bool CanCreateSuppressionWithoutReview => AllowsSilentPolicyMutation;
        /// <summary>Whether a baseline entry can be created without review.</summary>
        public bool CanCreateBaselineWithoutReview => AllowsSilentPolicyMutation;
    }
}
