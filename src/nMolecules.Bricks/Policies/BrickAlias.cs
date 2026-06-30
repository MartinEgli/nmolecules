using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Maps a selected element or element group to a canonical role id.
    /// </summary>
    /// <remarks>
    /// Use aliases when a project or package wants to adapt existing naming, marker or package
    /// concepts to a stable Bricks role without changing source annotations.
    /// </remarks>
    public sealed class BrickAlias
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickAlias"/> class.
        /// </summary>
        /// <param name="name">The alias name.</param>
        /// <param name="selector">The elements the alias applies to.</param>
        /// <param name="canonicalRoleId">The canonical role id assigned by the alias.</param>
        /// <param name="precedence">The precedence used during role resolution.</param>
        /// <param name="behavior">The assignment behavior used during role resolution.</param>
        /// <param name="reason">The optional reason for the alias.</param>
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

        /// <summary>
        /// Gets the alias name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the elements the alias applies to.
        /// </summary>
        public BrickElementSelector Selector { get; }

        /// <summary>
        /// Gets the canonical role id assigned by the alias.
        /// </summary>
        public RoleId CanonicalRoleId { get; }

        /// <summary>
        /// Gets the precedence used during role resolution.
        /// </summary>
        public BrickAssignmentPrecedence Precedence { get; }

        /// <summary>
        /// Gets the assignment behavior used during role resolution.
        /// </summary>
        public BrickAssignmentBehavior Behavior { get; }

        /// <summary>
        /// Gets the optional reason for the alias.
        /// </summary>
        public string Reason { get; }
    }
}
