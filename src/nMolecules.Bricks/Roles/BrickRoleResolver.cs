using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents role resolver data used by role dimensions, assignments, resolution, conflicts, and role
/// packs.
/// </summary>
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

        public static IEnumerable<BrickViolation> FindResolutionViolations(BrickResolvedRoles resolvedRoles)
        {
            if (resolvedRoles is null)
            {
                yield break;
            }

            foreach (var conflict in resolvedRoles.Conflicts)
            {
                yield return new BrickViolation(
                    BrickViolationKind.RoleResolution,
                    resolvedRoles.Element,
                    FormatResolutionViolation(conflict),
                    BrickSeverity.Warning,
                    BrickViolationState.Active,
                    resolvedSourceRoles: new[] { conflict.FirstAssignment.RoleId, conflict.SecondAssignment.RoleId });
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

        private static string FormatResolutionViolation(BrickRoleConflict conflict) =>
            $"Role assignments '{conflict.FirstAssignment.RoleId}' and '{conflict.SecondAssignment.RoleId}' could not be resolved. {conflict.Reason}";
    }
}
