using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Collects role assignments from multiple assignment providers.
    /// </summary>
    /// <remarks>
    /// Use this helper when project policy, package defaults and convention providers should be
    /// merged before role resolution.
    /// </remarks>
    public static class BrickRoleAssignmentCollector
    {
        /// <summary>
        /// Collects role assignments from all supplied providers.
        /// </summary>
        /// <param name="context">The model context passed to each provider.</param>
        /// <param name="providers">The providers to query. Null providers are ignored.</param>
        /// <returns>A snapshot of all contributed assignments.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
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
