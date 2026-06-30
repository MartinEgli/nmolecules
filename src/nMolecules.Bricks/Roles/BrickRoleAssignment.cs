using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Assigns a role to elements selected by a <see cref="BrickElementSelector"/>.
    /// </summary>
    /// <remarks>
    /// Role assignments are consumed by <see cref="BrickRoleResolver"/>. They carry source,
    /// precedence and behavior metadata so project policies can combine package defaults,
    /// conventions and local overrides deterministically.
    /// </remarks>
    public sealed class BrickRoleAssignment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickRoleAssignment"/> class.
        /// </summary>
        /// <param name="selector">The element selector this assignment applies to.</param>
        /// <param name="roleId">The role id to assign.</param>
        /// <param name="mode">Whether the assignment adds, replaces or suppresses roles.</param>
        /// <param name="source">Where the assignment came from.</param>
        /// <param name="precedence">The assignment precedence used during conflict resolution.</param>
        /// <param name="behavior">The conflict behavior for this assignment.</param>
        /// <param name="reason">The optional human-readable reason for the assignment.</param>
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

        /// <summary>
        /// Gets the element selector this assignment applies to.
        /// </summary>
        public BrickElementSelector Selector { get; }

        /// <summary>
        /// Gets the role id assigned by this assignment.
        /// </summary>
        public RoleId RoleId { get; }

        /// <summary>
        /// Gets the assignment mode.
        /// </summary>
        public BrickAssignmentMode Mode { get; }

        /// <summary>
        /// Gets where the assignment came from.
        /// </summary>
        public BrickAssignmentSource Source { get; }

        /// <summary>
        /// Gets the assignment precedence used during conflict resolution.
        /// </summary>
        public BrickAssignmentPrecedence Precedence { get; }

        /// <summary>
        /// Gets the conflict behavior for this assignment.
        /// </summary>
        public BrickAssignmentBehavior Behavior { get; }

        /// <summary>
        /// Gets the optional human-readable reason for the assignment.
        /// </summary>
        public string Reason { get; }
    }
}
