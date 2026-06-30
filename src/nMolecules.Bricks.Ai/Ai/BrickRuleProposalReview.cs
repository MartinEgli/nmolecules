using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Human review decision for one AI-generated rule proposal.
    /// </summary>
    public sealed class BrickRuleProposalReview
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
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

        /// <summary>
        /// Gets the Proposal Id value used by Bricks developer tooling.
        /// </summary>
        public string ProposalId { get; }
        /// <summary>
        /// Gets the Reviewer value used by Bricks developer tooling.
        /// </summary>
        public string Reviewer { get; }
        /// <summary>
        /// Gets the Approved value used by Bricks developer tooling.
        /// </summary>
        public bool Approved { get; }
        /// <summary>
        /// Gets the Target Lifecycle State value used by Bricks developer tooling.
        /// </summary>
        public BrickRuleLifecycleState TargetLifecycleState { get; }
        /// <summary>
        /// Gets the Rationale value used by Bricks developer tooling.
        /// </summary>
        public string Rationale { get; }
        /// <summary>
        /// Gets the timestamp associated with this Bricks model object.
        /// </summary>
        public DateTimeOffset ReviewedAt { get; }
        /// <summary>
        /// Gets a value indicating whether Has Reviewer applies.
        /// </summary>
        public bool HasReviewer => !string.IsNullOrWhiteSpace(Reviewer);
        /// <summary>
        /// Gets a value indicating whether Has Rationale applies.
        /// </summary>
        public bool HasRationale => !string.IsNullOrWhiteSpace(Rationale);
    }
}
