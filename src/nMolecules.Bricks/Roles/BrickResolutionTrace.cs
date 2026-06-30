using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Captures an explanatory trace of role resolution for one element.
    /// </summary>
    /// <remarks>
    /// Use traces in diagnostics, reports or documentation when developers need to understand why
    /// a role was applied, suppressed or left in conflict.
    /// </remarks>
    public sealed class BrickResolutionTrace
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickResolutionTrace"/> class.
        /// </summary>
        /// <param name="element">The element being resolved.</param>
        /// <param name="candidates">Candidate role assignments considered by the resolver.</param>
        /// <param name="resolvedRoles">Role ids resolved for the element.</param>
        /// <param name="decisions">Human-readable decision notes.</param>
        /// <param name="hasConflict">Whether unresolved conflicts were detected.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="element"/> is null.</exception>
        public BrickResolutionTrace(
            BrickElement element,
            IEnumerable<BrickRoleAssignment> candidates,
            IEnumerable<RoleId> resolvedRoles,
            IEnumerable<string> decisions,
            bool hasConflict)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            Candidates = (candidates ?? Enumerable.Empty<BrickRoleAssignment>()).ToArray();
            ResolvedRoles = (resolvedRoles ?? Enumerable.Empty<RoleId>()).ToArray();
            Decisions = (decisions ?? Enumerable.Empty<string>()).Select(decision => decision ?? string.Empty).ToArray();
            HasConflict = hasConflict;
        }

        /// <summary>
        /// Gets the element being resolved.
        /// </summary>
        public BrickElement Element { get; }

        /// <summary>
        /// Gets candidate role assignments considered by the resolver.
        /// </summary>
        public IReadOnlyList<BrickRoleAssignment> Candidates { get; }

        /// <summary>
        /// Gets role ids resolved for the element.
        /// </summary>
        public IReadOnlyList<RoleId> ResolvedRoles { get; }

        /// <summary>
        /// Gets human-readable decision notes.
        /// </summary>
        public IReadOnlyList<string> Decisions { get; }

        /// <summary>
        /// Gets a value indicating whether unresolved conflicts were detected.
        /// </summary>
        public bool HasConflict { get; }
    }
}
