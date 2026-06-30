using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Value object that orders role assignments by specificity and authority.
    /// </summary>
    /// <remarks>
    /// Higher precedence assignments can suppress or override lower precedence assignments during
    /// role resolution.
    /// </remarks>
    public readonly struct BrickAssignmentPrecedence : IComparable<BrickAssignmentPrecedence>, IEquatable<BrickAssignmentPrecedence>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickAssignmentPrecedence"/> struct.
        /// </summary>
        /// <param name="specificity">How specific the assignment is.</param>
        /// <param name="authority">How authoritative the assignment source is.</param>
        public BrickAssignmentPrecedence(BrickAssignmentSpecificity specificity, BrickAssignmentAuthority authority)
        {
            Specificity = specificity;
            Authority = authority;
        }

        /// <summary>
        /// Gets how specific the assignment is.
        /// </summary>
        public BrickAssignmentSpecificity Specificity { get; }

        /// <summary>
        /// Gets how authoritative the assignment source is.
        /// </summary>
        public BrickAssignmentAuthority Authority { get; }

        /// <summary>
        /// Compares this precedence with another precedence.
        /// </summary>
        /// <param name="other">The precedence to compare with.</param>
        /// <returns>A positive value when this precedence is stronger, zero when equal, otherwise a negative value.</returns>
        public int CompareTo(BrickAssignmentPrecedence other)
        {
            var specificityComparison = Specificity.CompareTo(other.Specificity);
            return specificityComparison != 0 ? specificityComparison : Authority.CompareTo(other.Authority);
        }

        /// <summary>
        /// Determines whether this precedence is stronger than another precedence.
        /// </summary>
        /// <param name="other">The precedence to compare with.</param>
        /// <returns><c>true</c> when this precedence is stronger; otherwise <c>false</c>.</returns>
        public bool IsStrongerThan(BrickAssignmentPrecedence other) => CompareTo(other) > 0;

        /// <summary>
        /// Determines whether this precedence equals another precedence.
        /// </summary>
        /// <param name="other">The precedence to compare with.</param>
        /// <returns><c>true</c> when specificity and authority are equal; otherwise <c>false</c>.</returns>
        public bool Equals(BrickAssignmentPrecedence other) => Specificity == other.Specificity && Authority == other.Authority;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is BrickAssignmentPrecedence other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Specificity.GetHashCode() ^ Authority.GetHashCode();

        /// <summary>
        /// Compares two precedences for equality.
        /// </summary>
        public static bool operator ==(BrickAssignmentPrecedence left, BrickAssignmentPrecedence right) => left.Equals(right);

        /// <summary>
        /// Compares two precedences for inequality.
        /// </summary>
        public static bool operator !=(BrickAssignmentPrecedence left, BrickAssignmentPrecedence right) => !left.Equals(right);
    }
}
