using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Value object that represents assignment precedence data for role dimensions, assignments, resolution,
/// conflicts, and role packs.
/// </summary>
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
}
