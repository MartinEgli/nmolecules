using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Describes a conflict between two role assignments.
    /// </summary>
    /// <remarks>
    /// Role conflicts are emitted by role resolution when assignments are incompatible or cannot be
    /// resolved by precedence and behavior metadata alone.
    /// </remarks>
    public sealed class BrickRoleConflict
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickRoleConflict"/> class.
        /// </summary>
        /// <param name="firstAssignment">The first conflicting assignment.</param>
        /// <param name="secondAssignment">The second conflicting assignment.</param>
        /// <param name="reason">The human-readable reason for the conflict.</param>
        /// <exception cref="ArgumentNullException">Thrown when an assignment is null.</exception>
        public BrickRoleConflict(BrickRoleAssignment firstAssignment, BrickRoleAssignment secondAssignment, string reason)
        {
            FirstAssignment = firstAssignment ?? throw new ArgumentNullException(nameof(firstAssignment));
            SecondAssignment = secondAssignment ?? throw new ArgumentNullException(nameof(secondAssignment));
            Reason = reason ?? string.Empty;
        }

        /// <summary>
        /// Gets the first conflicting assignment.
        /// </summary>
        public BrickRoleAssignment FirstAssignment { get; }

        /// <summary>
        /// Gets the second conflicting assignment.
        /// </summary>
        public BrickRoleAssignment SecondAssignment { get; }

        /// <summary>
        /// Gets the human-readable reason for the conflict.
        /// </summary>
        public string Reason { get; }
    }
}
