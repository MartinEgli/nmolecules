using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents resolution trace data used by role dimensions, assignments, resolution, conflicts, and role
/// packs.
/// </summary>
public sealed class BrickResolutionTrace
    {
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

        public BrickElement Element { get; }
        public IReadOnlyList<BrickRoleAssignment> Candidates { get; }
        public IReadOnlyList<RoleId> ResolvedRoles { get; }
        public IReadOnlyList<string> Decisions { get; }
        public bool HasConflict { get; }
    }
}
