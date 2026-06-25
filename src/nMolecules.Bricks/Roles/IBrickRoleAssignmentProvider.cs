using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Defines the contract for i brick role assignment provider integration points in the Bricks pipeline.
/// </summary>
public interface IBrickRoleAssignmentProvider
    {
        IEnumerable<BrickRoleAssignment> GetAssignments(BrickModelContext context);
    }
}
