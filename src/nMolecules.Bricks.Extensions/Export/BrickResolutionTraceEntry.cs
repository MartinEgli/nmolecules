using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents resolution trace entry data used by exportable role maps, dependency graphs, and resolution
/// traces.
/// </summary>
public sealed class BrickResolutionTraceEntry
    {
        public BrickResolutionTraceEntry(
            BrickElement element,
            IEnumerable<RoleId> candidateRoles,
            IEnumerable<RoleId> resolvedRoles,
            IEnumerable<string> decisions,
            bool hasConflict)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            CandidateRoles = (candidateRoles ?? Enumerable.Empty<RoleId>())
                .Distinct()
                .OrderBy(roleId => roleId.Value, StringComparer.Ordinal)
                .ToArray();
            ResolvedRoles = (resolvedRoles ?? Enumerable.Empty<RoleId>())
                .Distinct()
                .OrderBy(roleId => roleId.Value, StringComparer.Ordinal)
                .ToArray();
            Decisions = (decisions ?? Enumerable.Empty<string>())
                .Select(decision => decision ?? string.Empty)
                .ToArray();
            HasConflict = hasConflict;
        }

        public BrickElement Element { get; }
        public IReadOnlyList<RoleId> CandidateRoles { get; }
        public IReadOnlyList<RoleId> ResolvedRoles { get; }
        public IReadOnlyList<string> Decisions { get; }
        public bool HasConflict { get; }

        internal static BrickResolutionTraceEntry FromTrace(BrickResolutionTrace trace) =>
            new BrickResolutionTraceEntry(
                trace.Element,
                trace.Candidates.Select(candidate => candidate.RoleId),
                trace.ResolvedRoles,
                trace.Decisions,
                trace.HasConflict);
    }
}
