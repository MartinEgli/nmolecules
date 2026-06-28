using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Result of reviewing an AI-generated rule proposal.
    /// </summary>
    public sealed class BrickRuleProposalReviewResult
    {
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

        public BrickRuleProposal Proposal { get; }
        public BrickRuleProposalReview Review { get; }
        public bool CanPromote { get; }
        public string Reason { get; }
        public BrickRule? PromotedRule { get; }
    }
}
