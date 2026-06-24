using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickRoleCombinationRule
    {
        public BrickRoleCombinationRule(
            string name,
            BrickRoleSelector? leftRoles,
            BrickRoleSelector? rightRoles,
            BrickCombinationKind kind,
            string reason = null)
        {
            Name = name ?? string.Empty;
            LeftRoles = leftRoles ?? default;
            RightRoles = rightRoles ?? default;
            Kind = kind;
            Reason = reason;
        }

        public string Name { get; }
        public BrickRoleSelector LeftRoles { get; }
        public BrickRoleSelector RightRoles { get; }
        public BrickCombinationKind Kind { get; }
        public string Reason { get; }

        public bool Matches(RoleId left, RoleId right) =>
            (LeftRoles.Matches(left) && RightRoles.Matches(right)) ||
            (LeftRoles.Matches(right) && RightRoles.Matches(left));
    }
}
