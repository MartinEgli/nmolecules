using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Defines the policy data used to govern policy documents, aliases, imports, composition, and policy-driven
/// role assignment.
/// </summary>
public sealed class BrickPolicy
    {
        public BrickPolicy(BrickPolicyId id, string name, IEnumerable<BrickPolicyImport> imports, IEnumerable<BrickRule> rules, BrickPermissionDefault defaultDecision, BrickEnforcementMode enforcement)
            : this(id, name, imports, rules, null, null, null, defaultDecision, enforcement)
        {
        }

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

        public BrickPolicyId Id { get; }
        public string Name { get; }
        public IReadOnlyList<BrickPolicyImport> Imports { get; }
        public IReadOnlyList<BrickRule> Rules { get; }
        public IReadOnlyList<BrickRoleCombinationRule> CombinationRules { get; }
        public IReadOnlyList<BrickRoleAssignment> ExternalAssignments { get; }
        public IReadOnlyList<BrickAlias> Aliases { get; }
        public BrickPermissionDefault DefaultDecision { get; }
        public BrickEnforcementMode Enforcement { get; }
    }
}
