using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Groups roles and their combination rules into a reusable role pack.
    /// </summary>
    /// <remarks>
    /// Use role packs to publish domain-specific or architecture-specific role catalogs such as DDD,
    /// layered architecture or clean architecture presets.
    /// </remarks>
    public sealed class BrickRolePack
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickRolePack"/> class.
        /// </summary>
        /// <param name="id">The stable role pack id.</param>
        /// <param name="displayName">The developer-facing role pack name.</param>
        /// <param name="roles">The roles supplied by the pack.</param>
        /// <param name="combinationRules">The role combination rules supplied by the pack.</param>
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

        /// <summary>
        /// Gets the stable role pack id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the developer-facing role pack name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the roles supplied by the pack.
        /// </summary>
        public IReadOnlyList<BrickRole> Roles { get; }

        /// <summary>
        /// Gets the role combination rules supplied by the pack.
        /// </summary>
        public IReadOnlyList<BrickRoleCombinationRule> CombinationRules { get; }

        /// <summary>
        /// Determines whether the pack contains a role with the supplied id.
        /// </summary>
        /// <param name="roleId">The role id to find.</param>
        /// <returns><c>true</c> when the role is part of this pack; otherwise <c>false</c>.</returns>
        public bool ContainsRole(RoleId roleId) => Roles.Any(role => role.Id == roleId);
    }
}
