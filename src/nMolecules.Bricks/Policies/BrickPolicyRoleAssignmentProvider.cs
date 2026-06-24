using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickPolicyRoleAssignmentProvider : IBrickRoleAssignmentProvider
    {
        public IEnumerable<BrickRoleAssignment> GetAssignments(BrickModelContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var assignment in context.Policy.ExternalAssignments)
            {
                yield return assignment;
            }

            foreach (var alias in context.Policy.Aliases)
            {
                yield return new BrickRoleAssignment(
                    alias.Selector,
                    alias.CanonicalRoleId,
                    BrickAssignmentMode.AliasMapping,
                    BrickAssignmentSource.AliasMapping,
                    alias.Precedence,
                    alias.Behavior,
                    alias.Reason);
            }
        }
    }
}
