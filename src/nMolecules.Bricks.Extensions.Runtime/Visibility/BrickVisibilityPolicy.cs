using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Defines the policy data used to govern assembly visibility and friend-assembly policy checks.
/// </summary>
public sealed class BrickVisibilityPolicy
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickVisibilityPolicy(
            IEnumerable<RoleId> allowedFriendConsumerRoles = null,
            bool requireJustification = true,
            bool enabled = true)
        {
            AllowedFriendConsumerRoles = (allowedFriendConsumerRoles ?? Defaults()).Distinct().ToArray();
            RequireJustification = requireJustification;
            Enabled = enabled;
        }

        /// <summary>
        /// Gets a value indicating whether Allowed Friend Consumer Roles applies.
        /// </summary>
        public IReadOnlyList<RoleId> AllowedFriendConsumerRoles { get; }
        /// <summary>
        /// Gets a value indicating whether Require Justification applies.
        /// </summary>
        public bool RequireJustification { get; }
        /// <summary>
        /// Gets a value indicating whether Enabled applies.
        /// </summary>
        public bool Enabled { get; }

        private static IEnumerable<RoleId> Defaults()
        {
            yield return RoleId.From("FriendConsumer");
            yield return RoleId.From("TestOnly");
        }
    }
}
