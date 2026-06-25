using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents role assignment data used by role dimensions, assignments, resolution, conflicts, and role
/// packs.
/// </summary>
public sealed class BrickRoleAssignment
    {
        public BrickRoleAssignment(
            BrickElementSelector? selector,
            RoleId roleId,
            BrickAssignmentMode mode,
            BrickAssignmentSource source,
            BrickAssignmentPrecedence precedence,
            BrickAssignmentBehavior behavior,
            string reason = null)
        {
            Selector = selector ?? default;
            RoleId = roleId;
            Mode = mode;
            Source = source;
            Precedence = precedence;
            Behavior = behavior;
            Reason = reason;
        }

        public BrickElementSelector Selector { get; }
        public RoleId RoleId { get; }
        public BrickAssignmentMode Mode { get; }
        public BrickAssignmentSource Source { get; }
        public BrickAssignmentPrecedence Precedence { get; }
        public BrickAssignmentBehavior Behavior { get; }
        public string Reason { get; }
    }
}
