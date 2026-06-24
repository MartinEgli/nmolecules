using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Advisory structural rule suggested by AI and awaiting deterministic review or promotion.
    /// </summary>
    /// <remarks>
    /// Proposals cannot start in the <see cref="BrickRuleLifecycleState.Enforced"/> state.
    /// Enforced rules must be promoted explicitly through a reviewed policy workflow.
    /// </remarks>
    public sealed class BrickRuleProposal
    {
        /// <summary>
        /// Creates an advisory rule proposal with selectors, suggested decision, severity, evidence, and lifecycle state.
        /// </summary>
        public BrickRuleProposal(
            string proposalId,
            string title,
            string rationale,
            BrickRoleSelector sourceRoles,
            BrickRoleSelector targetRoles,
            BrickDependencyKindId dependencyKindId,
            BrickDecision suggestedDecision,
            BrickSeverity suggestedSeverity,
            BrickRuleProposalEvidence evidence,
            BrickRuleLifecycleState lifecycleState)
        {
            if (lifecycleState == BrickRuleLifecycleState.Enforced)
            {
                throw new ArgumentOutOfRangeException(nameof(lifecycleState), "AI-generated rule proposals cannot start enforced.");
            }

            ProposalId = proposalId ?? string.Empty;
            Title = title ?? string.Empty;
            Rationale = rationale ?? string.Empty;
            SourceRoles = sourceRoles;
            TargetRoles = targetRoles;
            DependencyKindId = dependencyKindId;
            SuggestedDecision = suggestedDecision;
            SuggestedSeverity = suggestedSeverity;
            Evidence = evidence ?? new BrickRuleProposalEvidence(null, null, null, null, null, null);
            LifecycleState = lifecycleState;
        }

        /// <summary>Stable proposal identifier.</summary>
        public string ProposalId { get; }
        /// <summary>Human-readable proposal title.</summary>
        public string Title { get; }
        /// <summary>Reason why the rule may be useful.</summary>
        public string Rationale { get; }
        /// <summary>Source-role selector for the proposed rule.</summary>
        public BrickRoleSelector SourceRoles { get; }
        /// <summary>Target-role selector for the proposed rule.</summary>
        public BrickRoleSelector TargetRoles { get; }
        /// <summary>Dependency kind the proposed rule applies to.</summary>
        public BrickDependencyKindId DependencyKindId { get; }
        /// <summary>Suggested deterministic decision if the proposal is later promoted.</summary>
        public BrickDecision SuggestedDecision { get; }
        /// <summary>Suggested severity if the proposal is later promoted.</summary>
        public BrickSeverity SuggestedSeverity { get; }
        /// <summary>Evidence supporting and constraining the proposal.</summary>
        public BrickRuleProposalEvidence Evidence { get; }
        /// <summary>Current review lifecycle state.</summary>
        public BrickRuleLifecycleState LifecycleState { get; }
        /// <summary>Indicates whether the proposal is still advisory.</summary>
        public bool IsAdvisory => LifecycleState != BrickRuleLifecycleState.Enforced;
        /// <summary>Indicates whether the proposal can currently break builds.</summary>
        public bool CanBreakBuild => LifecycleState == BrickRuleLifecycleState.Enforced;
        /// <summary>Indicates whether the proposal has the minimum evidence required for review.</summary>
        public bool HasRequiredEvidence => Evidence.HasRequiredEvidence;
    }
}
