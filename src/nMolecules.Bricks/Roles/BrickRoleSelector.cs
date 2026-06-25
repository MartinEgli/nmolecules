using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Value object that represents role selector data for role dimensions, assignments, resolution, conflicts,
/// and role packs.
/// </summary>
public readonly struct BrickRoleSelector : IEquatable<BrickRoleSelector>
    {
        public BrickRoleSelector(string pattern)
        {
            Pattern = pattern ?? string.Empty;
        }

        public string Pattern { get; }

        public static BrickRoleSelector From(string pattern) => new BrickRoleSelector(pattern);

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

        public bool Equals(BrickRoleSelector other) => string.Equals(Pattern, other.Pattern, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is BrickRoleSelector other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Pattern ?? string.Empty);
        public static bool operator ==(BrickRoleSelector left, BrickRoleSelector right) => left.Equals(right);
        public static bool operator !=(BrickRoleSelector left, BrickRoleSelector right) => !left.Equals(right);
    }
}
