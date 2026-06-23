using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    public sealed class BrickModelContext
    {
        public BrickModelContext(BrickPolicy policy, IEnumerable<BrickElement> elements)
        {
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));
            Elements = (elements ?? Enumerable.Empty<BrickElement>()).ToArray();
        }

        public BrickPolicy Policy { get; }
        public IReadOnlyList<BrickElement> Elements { get; }
    }

    public interface IBrickRoleAssignmentProvider
    {
        IEnumerable<BrickRoleAssignment> GetAssignments(BrickModelContext context);
    }

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
