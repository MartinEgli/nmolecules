using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Resolves role assignments and turns unresolved role state into Bricks violations.
    /// </summary>
    /// <remarks>
    /// Use this resolver after collecting assignments from attributes, policies, packages or
    /// conventions. It applies suppressions, exclusive role precedence and incompatible combination
    /// checks before dependency rules are evaluated.
    /// </remarks>
    public static class BrickRoleResolver
    {
        /// <summary>
        /// Resolves candidate assignments for one element.
        /// </summary>
        /// <param name="element">The element whose roles should be resolved.</param>
        /// <param name="assignments">Candidate assignments from all providers.</param>
        /// <param name="combinationRules">Combination rules used for exclusive-role resolution.</param>
        /// <returns>The resolved role state for the element.</returns>
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
            var suppressingAssignments = candidates
                .Where(candidate => candidate.Behavior == BrickAssignmentBehavior.Suppress)
                .ToArray();

            foreach (var suppressingAssignment in suppressingAssignments)
            {
                foreach (var candidate in candidates)
                {
                    if (candidate.Behavior == BrickAssignmentBehavior.Suppress ||
                        candidate.RoleId != suppressingAssignment.RoleId)
                    {
                        continue;
                    }

                    if (suppressingAssignment.Precedence.CompareTo(candidate.Precedence) >= 0)
                    {
                        suppressed.Add(candidate);
                    }
                }
            }

            for (var leftIndex = 0; leftIndex < candidates.Length; leftIndex++)
            {
                for (var rightIndex = leftIndex + 1; rightIndex < candidates.Length; rightIndex++)
                {
                    var left = candidates[leftIndex];
                    var right = candidates[rightIndex];
                    if (left.Behavior == BrickAssignmentBehavior.Suppress ||
                        right.Behavior == BrickAssignmentBehavior.Suppress ||
                        suppressed.Contains(left) ||
                        suppressed.Contains(right))
                    {
                        continue;
                    }

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
                .Where(candidate => candidate.Behavior != BrickAssignmentBehavior.Suppress)
                .Where(candidate => !suppressed.Contains(candidate))
                .Where(candidate => !conflicted.Contains(candidate))
                .ToArray();

            return new BrickResolvedRoles(element, candidates, applied, suppressed.ToArray(), conflicts);
        }

        /// <summary>
        /// Finds violations for incompatible effective role combinations.
        /// </summary>
        /// <param name="resolvedRoles">The resolved role state to inspect.</param>
        /// <param name="combinationRules">Combination rules that mark role pairs as incompatible.</param>
        /// <returns>Role-combination violations for incompatible effective role pairs.</returns>
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
                            ResolveRuleId(rule),
                            ruleName: rule.Name,
                            resolvedSourceRoles: new[] { left, right });
                    }
                }
            }
        }

        /// <summary>
        /// Finds violations for role assignments that could not be resolved by precedence.
        /// </summary>
        /// <param name="resolvedRoles">The resolved role state to inspect.</param>
        /// <returns>Role-resolution violations for unresolved conflicts.</returns>
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

        private static RuleId? ResolveRuleId(BrickRoleCombinationRule rule) =>
            string.IsNullOrWhiteSpace(rule.Name) ? (RuleId?)null : RuleId.From(rule.Name);

        private static string FormatResolutionViolation(BrickRoleConflict conflict) =>
            $"Role assignments '{conflict.FirstAssignment.RoleId}' and '{conflict.SecondAssignment.RoleId}' could not be resolved. {conflict.Reason}";
    }
}
