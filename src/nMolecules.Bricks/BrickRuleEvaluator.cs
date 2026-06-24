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
            var rules = policy.Rules;
            var permissionRules = BuildPermissionRuleIndex(rules, out var requirements);
            var rolesByElement = BuildRoleMap(roleSets);
            var violations = new List<BrickViolation>();

            foreach (var dependency in dependencyList)
            {
                EvaluatePermission(policy, permissionRules, dependency, rolesByElement, violations);
            }

            for (var i = 0; i < requirements.Count; i++)
            {
                EvaluateRequirement(requirements[i], dependencyList, roleSets, rolesByElement, violations);
            }

            return violations;
        }

        private static Dictionary<(BrickScope Scope, RoleId SourceRole, RoleId TargetRole), List<IndexedBrickRule>> BuildPermissionRuleIndex(
            IReadOnlyList<BrickRule> rules,
            out IReadOnlyList<BrickRule> requirements)
        {
            var permissionRules = new Dictionary<(BrickScope Scope, RoleId SourceRole, RoleId TargetRole), List<IndexedBrickRule>>();
            var requirementRules = new List<BrickRule>();

            for (var i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                if (rule.Decision == BrickDecision.Require)
                {
                    requirementRules.Add(rule);
                    continue;
                }

                var key = (rule.Scope, rule.SourceRole, rule.TargetRole);
                if (!permissionRules.TryGetValue(key, out var indexedRules))
                {
                    indexedRules = new List<IndexedBrickRule>();
                    permissionRules.Add(key, indexedRules);
                }

                indexedRules.Add(new IndexedBrickRule(rule, i));
            }

            requirements = requirementRules;
            return permissionRules;
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
            IReadOnlyDictionary<(BrickScope Scope, RoleId SourceRole, RoleId TargetRole), List<IndexedBrickRule>> permissionRules,
            BrickDependency dependency,
            IReadOnlyDictionary<BrickElementId, IReadOnlyList<RoleId>> rolesByElement,
            ICollection<BrickViolation> violations)
        {
            var sourceRoles = GetRoles(rolesByElement, dependency.Source);
            var targetRoles = GetRoles(rolesByElement, dependency.Target);
            var hasApplicableRule = false;
            var highestPriority = int.MinValue;
            BrickRule deniedByRule = default;
            var deniedByRuleIndex = int.MaxValue;
            var hasDeniedRuleAtHighestPriority = false;

            for (var sourceRoleIndex = 0; sourceRoleIndex < sourceRoles.Count; sourceRoleIndex++)
            {
                for (var targetRoleIndex = 0; targetRoleIndex < targetRoles.Count; targetRoleIndex++)
                {
                    var key = (dependency.Scope, sourceRoles[sourceRoleIndex], targetRoles[targetRoleIndex]);
                    if (!permissionRules.TryGetValue(key, out var indexedRules))
                    {
                        continue;
                    }

                    for (var ruleIndex = 0; ruleIndex < indexedRules.Count; ruleIndex++)
                    {
                        var indexedRule = indexedRules[ruleIndex];
                        var rule = indexedRule.Rule;
                        if (!hasApplicableRule || rule.Priority > highestPriority)
                        {
                            hasApplicableRule = true;
                            highestPriority = rule.Priority;
                            deniedByRule = rule.Decision == BrickDecision.Deny ? rule : default;
                            deniedByRuleIndex = rule.Decision == BrickDecision.Deny ? indexedRule.Index : int.MaxValue;
                            hasDeniedRuleAtHighestPriority = rule.Decision == BrickDecision.Deny;
                            continue;
                        }

                        if (rule.Priority != highestPriority || rule.Decision != BrickDecision.Deny)
                        {
                            continue;
                        }

                        if (hasDeniedRuleAtHighestPriority && indexedRule.Index >= deniedByRuleIndex)
                        {
                            continue;
                        }

                        deniedByRule = rule;
                        deniedByRuleIndex = indexedRule.Index;
                        hasDeniedRuleAtHighestPriority = true;
                    }
                }
            }

            if (!hasApplicableRule)
            {
                if (policy.DefaultDecision == BrickPermissionDefault.Deny)
                {
                    violations.Add(CreateDependencyViolation(dependency, sourceRoles, targetRoles));
                }

                return;
            }

            if (!hasDeniedRuleAtHighestPriority || deniedByRule.RuleId.IsEmpty)
            {
                return;
            }

            violations.Add(CreateDependencyViolation(dependency, sourceRoles, targetRoles, deniedByRule));
        }

        private static void EvaluateRequirement(
            BrickRule requirement,
            IReadOnlyList<BrickDependency> dependencies,
            IReadOnlyList<BrickResolvedRoles> roleSets,
            IReadOnlyDictionary<BrickElementId, IReadOnlyList<RoleId>> rolesByElement,
            ICollection<BrickViolation> violations)
        {
            for (var roleSetIndex = 0; roleSetIndex < roleSets.Count; roleSetIndex++)
            {
                var roleSet = roleSets[roleSetIndex];
                if (!roleSet.EffectiveRoles.Contains(requirement.SourceRole))
                {
                    continue;
                }

                var isSatisfied = false;
                for (var dependencyIndex = 0; dependencyIndex < dependencies.Count; dependencyIndex++)
                {
                    var dependency = dependencies[dependencyIndex];
                    if (dependency.Scope == requirement.Scope &&
                        dependency.Source.Id == roleSet.Element.Id &&
                        GetRoles(rolesByElement, dependency.Target).Contains(requirement.TargetRole))
                    {
                        isSatisfied = true;
                        break;
                    }
                }

                if (!isSatisfied)
                {
                    violations.Add(CreateRequirementViolation(requirement, roleSet.Element));
                }
            }
        }

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

        private readonly struct IndexedBrickRule
        {
            public IndexedBrickRule(BrickRule rule, int index)
            {
                Rule = rule;
                Index = index;
            }

            public BrickRule Rule { get; }
            public int Index { get; }
        }
    }
}
