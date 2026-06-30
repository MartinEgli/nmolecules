using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Defines a deterministic Bricks policy with imports, rules, role metadata and enforcement defaults.
    /// </summary>
    /// <remarks>
    /// Use policies as the exchangeable unit for architectural governance. A policy can define
    /// dependency rules, role-combination rules, external assignments, aliases and the default
    /// decision for dependencies not covered by explicit rules.
    /// </remarks>
    public sealed class BrickPolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickPolicy"/> class without role metadata.
        /// </summary>
        /// <param name="id">The stable policy id.</param>
        /// <param name="name">The developer-facing policy name.</param>
        /// <param name="imports">Policies imported by this policy.</param>
        /// <param name="rules">Dependency rules declared by this policy.</param>
        /// <param name="defaultDecision">The default decision for uncovered dependencies.</param>
        /// <param name="enforcement">The enforcement mode for this policy.</param>
        public BrickPolicy(BrickPolicyId id, string name, IEnumerable<BrickPolicyImport> imports, IEnumerable<BrickRule> rules, BrickPermissionDefault defaultDecision, BrickEnforcementMode enforcement)
            : this(id, name, imports, rules, null, null, null, defaultDecision, enforcement)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrickPolicy"/> class.
        /// </summary>
        /// <param name="id">The stable policy id.</param>
        /// <param name="name">The developer-facing policy name.</param>
        /// <param name="imports">Policies imported by this policy.</param>
        /// <param name="rules">Dependency rules declared by this policy.</param>
        /// <param name="combinationRules">Role-combination rules declared by this policy.</param>
        /// <param name="externalAssignments">Role assignments declared outside direct attributes.</param>
        /// <param name="aliases">Role aliases declared by this policy.</param>
        /// <param name="defaultDecision">The default decision for uncovered dependencies.</param>
        /// <param name="enforcement">The enforcement mode for this policy.</param>
        public BrickPolicy(
            BrickPolicyId id,
            string name,
            IEnumerable<BrickPolicyImport> imports,
            IEnumerable<BrickRule> rules,
            IEnumerable<BrickRoleCombinationRule> combinationRules,
            IEnumerable<BrickRoleAssignment> externalAssignments,
            IEnumerable<BrickAlias> aliases,
            BrickPermissionDefault defaultDecision,
            BrickEnforcementMode enforcement)
        {
            Id = id;
            Name = name ?? string.Empty;
            Imports = (imports ?? Enumerable.Empty<BrickPolicyImport>()).ToArray();
            Rules = (rules ?? Enumerable.Empty<BrickRule>()).ToArray();
            CombinationRules = (combinationRules ?? Enumerable.Empty<BrickRoleCombinationRule>()).ToArray();
            ExternalAssignments = (externalAssignments ?? Enumerable.Empty<BrickRoleAssignment>()).ToArray();
            Aliases = (aliases ?? Enumerable.Empty<BrickAlias>()).ToArray();
            DefaultDecision = defaultDecision;
            Enforcement = enforcement;
        }

        /// <summary>
        /// Gets the stable policy id.
        /// </summary>
        public BrickPolicyId Id { get; }

        /// <summary>
        /// Gets the developer-facing policy name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets policies imported by this policy.
        /// </summary>
        public IReadOnlyList<BrickPolicyImport> Imports { get; }

        /// <summary>
        /// Gets dependency rules declared by this policy.
        /// </summary>
        public IReadOnlyList<BrickRule> Rules { get; }

        /// <summary>
        /// Gets role-combination rules declared by this policy.
        /// </summary>
        public IReadOnlyList<BrickRoleCombinationRule> CombinationRules { get; }

        /// <summary>
        /// Gets role assignments declared outside direct attributes.
        /// </summary>
        public IReadOnlyList<BrickRoleAssignment> ExternalAssignments { get; }

        /// <summary>
        /// Gets role aliases declared by this policy.
        /// </summary>
        public IReadOnlyList<BrickAlias> Aliases { get; }

        /// <summary>
        /// Gets the default decision for uncovered dependencies.
        /// </summary>
        public BrickPermissionDefault DefaultDecision { get; }

        /// <summary>
        /// Gets the enforcement mode for this policy.
        /// </summary>
        public BrickEnforcementMode Enforcement { get; }
    }
}
