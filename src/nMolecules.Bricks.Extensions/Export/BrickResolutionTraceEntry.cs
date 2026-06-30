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
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
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

        /// <summary>
        /// Gets the Element value used by Bricks developer tooling.
        /// </summary>
        public BrickElement Element { get; }
        /// <summary>
        /// Gets a value indicating whether Candidate Roles applies.
        /// </summary>
        public IReadOnlyList<RoleId> CandidateRoles { get; }
        /// <summary>
        /// Gets the Resolved Roles value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<RoleId> ResolvedRoles { get; }
        /// <summary>
        /// Gets the Decisions value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<string> Decisions { get; }
        /// <summary>
        /// Gets a value indicating whether Has Conflict applies.
        /// </summary>
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
