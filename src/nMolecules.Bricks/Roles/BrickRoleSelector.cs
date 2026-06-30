using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Value object that matches roles by exact id, wildcard or namespace-like prefix pattern.
    /// </summary>
    /// <remarks>
    /// Use selectors in role combination rules and policy documents when one rule should apply to
    /// one role, all roles or a role family such as <c>DDD.*</c>.
    /// </remarks>
    public readonly struct BrickRoleSelector : IEquatable<BrickRoleSelector>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickRoleSelector"/> struct.
        /// </summary>
        /// <param name="pattern">The exact role id, <c>*</c>, or prefix pattern ending with <c>.*</c>.</param>
        public BrickRoleSelector(string pattern)
        {
            Pattern = pattern ?? string.Empty;
        }

        /// <summary>
        /// Gets the role selector pattern.
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// Creates a selector from a pattern string.
        /// </summary>
        /// <param name="pattern">The exact role id, <c>*</c>, or prefix pattern ending with <c>.*</c>.</param>
        /// <returns>A role selector.</returns>
        public static BrickRoleSelector From(string pattern) => new BrickRoleSelector(pattern);

        /// <summary>
        /// Determines whether the selector matches the supplied role id.
        /// </summary>
        /// <param name="roleId">The role id to test.</param>
        /// <returns><c>true</c> when the role id matches the selector pattern; otherwise <c>false</c>.</returns>
        public bool Matches(RoleId roleId)
        {
            if (string.IsNullOrWhiteSpace(Pattern))
            {
                return false;
            }

            if (Pattern == "*")
            {
                return true;
            }

            if (Pattern.EndsWith(".*", StringComparison.Ordinal))
            {
                var prefix = Pattern.Substring(0, Pattern.Length - 1);
                return roleId.Value.StartsWith(prefix, StringComparison.Ordinal);
            }

            return string.Equals(Pattern, roleId.Value, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether this selector equals another selector by ordinal pattern comparison.
        /// </summary>
        /// <param name="other">The other selector.</param>
        /// <returns><c>true</c> when both patterns are equal; otherwise <c>false</c>.</returns>
        public bool Equals(BrickRoleSelector other) => string.Equals(Pattern, other.Pattern, StringComparison.Ordinal);

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is BrickRoleSelector other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Pattern ?? string.Empty);

        /// <summary>
        /// Compares two role selectors for equality.
        /// </summary>
        public static bool operator ==(BrickRoleSelector left, BrickRoleSelector right) => left.Equals(right);

        /// <summary>
        /// Compares two role selectors for inequality.
        /// </summary>
        public static bool operator !=(BrickRoleSelector left, BrickRoleSelector right) => !left.Equals(right);
    }
}
