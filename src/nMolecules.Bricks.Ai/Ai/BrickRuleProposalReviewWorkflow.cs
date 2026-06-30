using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Applies human review decisions to advisory AI rule proposals.
    /// </summary>
    public static class BrickRuleProposalReviewWorkflow
    {
        /// <summary>
        /// Reviews a Bricks AI rule proposal and returns the promotion decision.
        /// </summary>
        public static BrickRuleProposalReviewResult Review(
            BrickRuleProposal proposal,
            BrickRuleProposalReview review,
            RuleId promotedRuleId,
            string promotedRuleName,
            BrickScope scope = BrickScope.Type,
            int priority = 0)
        {
            if (proposal == null)
            {
                throw new ArgumentNullException(nameof(proposal));
            }

            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            var reason = Validate(proposal, review, promotedRuleId);
            if (reason != null)
            {
                return new BrickRuleProposalReviewResult(proposal, review, canPromote: false, reason, null);
            }

            var rule = new BrickRule(
                promotedRuleId,
                promotedRuleName,
                RoleId.From(proposal.SourceRoles.Pattern),
                RoleId.From(proposal.TargetRoles.Pattern),
                proposal.SuggestedDecision,
                scope,
                proposal.SuggestedSeverity,
                priority,
                review.Rationale);

            return new BrickRuleProposalReviewResult(
                proposal,
                review,
                canPromote: true,
                "Proposal was explicitly reviewed and can be promoted to deterministic policy.",
                rule);
        }

        private static string Validate(BrickRuleProposal proposal, BrickRuleProposalReview review, RuleId promotedRuleId)
        {
            if (!string.Equals(proposal.ProposalId, review.ProposalId, StringComparison.Ordinal))
            {
                return "Review proposal id does not match the proposal.";
            }

            if (!review.Approved)
            {
                return "Review did not approve promotion.";
            }

            if (!review.HasReviewer)
            {
                return "Review requires a reviewer.";
            }

            if (!review.HasRationale)
            {
                return "Review requires a rationale.";
            }

            if (!proposal.HasRequiredEvidence)
            {
                return "Proposal does not have the required evidence.";
            }

            if (review.TargetLifecycleState != BrickRuleLifecycleState.Enforced)
            {
                return "Only an explicit Enforced review target can promote a deterministic rule.";
            }

            if (promotedRuleId.IsEmpty)
            {
                return "Promoted rule id is required.";
            }

            return null;
        }
    }
}
