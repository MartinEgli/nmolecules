using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public interface IBrickRoleAssignmentProvider
    {
        IEnumerable<BrickRoleAssignment> GetAssignments(BrickModelContext context);
    }
}
