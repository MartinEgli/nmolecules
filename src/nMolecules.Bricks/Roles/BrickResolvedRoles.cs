using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickResolvedRoles
    {
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

        public BrickElement Element { get; }
        public IReadOnlyList<BrickRoleAssignment> CandidateAssignments { get; }
        public IReadOnlyList<RoleId> EffectiveRoles { get; }
        public IReadOnlyList<BrickRoleAssignment> AppliedAssignments { get; }
        public IReadOnlyList<BrickRoleAssignment> SuppressedAssignments { get; }
        public IReadOnlyList<BrickRoleConflict> Conflicts { get; }
        public bool HasConflicts => Conflicts.Count > 0;
    }
}
