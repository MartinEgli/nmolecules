using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents profile data used by built-in Bricks profiles.
/// </summary>
public sealed class BrickProfile
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
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

        /// <summary>
        /// Gets the Id value used by Bricks developer tooling.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Gets the Display Name value used by Bricks developer tooling.
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// Gets the Role Packs value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickRolePack> RolePacks { get; }
        /// <summary>
        /// Gets the Rules value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickRule> Rules { get; }
        /// <summary>
        /// Gets the Default Decision value used by Bricks developer tooling.
        /// </summary>
        public BrickPermissionDefault DefaultDecision { get; }
        /// <summary>
        /// Gets the Enforcement value used by Bricks developer tooling.
        /// </summary>
        public BrickEnforcementMode Enforcement { get; }

        /// <summary>
        /// Executes the Contains Role operation for Bricks developer tooling.
        /// </summary>
        public bool ContainsRole(RoleId roleId) =>
            RolePacks.Any(pack => pack.ContainsRole(roleId));

        /// <summary>
        /// Converts this Bricks model object to the corresponding core representation.
        /// </summary>
        public BrickPolicy ToPolicy() =>
            ToPolicy(BrickPolicyId.From("Profile." + Id));

        /// <summary>
        /// Converts this Bricks model object to the corresponding core representation.
        /// </summary>
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
