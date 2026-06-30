using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Defines how two role selector groups may be combined on the same element.
    /// </summary>
    /// <remarks>
    /// Combination rules let a role pack declare additive, exclusive or incompatible role
    /// combinations. The resolver uses them to report conflicting role assignments.
    /// </remarks>
    public sealed class BrickRoleCombinationRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickRoleCombinationRule"/> class.
        /// </summary>
        /// <param name="name">The rule name used in diagnostics and documentation.</param>
        /// <param name="leftRoles">The left role selector.</param>
        /// <param name="rightRoles">The right role selector.</param>
        /// <param name="kind">The combination kind.</param>
        /// <param name="reason">The optional human-readable reason for the rule.</param>
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

        /// <summary>
        /// Gets the rule name used in diagnostics and documentation.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the left role selector.
        /// </summary>
        public BrickRoleSelector LeftRoles { get; }

        /// <summary>
        /// Gets the right role selector.
        /// </summary>
        public BrickRoleSelector RightRoles { get; }

        /// <summary>
        /// Gets the combination kind.
        /// </summary>
        public BrickCombinationKind Kind { get; }

        /// <summary>
        /// Gets the optional human-readable reason for the rule.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Determines whether this rule applies to the supplied role pair.
        /// </summary>
        /// <param name="left">The first role id.</param>
        /// <param name="right">The second role id.</param>
        /// <returns><c>true</c> when the role pair matches the selectors in either order; otherwise <c>false</c>.</returns>
        public bool Matches(RoleId left, RoleId right) =>
            (LeftRoles.Matches(left) && RightRoles.Matches(right)) ||
            (LeftRoles.Matches(right) && RightRoles.Matches(left));
    }
}
