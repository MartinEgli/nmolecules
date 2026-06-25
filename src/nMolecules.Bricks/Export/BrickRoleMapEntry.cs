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

        public BrickElement Element { get; }
        public IReadOnlyList<RoleId> EffectiveRoles { get; }
        public bool HasConflicts { get; }
        public int AppliedAssignmentCount { get; }
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
