using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Value object that represents element selector data for adoption workflows, baselines, suppressions, and
/// violation lifecycle state.
/// </summary>
public readonly struct BrickElementSelector : IEquatable<BrickElementSelector>
    {
        public BrickElementSelector(BrickElementKind kind, string pattern, string assemblyName = null)
        {
            Kind = kind;
            Pattern = pattern ?? string.Empty;
            AssemblyName = assemblyName;
        }

        public BrickElementKind Kind { get; }
        public string Pattern { get; }
        public string AssemblyName { get; }

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

        public bool Equals(BrickElementSelector other) =>
            Kind == other.Kind &&
            string.Equals(Pattern, other.Pattern, StringComparison.Ordinal) &&
            string.Equals(AssemblyName, other.AssemblyName, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is BrickElementSelector other && Equals(other);
        public override int GetHashCode() => Kind.GetHashCode() ^ StringComparer.Ordinal.GetHashCode(Pattern ?? string.Empty) ^ StringComparer.Ordinal.GetHashCode(AssemblyName ?? string.Empty);
        public static bool operator ==(BrickElementSelector left, BrickElementSelector right) => left.Equals(right);
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
