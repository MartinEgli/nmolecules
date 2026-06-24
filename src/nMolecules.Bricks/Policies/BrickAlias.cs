using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickAlias
    {
        public BrickAlias(
            string name,
            BrickElementSelector? selector,
            RoleId canonicalRoleId,
            BrickAssignmentPrecedence precedence,
            BrickAssignmentBehavior behavior,
            string reason = null)
        {
            Name = name ?? string.Empty;
            Selector = selector ?? default;
            CanonicalRoleId = canonicalRoleId;
            Precedence = precedence;
            Behavior = behavior;
            Reason = reason;
        }

        public string Name { get; }
        public BrickElementSelector Selector { get; }
        public RoleId CanonicalRoleId { get; }
        public BrickAssignmentPrecedence Precedence { get; }
        public BrickAssignmentBehavior Behavior { get; }
        public string Reason { get; }
    }
}
