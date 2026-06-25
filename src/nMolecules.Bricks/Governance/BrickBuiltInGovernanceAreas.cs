using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Provides built-in Bricks defaults for governance readiness areas, requirements, and summaries.
/// </summary>
public static class BrickBuiltInGovernanceAreas
    {
        public static BrickGovernanceAreaDefinition PolicyOwnership => Area(
            BrickGovernanceArea.PolicyOwnership,
            "Policy ownership",
            "Policies have explicit accountability and review ownership.",
            Requirement("policy-owner", BrickGovernanceArea.PolicyOwnership, "Policy owner", true, "Every activated policy has an accountable owner."),
            Requirement("review-process", BrickGovernanceArea.PolicyOwnership, "Review process", true, "Policy changes are reviewed like code."),
            Requirement("change-history", BrickGovernanceArea.PolicyOwnership, "Change history", true, "Policy changes remain auditable."));

        public static BrickGovernanceAreaDefinition ExceptionHandling => Area(
            BrickGovernanceArea.ExceptionHandling,
            "Exception handling",
            "Exceptions are deliberate, separate from baselines, and time-aware.",
            Requirement("suppression-justification", BrickGovernanceArea.ExceptionHandling, "Suppression justification", true, "Suppressions carry an intentional reason."),
            Requirement("suppression-owner", BrickGovernanceArea.ExceptionHandling, "Suppression owner", true, "Suppressions carry an accountable owner."),
            Requirement("expiration-policy", BrickGovernanceArea.ExceptionHandling, "Expiration policy", true, "Exceptions can expire and be revisited."),
            Requirement("baseline-separation", BrickGovernanceArea.ExceptionHandling, "Baseline separation", true, "Baselines and suppressions stay separate."));

        public static BrickGovernanceAreaDefinition RolePackEvolution => Area(
            BrickGovernanceArea.RolePackEvolution,
            "Role-pack evolution",
            "Role packs can evolve without hidden compatibility drift.",
            Requirement("role-pack-owner", BrickGovernanceArea.RolePackEvolution, "Role-pack owner", true, "Role packs have ownership."),
            Requirement("role-pack-versioning", BrickGovernanceArea.RolePackEvolution, "Role-pack versioning", true, "Role-pack changes are versioned."),
            Requirement("role-pack-deprecation-path", BrickGovernanceArea.RolePackEvolution, "Role-pack deprecation path", true, "Role-pack changes have a migration path."));

        public static BrickGovernanceAreaDefinition CompatibilityExpectations => Area(
            BrickGovernanceArea.CompatibilityExpectations,
            "Compatibility expectations",
            "Public contracts and machine-readable outputs stay compatible.",
            Requirement("schema-versioning", BrickGovernanceArea.CompatibilityExpectations, "Schema versioning", true, "Export contracts carry schema versions."),
            Requirement("stable-diagnostic-ids", BrickGovernanceArea.CompatibilityExpectations, "Stable diagnostic IDs", true, "Diagnostic IDs remain stable."),
            Requirement("breaking-change-review", BrickGovernanceArea.CompatibilityExpectations, "Breaking-change review", true, "Breaking changes are intentional and reviewed."));

        public static IReadOnlyList<BrickGovernanceAreaDefinition> All => new[]
        {
            PolicyOwnership,
            ExceptionHandling,
            RolePackEvolution,
            CompatibilityExpectations
        };

        private static BrickGovernanceAreaDefinition Area(
            BrickGovernanceArea area,
            string displayName,
            string description,
            params BrickGovernanceRequirement[] requirements) =>
            new BrickGovernanceAreaDefinition(area, displayName, description, requirements);

        private static BrickGovernanceRequirement Requirement(
            string id,
            BrickGovernanceArea area,
            string displayName,
            bool required,
            string rationale) =>
            new BrickGovernanceRequirement(id, area, displayName, required, rationale);
    }
}
