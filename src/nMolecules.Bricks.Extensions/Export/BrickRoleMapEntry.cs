using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents role map entry data used by exportable role maps, dependency graphs, and resolution traces.
/// </summary>
public sealed class BrickRoleMapEntry
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickRoleMapEntry(
            BrickElement element,
            IEnumerable<RoleId> effectiveRoles,
            bool hasConflicts,
            int appliedAssignmentCount,
            int suppressedAssignmentCount)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            EffectiveRoles = (effectiveRoles ?? Enumerable.Empty<RoleId>())
                .Distinct()
                .OrderBy(roleId => roleId.Value, StringComparer.Ordinal)
                .ToArray();
            HasConflicts = hasConflicts;
            AppliedAssignmentCount = appliedAssignmentCount;
            SuppressedAssignmentCount = suppressedAssignmentCount;
        }

        /// <summary>
        /// Gets the Element value used by Bricks developer tooling.
        /// </summary>
        public BrickElement Element { get; }
        /// <summary>
        /// Gets the Effective Roles value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<RoleId> EffectiveRoles { get; }
        /// <summary>
        /// Gets a value indicating whether Has Conflicts applies.
        /// </summary>
        public bool HasConflicts { get; }
        /// <summary>
        /// Gets the Applied Assignment Count value used by Bricks developer tooling.
        /// </summary>
        public int AppliedAssignmentCount { get; }
        /// <summary>
        /// Gets the Suppressed Assignment Count value used by Bricks developer tooling.
        /// </summary>
        public int SuppressedAssignmentCount { get; }

        internal static BrickRoleMapEntry FromResolvedRoles(BrickResolvedRoles resolvedRoles) =>
            new BrickRoleMapEntry(
                resolvedRoles.Element,
                resolvedRoles.EffectiveRoles,
                resolvedRoles.HasConflicts,
                resolvedRoles.AppliedAssignments.Count,
                resolvedRoles.SuppressedAssignments.Count);
    }
}
