using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    public readonly struct BrickAssignmentPrecedence : IComparable<BrickAssignmentPrecedence>, IEquatable<BrickAssignmentPrecedence>
    {
        public BrickAssignmentPrecedence(BrickAssignmentSpecificity specificity, BrickAssignmentAuthority authority)
        {
            Specificity = specificity;
            Authority = authority;
        }

        public BrickAssignmentSpecificity Specificity { get; }
        public BrickAssignmentAuthority Authority { get; }

        public int CompareTo(BrickAssignmentPrecedence other)
        {
            var specificityComparison = Specificity.CompareTo(other.Specificity);
            return specificityComparison != 0 ? specificityComparison : Authority.CompareTo(other.Authority);
        }

        public bool IsStrongerThan(BrickAssignmentPrecedence other) => CompareTo(other) > 0;
        public bool Equals(BrickAssignmentPrecedence other) => Specificity == other.Specificity && Authority == other.Authority;
        public override bool Equals(object obj) => obj is BrickAssignmentPrecedence other && Equals(other);
        public override int GetHashCode() => Specificity.GetHashCode() ^ Authority.GetHashCode();
        public static bool operator ==(BrickAssignmentPrecedence left, BrickAssignmentPrecedence right) => left.Equals(right);
        public static bool operator !=(BrickAssignmentPrecedence left, BrickAssignmentPrecedence right) => !left.Equals(right);
    }

    public sealed class BrickRoleAssignment
    {
        public BrickRoleAssignment(
            BrickElementSelector? selector,
            RoleId roleId,
            BrickAssignmentMode mode,
            BrickAssignmentSource source,
            BrickAssignmentPrecedence precedence,
            BrickAssignmentBehavior behavior,
            string reason = null)
        {
            Selector = selector ?? default;
            RoleId = roleId;
            Mode = mode;
            Source = source;
            Precedence = precedence;
            Behavior = behavior;
            Reason = reason;
        }

        public BrickElementSelector Selector { get; }
        public RoleId RoleId { get; }
        public BrickAssignmentMode Mode { get; }
        public BrickAssignmentSource Source { get; }
        public BrickAssignmentPrecedence Precedence { get; }
        public BrickAssignmentBehavior Behavior { get; }
        public string Reason { get; }
    }

    public sealed class BrickRoleConflict
    {
        public BrickRoleConflict(BrickRoleAssignment firstAssignment, BrickRoleAssignment secondAssignment, string reason)
        {
            FirstAssignment = firstAssignment ?? throw new ArgumentNullException(nameof(firstAssignment));
            SecondAssignment = secondAssignment ?? throw new ArgumentNullException(nameof(secondAssignment));
            Reason = reason ?? string.Empty;
        }

        public BrickRoleAssignment FirstAssignment { get; }
        public BrickRoleAssignment SecondAssignment { get; }
        public string Reason { get; }
    }

    public sealed class BrickResolvedRoles
    {
        public BrickResolvedRoles(
            BrickElement element,
            IEnumerable<BrickRoleAssignment> candidateAssignments,
            IEnumerable<BrickRoleAssignment> appliedAssignments,
            IEnumerable<BrickRoleAssignment> suppressedAssignments,
            IEnumerable<BrickRoleConflict> conflicts)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            CandidateAssignments = (candidateAssignments ?? Enumerable.Empty<BrickRoleAssignment>()).ToArray();
            AppliedAssignments = (appliedAssignments ?? Enumerable.Empty<BrickRoleAssignment>()).ToArray();
            SuppressedAssignments = (suppressedAssignments ?? Enumerable.Empty<BrickRoleAssignment>()).ToArray();
            Conflicts = (conflicts ?? Enumerable.Empty<BrickRoleConflict>()).ToArray();
            EffectiveRoles = AppliedAssignments.Select(assignment => assignment.RoleId).Distinct().ToArray();
        }

        public BrickElement Element { get; }
        public IReadOnlyList<BrickRoleAssignment> CandidateAssignments { get; }
        public IReadOnlyList<RoleId> EffectiveRoles { get; }
        public IReadOnlyList<BrickRoleAssignment> AppliedAssignments { get; }
        public IReadOnlyList<BrickRoleAssignment> SuppressedAssignments { get; }
        public IReadOnlyList<BrickRoleConflict> Conflicts { get; }
        public bool HasConflicts => Conflicts.Count > 0;
    }

    public sealed class BrickResolutionTrace
    {
        public BrickResolutionTrace(
            BrickElement element,
            IEnumerable<BrickRoleAssignment> candidates,
            IEnumerable<RoleId> resolvedRoles,
            IEnumerable<string> decisions,
            bool hasConflict)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            Candidates = (candidates ?? Enumerable.Empty<BrickRoleAssignment>()).ToArray();
            ResolvedRoles = (resolvedRoles ?? Enumerable.Empty<RoleId>()).ToArray();
            Decisions = (decisions ?? Enumerable.Empty<string>()).Select(decision => decision ?? string.Empty).ToArray();
            HasConflict = hasConflict;
        }

        public BrickElement Element { get; }
        public IReadOnlyList<BrickRoleAssignment> Candidates { get; }
        public IReadOnlyList<RoleId> ResolvedRoles { get; }
        public IReadOnlyList<string> Decisions { get; }
        public bool HasConflict { get; }
    }
}
