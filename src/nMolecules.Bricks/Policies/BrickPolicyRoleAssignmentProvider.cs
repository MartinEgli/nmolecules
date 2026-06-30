using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Provides role assignments declared by a policy.
    /// </summary>
    /// <remarks>
    /// This provider exposes both explicit external assignments and alias mappings from the current
    /// policy as role assignments for the role resolver.
    /// </remarks>
    public sealed class BrickPolicyRoleAssignmentProvider : IBrickRoleAssignmentProvider
    {
        /// <summary>
        /// Gets policy-backed role assignments for the supplied model context.
        /// </summary>
        /// <param name="context">The model context whose policy should be inspected.</param>
        /// <returns>Assignments contributed by the context policy.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
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
