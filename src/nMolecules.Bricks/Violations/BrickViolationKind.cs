

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes why a violation exists. Use it to separate dependency-rule
    /// problems from role-resolution, configuration, baseline, suppression, and member-contract issues.
    /// </summary>
    public enum BrickViolationKind
    {
        /// <summary>A dependency matched a forbidden or denied dependency rule.</summary>
        DependencyRule = 0,
        /// <summary>A required dependency was missing.</summary>
        RequiredDependency = 1,
        /// <summary>Role assignment or role resolution produced an issue.</summary>
        RoleResolution = 2,
        /// <summary>A role-combination rule was violated.</summary>
        RoleCombination = 3,
        /// <summary>The policy itself is invalid, incomplete, or unsupported.</summary>
        PolicyConfiguration = 4,
        /// <summary>The finding relates to a baseline entry.</summary>
        Baseline = 5,
        /// <summary>The finding relates to a suppression entry.</summary>
        Suppression = 6,
        /// <summary>A member-cardinality contract was violated.</summary>
        MemberCardinality = 7,
        /// <summary>An element violates a single-element structural constraint.</summary>
        ElementConstraint = 8,
        /// <summary>An element has conflicting structural constraints with no active override.</summary>
        ElementConstraintConflict = 9
    }
}
