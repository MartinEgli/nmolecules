using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Result of reviewing an AI-generated rule proposal.
    /// </summary>
    public sealed class BrickRuleProposalReviewResult
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickRuleProposalReviewResult(
            BrickRuleProposal proposal,
            BrickRuleProposalReview review,
            bool canPromote,
            string reason,
            BrickRule? promotedRule)
        {
            Proposal = proposal ?? throw new ArgumentNullException(nameof(proposal));
            Review = review ?? throw new ArgumentNullException(nameof(review));
            CanPromote = canPromote;
            Reason = reason ?? string.Empty;
            PromotedRule = promotedRule;
        }

        /// <summary>
        /// Gets the Proposal value used by Bricks developer tooling.
        /// </summary>
        public BrickRuleProposal Proposal { get; }
        /// <summary>
        /// Gets the Review value used by Bricks developer tooling.
        /// </summary>
        public BrickRuleProposalReview Review { get; }
        /// <summary>
        /// Gets a value indicating whether Can Promote applies.
        /// </summary>
        public bool CanPromote { get; }
        /// <summary>
        /// Gets the Reason value used by Bricks developer tooling.
        /// </summary>
        public string Reason { get; }
        /// <summary>
        /// Gets the Promoted Rule value used by Bricks developer tooling.
        /// </summary>
        public BrickRule? PromotedRule { get; }
    }
}
