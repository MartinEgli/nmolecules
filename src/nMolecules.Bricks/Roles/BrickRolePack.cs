using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickRolePack
    {
        public BrickRolePack(
            string id,
            string displayName,
            IEnumerable<BrickRole> roles,
            IEnumerable<BrickRoleCombinationRule> combinationRules)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Roles = (roles ?? Enumerable.Empty<BrickRole>()).ToArray();
            CombinationRules = (combinationRules ?? Enumerable.Empty<BrickRoleCombinationRule>()).ToArray();
        }

        public string Id { get; }
        public string DisplayName { get; }
        public IReadOnlyList<BrickRole> Roles { get; }
        public IReadOnlyList<BrickRoleCombinationRule> CombinationRules { get; }

        public bool ContainsRole(RoleId roleId) => Roles.Any(role => role.Id == roleId);
    }
}
