using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Provides role assignments for a Bricks model context.
    /// </summary>
    /// <remarks>
    /// Implement this interface when assignments come from policy documents, packages, conventions
    /// or project-specific evidence instead of direct attributes.
    /// </remarks>
    public interface IBrickRoleAssignmentProvider
    {
        /// <summary>
        /// Gets role assignments for the supplied model context.
        /// </summary>
        /// <param name="context">The model context to inspect.</param>
        /// <returns>The assignments contributed by this provider.</returns>
        IEnumerable<BrickRoleAssignment> GetAssignments(BrickModelContext context);
    }
}
