using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public static class BrickRoleAssignmentCollector
    {
        public static IReadOnlyList<BrickRoleAssignment> Collect(
            BrickModelContext context,
            IEnumerable<IBrickRoleAssignmentProvider> providers)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var assignments = new List<BrickRoleAssignment>();
            foreach (var provider in providers ?? Enumerable.Empty<IBrickRoleAssignmentProvider>())
            {
                if (provider is null)
                {
                    continue;
                }

                assignments.AddRange(provider.GetAssignments(context) ?? Enumerable.Empty<BrickRoleAssignment>());
            }

            return assignments;
        }
    }
}
