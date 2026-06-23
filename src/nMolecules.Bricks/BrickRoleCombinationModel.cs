using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
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

    public sealed class BrickRoleCombinationRule
    {
        public BrickRoleCombinationRule(
            string name,
            BrickRoleSelector? leftRoles,
            BrickRoleSelector? rightRoles,
            BrickCombinationKind kind,
            string reason = null)
        {
            Name = name ?? string.Empty;
            LeftRoles = leftRoles ?? default;
            RightRoles = rightRoles ?? default;
            Kind = kind;
            Reason = reason;
        }

        public string Name { get; }
        public BrickRoleSelector LeftRoles { get; }
        public BrickRoleSelector RightRoles { get; }
        public BrickCombinationKind Kind { get; }
        public string Reason { get; }

        public bool Matches(RoleId left, RoleId right) =>
            (LeftRoles.Matches(left) && RightRoles.Matches(right)) ||
            (LeftRoles.Matches(right) && RightRoles.Matches(left));
    }

    public static class BrickRoleResolver
    {
        public static BrickResolvedRoles Resolve(
            BrickElement element,
            IEnumerable<BrickRoleAssignment> assignments,
            IEnumerable<BrickRoleCombinationRule> combinationRules)
        {
            var candidates = (assignments ?? Enumerable.Empty<BrickRoleAssignment>()).ToArray();
            var rules = (combinationRules ?? Enumerable.Empty<BrickRoleCombinationRule>()).ToArray();
            var suppressed = new HashSet<BrickRoleAssignment>();
            var conflicted = new HashSet<BrickRoleAssignment>();
            var conflicts = new List<BrickRoleConflict>();

            for (var leftIndex = 0; leftIndex < candidates.Length; leftIndex++)
            {
                for (var rightIndex = leftIndex + 1; rightIndex < candidates.Length; rightIndex++)
                {
                    var left = candidates[leftIndex];
                    var right = candidates[rightIndex];
                    var rule = rules.FirstOrDefault(candidate => candidate.Kind == BrickCombinationKind.Exclusive && candidate.Matches(left.RoleId, right.RoleId));
                    if (rule is null)
                    {
                        continue;
                    }

                    var comparison = left.Precedence.CompareTo(right.Precedence);
                    if (comparison < 0)
                    {
                        suppressed.Add(left);
                    }
                    else if (comparison > 0)
                    {
                        suppressed.Add(right);
                    }
                    else
                    {
                        conflicted.Add(left);
                        conflicted.Add(right);
                        conflicts.Add(new BrickRoleConflict(left, right, FormatConflictReason(rule)));
                    }
                }
            }

            var applied = candidates
                .Where(candidate => !suppressed.Contains(candidate))
                .Where(candidate => !conflicted.Contains(candidate))
                .ToArray();

            return new BrickResolvedRoles(element, candidates, applied, suppressed.ToArray(), conflicts);
        }

        public static IEnumerable<BrickViolation> FindCombinationViolations(
            BrickResolvedRoles resolvedRoles,
            IEnumerable<BrickRoleCombinationRule> combinationRules)
        {
            if (resolvedRoles is null)
            {
                yield break;
            }

            var rules = (combinationRules ?? Enumerable.Empty<BrickRoleCombinationRule>())
                .Where(rule => rule.Kind == BrickCombinationKind.Incompatible)
                .ToArray();

            var roles = resolvedRoles.EffectiveRoles.ToArray();
            for (var leftIndex = 0; leftIndex < roles.Length; leftIndex++)
            {
                for (var rightIndex = leftIndex + 1; rightIndex < roles.Length; rightIndex++)
                {
                    var left = roles[leftIndex];
                    var right = roles[rightIndex];
                    foreach (var rule in rules.Where(rule => rule.Matches(left, right)))
                    {
                        yield return new BrickViolation(
                            BrickViolationKind.RoleCombination,
                            resolvedRoles.Element,
                            FormatCombinationViolation(rule, left, right),
                            BrickSeverity.Warning,
                            BrickViolationState.Active,
                            ruleName: rule.Name,
                            resolvedSourceRoles: new[] { left, right });
                    }
                }
            }
        }

        private static string FormatConflictReason(BrickRoleCombinationRule rule)
        {
            var reason = string.IsNullOrWhiteSpace(rule.Reason) ? "Exclusive role assignments have equal precedence." : rule.Reason;
            return string.IsNullOrWhiteSpace(rule.Name) ? reason : $"{rule.Name}: {reason}";
        }

        private static string FormatCombinationViolation(BrickRoleCombinationRule rule, RoleId left, RoleId right)
        {
            var reason = string.IsNullOrWhiteSpace(rule.Reason) ? "Role combination is incompatible." : rule.Reason;
            return $"Role combination '{left}' + '{right}' violates '{rule.Name}'. {reason}";
        }
    }
}
