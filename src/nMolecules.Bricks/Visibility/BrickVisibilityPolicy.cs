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
        public BrickVisibilityPolicy(
            IEnumerable<RoleId> allowedFriendConsumerRoles = null,
            bool requireJustification = true,
            bool enabled = true)
        {
            AllowedFriendConsumerRoles = (allowedFriendConsumerRoles ?? Defaults()).Distinct().ToArray();
            RequireJustification = requireJustification;
            Enabled = enabled;
        }

        public IReadOnlyList<RoleId> AllowedFriendConsumerRoles { get; }
        public bool RequireJustification { get; }
        public bool Enabled { get; }

        private static IEnumerable<RoleId> Defaults()
        {
            yield return RoleId.From("FriendConsumer");
            yield return RoleId.From("TestOnly");
        }
    }
}
