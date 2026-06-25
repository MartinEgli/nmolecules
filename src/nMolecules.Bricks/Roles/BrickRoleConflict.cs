using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents role conflict data used by role dimensions, assignments, resolution, conflicts, and role
/// packs.
/// </summary>
public sealed class BrickRoleConflict
    {
        public BrickRoleConflict(BrickRoleAssignment firstAssignment, BrickRoleAssignment secondAssignment, string reason)
        {
            FirstAssignment = firstAssignment ?? throw new ArgumentNullException(nameof(firstAssignment));
            SecondAssignment = secondAssignment ?? throw new ArgumentNullException(nameof(secondAssignment));
            Reason = reason ?? string.Empty;
        }

        public BrickRoleAssignment FirstAssignment { get; }
        public BrickRoleAssignment SecondAssignment { get; }
        public string Reason { get; }
    }
}
