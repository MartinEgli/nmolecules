using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Value object that selects Bricks elements by kind, name pattern and optional assembly name.
    /// </summary>
    /// <remarks>
    /// Use selectors in role assignments, aliases and policy documents when a rule should apply to
    /// a group of elements instead of a single exact element id. Patterns support exact values,
    /// <c>*</c> for all values and prefix patterns such as <c>Sales.*</c>.
    /// </remarks>
    public readonly struct BrickElementSelector : IEquatable<BrickElementSelector>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickElementSelector"/> struct.
        /// </summary>
        /// <param name="kind">The element kind to match, or <see cref="BrickElementKind.Unknown"/> for any kind.</param>
        /// <param name="pattern">The element id, display name or full-name pattern.</param>
        /// <param name="assemblyName">The optional assembly name that must match.</param>
        public BrickElementSelector(BrickElementKind kind, string pattern, string assemblyName = null)
        {
            Kind = kind;
            Pattern = pattern ?? string.Empty;
            AssemblyName = assemblyName;
        }

        /// <summary>
        /// Gets the element kind that must match.
        /// </summary>
        public BrickElementKind Kind { get; }

        /// <summary>
        /// Gets the element id, display name or full-name pattern.
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// Gets the optional assembly name constraint.
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        /// Determines whether the selector matches the supplied element.
        /// </summary>
        /// <param name="element">The element to test.</param>
        /// <returns><c>true</c> when kind, assembly and pattern match; otherwise <c>false</c>.</returns>
        public bool Matches(BrickElement element)
        {
            if (element is null)
            {
                return false;
            }

            if (Kind != BrickElementKind.Unknown && element.Kind != Kind)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(AssemblyName) && !string.Equals(AssemblyName, element.AssemblyName, StringComparison.Ordinal))
            {
                return false;
            }

            return MatchesPattern(Pattern, element.FullName) ||
                   MatchesPattern(Pattern, element.DisplayName) ||
                   MatchesPattern(Pattern, element.Id.Value);
        }

        /// <summary>
        /// Determines whether this selector equals another selector by kind, pattern and assembly.
        /// </summary>
        /// <param name="other">The other selector.</param>
        /// <returns><c>true</c> when both selectors are equal; otherwise <c>false</c>.</returns>
        public bool Equals(BrickElementSelector other) =>
            Kind == other.Kind &&
            string.Equals(Pattern, other.Pattern, StringComparison.Ordinal) &&
            string.Equals(AssemblyName, other.AssemblyName, StringComparison.Ordinal);

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is BrickElementSelector other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Kind.GetHashCode() ^ StringComparer.Ordinal.GetHashCode(Pattern ?? string.Empty) ^ StringComparer.Ordinal.GetHashCode(AssemblyName ?? string.Empty);

        /// <summary>
        /// Compares two selectors for equality.
        /// </summary>
        public static bool operator ==(BrickElementSelector left, BrickElementSelector right) => left.Equals(right);

        /// <summary>
        /// Compares two selectors for inequality.
        /// </summary>
        public static bool operator !=(BrickElementSelector left, BrickElementSelector right) => !left.Equals(right);

        private static bool MatchesPattern(string pattern, string value)
        {
            if (string.IsNullOrWhiteSpace(pattern) || string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (pattern == "*")
            {
                return true;
            }

            if (pattern.EndsWith("*", StringComparison.Ordinal))
            {
                return value.StartsWith(pattern.Substring(0, pattern.Length - 1), StringComparison.Ordinal);
            }

            return string.Equals(pattern, value, StringComparison.Ordinal);
        }
    }
}
