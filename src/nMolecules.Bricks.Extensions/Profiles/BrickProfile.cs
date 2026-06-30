using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents profile data used by built-in Bricks profiles.
/// </summary>
public sealed class BrickProfile
    {
        public BrickProfile(
            string id,
            string displayName,
            IEnumerable<BrickRolePack> rolePacks,
            IEnumerable<BrickRule> rules,
            BrickPermissionDefault defaultDecision,
            BrickEnforcementMode enforcement)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            RolePacks = (rolePacks ?? Enumerable.Empty<BrickRolePack>()).ToArray();
            Rules = (rules ?? Enumerable.Empty<BrickRule>()).ToArray();
            DefaultDecision = defaultDecision;
            Enforcement = enforcement;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public IReadOnlyList<BrickRolePack> RolePacks { get; }
        public IReadOnlyList<BrickRule> Rules { get; }
        public BrickPermissionDefault DefaultDecision { get; }
        public BrickEnforcementMode Enforcement { get; }

        public bool ContainsRole(RoleId roleId) =>
            RolePacks.Any(pack => pack.ContainsRole(roleId));

        public BrickPolicy ToPolicy() =>
            ToPolicy(BrickPolicyId.From("Profile." + Id));

        public BrickPolicy ToPolicy(BrickPolicyId policyId) =>
            new BrickPolicy(
                policyId,
                DisplayName,
                Enumerable.Empty<BrickPolicyImport>(),
                Rules,
                RolePacks.SelectMany(pack => pack.CombinationRules),
                Enumerable.Empty<BrickRoleAssignment>(),
                Enumerable.Empty<BrickAlias>(),
                DefaultDecision,
                Enforcement);
    }
}
