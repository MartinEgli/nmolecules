using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Represents the result of resolving role assignments for one Bricks element.
    /// </summary>
    /// <remarks>
    /// Use this result to inspect all candidate assignments, the assignments that became effective,
    /// suppressed assignments and unresolved conflicts before rule evaluation or reporting.
    /// </remarks>
    public sealed class BrickResolvedRoles
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickResolvedRoles"/> class.
        /// </summary>
        /// <param name="element">The element whose roles were resolved.</param>
        /// <param name="candidateAssignments">All assignments considered by the resolver.</param>
        /// <param name="appliedAssignments">Assignments that became effective.</param>
        /// <param name="suppressedAssignments">Assignments suppressed by precedence or suppress behavior.</param>
        /// <param name="conflicts">Role conflicts that could not be resolved automatically.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="element"/> is null.</exception>
        public BrickResolvedRoles(
            BrickElement element,
            IEnumerable<BrickRoleAssignment> candidateAssignments,
            IEnumerable<BrickRoleAssignment> appliedAssignments,
            IEnumerable<BrickRoleAssignment> suppressedAssignments,
            IEnumerable<BrickRoleConflict> conflicts)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            CandidateAssignments = (candidateAssignments ?? Enumerable.Empty<BrickRoleAssignment>()).ToArray();
            AppliedAssignments = (appliedAssignments ?? Enumerable.Empty<BrickRoleAssignment>()).ToArray();
            SuppressedAssignments = (suppressedAssignments ?? Enumerable.Empty<BrickRoleAssignment>()).ToArray();
            Conflicts = (conflicts ?? Enumerable.Empty<BrickRoleConflict>()).ToArray();
            EffectiveRoles = AppliedAssignments.Select(assignment => assignment.RoleId).Distinct().ToArray();
        }

        /// <summary>
        /// Gets the element whose roles were resolved.
        /// </summary>
        public BrickElement Element { get; }

        /// <summary>
        /// Gets all assignments considered by the resolver.
        /// </summary>
        public IReadOnlyList<BrickRoleAssignment> CandidateAssignments { get; }

        /// <summary>
        /// Gets the distinct role ids from the applied assignments.
        /// </summary>
        public IReadOnlyList<RoleId> EffectiveRoles { get; }

        /// <summary>
        /// Gets assignments that became effective.
        /// </summary>
        public IReadOnlyList<BrickRoleAssignment> AppliedAssignments { get; }

        /// <summary>
        /// Gets assignments suppressed by precedence or suppress behavior.
        /// </summary>
        public IReadOnlyList<BrickRoleAssignment> SuppressedAssignments { get; }

        /// <summary>
        /// Gets role conflicts that could not be resolved automatically.
        /// </summary>
        public IReadOnlyList<BrickRoleConflict> Conflicts { get; }

        /// <summary>
        /// Gets a value indicating whether unresolved role conflicts exist.
        /// </summary>
        public bool HasConflicts => Conflicts.Count > 0;
    }
}
