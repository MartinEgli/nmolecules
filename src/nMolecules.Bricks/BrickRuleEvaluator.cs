using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    public static class BrickRuleEvaluator
    {
        public static IReadOnlyList<BrickViolation> Evaluate(
            BrickPolicy policy,
            IEnumerable<BrickDependency> dependencies,
            IEnumerable<BrickResolvedRoles> resolvedRoles)
        {
            if (policy is null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            if (policy.Enforcement == BrickEnforcementMode.Disabled)
            {
                return Array.Empty<BrickViolation>();
            }

            var dependencyList = (dependencies ?? Enumerable.Empty<BrickDependency>()).ToArray();
            var roleSets = (resolvedRoles ?? Enumerable.Empty<BrickResolvedRoles>()).ToArray();
            var rolesByElement = BuildRoleMap(roleSets);
            var violations = new List<BrickViolation>();

            foreach (var dependency in dependencyList)
            {
                EvaluatePermission(policy, dependency, rolesByElement, violations);
            }

            foreach (var requirement in policy.Rules.Where(rule => rule.Decision == BrickDecision.Require))
            {
                EvaluateRequirement(requirement, dependencyList, roleSets, rolesByElement, violations);
            }

            return violations;
        }

        private static Dictionary<BrickElementId, IReadOnlyList<RoleId>> BuildRoleMap(IEnumerable<BrickResolvedRoles> roleSets)
        {
            var rolesByElement = new Dictionary<BrickElementId, IReadOnlyList<RoleId>>();
            foreach (var roleSet in roleSets)
            {
                if (!rolesByElement.ContainsKey(roleSet.Element.Id))
                {
                    rolesByElement.Add(roleSet.Element.Id, roleSet.EffectiveRoles);
                }
            }

            return rolesByElement;
        }

        private static void EvaluatePermission(
            BrickPolicy policy,
            BrickDependency dependency,
            IReadOnlyDictionary<BrickElementId, IReadOnlyList<RoleId>> rolesByElement,
            ICollection<BrickViolation> violations)
        {
            var sourceRoles = GetRoles(rolesByElement, dependency.Source);
            var targetRoles = GetRoles(rolesByElement, dependency.Target);
            var applicableRules = policy.Rules
                .Where(rule => rule.Decision != BrickDecision.Require)
                .Where(rule => RuleMatches(rule, dependency, sourceRoles, targetRoles))
                .ToArray();

            if (applicableRules.Length == 0)
            {
                if (policy.DefaultDecision == BrickPermissionDefault.Deny)
                {
                    violations.Add(CreateDependencyViolation(dependency, sourceRoles, targetRoles));
                }

                return;
            }

            var highestPriority = applicableRules.Max(rule => rule.Priority);
            var deniedByRule = applicableRules
                .Where(rule => rule.Priority == highestPriority)
                .FirstOrDefault(rule => rule.Decision == BrickDecision.Deny);

            if (deniedByRule.RuleId.IsEmpty)
            {
                return;
            }

            violations.Add(CreateDependencyViolation(dependency, sourceRoles, targetRoles, deniedByRule));
        }

        private static void EvaluateRequirement(
            BrickRule requirement,
            IEnumerable<BrickDependency> dependencies,
            IEnumerable<BrickResolvedRoles> roleSets,
            IReadOnlyDictionary<BrickElementId, IReadOnlyList<RoleId>> rolesByElement,
            ICollection<BrickViolation> violations)
        {
            foreach (var roleSet in roleSets.Where(candidate => candidate.EffectiveRoles.Contains(requirement.SourceRole)))
            {
                var isSatisfied = dependencies.Any(dependency =>
                    dependency.Scope == requirement.Scope &&
                    dependency.Source.Id == roleSet.Element.Id &&
                    GetRoles(rolesByElement, dependency.Target).Contains(requirement.TargetRole));

                if (!isSatisfied)
                {
                    violations.Add(CreateRequirementViolation(requirement, roleSet.Element));
                }
            }
        }

        private static bool RuleMatches(
            BrickRule rule,
            BrickDependency dependency,
            IReadOnlyCollection<RoleId> sourceRoles,
            IReadOnlyCollection<RoleId> targetRoles) =>
            rule.Scope == dependency.Scope &&
            sourceRoles.Contains(rule.SourceRole) &&
            targetRoles.Contains(rule.TargetRole);

        private static IReadOnlyList<RoleId> GetRoles(
            IReadOnlyDictionary<BrickElementId, IReadOnlyList<RoleId>> rolesByElement,
            BrickElement element)
        {
            IReadOnlyList<RoleId> roles;
            return rolesByElement.TryGetValue(element.Id, out roles) ? roles : Array.Empty<RoleId>();
        }

        private static BrickViolation CreateDependencyViolation(
            BrickDependency dependency,
            IReadOnlyList<RoleId> sourceRoles,
            IReadOnlyList<RoleId> targetRoles,
            BrickRule? rule = null)
        {
            var hasRule = rule.HasValue;
            var message = hasRule
                ? $"Dependency from '{dependency.Source.DisplayName}' to '{dependency.Target.DisplayName}' violates rule '{rule.Value.Name}'."
                : $"Dependency from '{dependency.Source.DisplayName}' to '{dependency.Target.DisplayName}' is not covered by an allow rule. Default decision Deny applies.";

            return new BrickViolation(
                BrickViolationKind.DependencyRule,
                dependency.Source,
                message,
                hasRule ? rule.Value.Severity : BrickSeverity.Error,
                BrickViolationState.Active,
                hasRule ? rule.Value.RuleId : (RuleId?)null,
                hasRule ? rule.Value.Name : null,
                dependency.Target,
                sourceRoles,
                targetRoles,
                dependency.KindId,
                dependency.Scope,
                dependency.Layer,
                dependency.EvidenceLevel);
        }

        private static BrickViolation CreateRequirementViolation(BrickRule requirement, BrickElement source) =>
            new BrickViolation(
                BrickViolationKind.RequiredDependency,
                source,
                $"Element '{source.DisplayName}' does not satisfy required dependency rule '{requirement.Name}'.",
                requirement.Severity,
                BrickViolationState.Active,
                requirement.RuleId,
                requirement.Name,
                resolvedSourceRoles: new[] { requirement.SourceRole },
                resolvedTargetRoles: new[] { requirement.TargetRole },
                scope: requirement.Scope);
    }
}
