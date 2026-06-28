using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Human review decision for one AI-generated rule proposal.
    /// </summary>
    public sealed class BrickRuleProposalReview
    {
        public BrickRuleProposalReview(
            string proposalId,
            string reviewer,
            bool approved,
            BrickRuleLifecycleState targetLifecycleState,
            string rationale,
            DateTimeOffset reviewedAt)
        {
            ProposalId = proposalId ?? string.Empty;
            Reviewer = reviewer ?? string.Empty;
            Approved = approved;
            TargetLifecycleState = targetLifecycleState;
            Rationale = rationale ?? string.Empty;
            ReviewedAt = reviewedAt;
        }

        public string ProposalId { get; }
        public string Reviewer { get; }
        public bool Approved { get; }
        public BrickRuleLifecycleState TargetLifecycleState { get; }
        public string Rationale { get; }
        public DateTimeOffset ReviewedAt { get; }
        public bool HasReviewer => !string.IsNullOrWhiteSpace(Reviewer);
        public bool HasRationale => !string.IsNullOrWhiteSpace(Rationale);
    }
}
