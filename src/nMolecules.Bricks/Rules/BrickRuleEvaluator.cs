using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Evaluates deterministic Bricks policy rules against observed dependencies and resolved roles.
    /// </summary>
    /// <remarks>
    /// The evaluator is the enforcement source of truth for dependency permissions and required dependencies.
    /// It does not perform AI interpretation, policy mutation, suppression creation, or baseline creation.
    ///
    /// Example: see
    /// <c>../nmolecules.brick-examples/samples/bricks/implementation-samples/function-coverage/ViolationAndRuntimeExamples.cs</c>.
    /// </remarks>
    public static class BrickRuleEvaluator
    {
        /// <summary>
        /// Evaluates a policy against dependencies and role-resolution results.
        /// </summary>
        /// <param name="policy">Policy that supplies permission defaults, rules, and enforcement mode.</param>
        /// <param name="dependencies">Observed dependencies to evaluate. A <c>null</c> value is treated as an empty collection.</param>
        /// <param name="resolvedRoles">Resolved roles for the dependency source and target elements. A <c>null</c> value is treated as an empty collection.</param>
        /// <returns>Deterministic violations produced by denied dependencies, closed-policy defaults, or unsatisfied required dependencies.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is <c>null</c>.</exception>
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
            var validDependencies = new List<BrickDependency>();

            foreach (var dependency in dependencyList)
            {
                if (IsSelfDependency(dependency))
                {
                    violations.Add(CreateSelfDependencyViolation(dependency, rolesByElement));
                    continue;
                }

                validDependencies.Add(dependency);
                EvaluatePermission(policy, permissionRules, dependency, rolesByElement, violations);
            }

            if (requirements.Count > 0)
            {
                var targetRolesBySourceAndScope = BuildRequiredDependencyTargetRoleIndex(validDependencies, rolesByElement);
                for (var i = 0; i < requirements.Count; i++)
                {
                    EvaluateRequirement(requirements[i], roleSets, targetRolesBySourceAndScope, violations);
                }
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

        private static Dictionary<(BrickScope Scope, BrickElementId SourceId), HashSet<RoleId>> BuildRequiredDependencyTargetRoleIndex(
            IReadOnlyList<BrickDependency> dependencies,
            IReadOnlyDictionary<BrickElementId, IReadOnlyList<RoleId>> rolesByElement)
        {
            var targetRolesBySourceAndScope = new Dictionary<(BrickScope Scope, BrickElementId SourceId), HashSet<RoleId>>();
            for (var dependencyIndex = 0; dependencyIndex < dependencies.Count; dependencyIndex++)
            {
                var dependency = dependencies[dependencyIndex];
                var key = (dependency.Scope, dependency.Source.Id);
                HashSet<RoleId> targetRoles;
                if (!targetRolesBySourceAndScope.TryGetValue(key, out targetRoles))
                {
                    targetRoles = new HashSet<RoleId>();
                    targetRolesBySourceAndScope.Add(key, targetRoles);
                }

                var dependencyTargetRoles = GetRoles(rolesByElement, dependency.Target);
                for (var roleIndex = 0; roleIndex < dependencyTargetRoles.Count; roleIndex++)
                {
                    targetRoles.Add(dependencyTargetRoles[roleIndex]);
                }
            }

            return targetRolesBySourceAndScope;
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
            IReadOnlyList<BrickResolvedRoles> roleSets,
            IReadOnlyDictionary<(BrickScope Scope, BrickElementId SourceId), HashSet<RoleId>> targetRolesBySourceAndScope,
            ICollection<BrickViolation> violations)
        {
            for (var roleSetIndex = 0; roleSetIndex < roleSets.Count; roleSetIndex++)
            {
                var roleSet = roleSets[roleSetIndex];
                if (!roleSet.EffectiveRoles.Contains(requirement.SourceRole))
                {
                    continue;
                }

                if (!HasRequiredDependency(requirement, roleSet, targetRolesBySourceAndScope))
                {
                    violations.Add(CreateRequirementViolation(requirement, roleSet.Element));
                }
            }
        }

        private static bool HasRequiredDependency(
            BrickRule requirement,
            BrickResolvedRoles roleSet,
            IReadOnlyDictionary<(BrickScope Scope, BrickElementId SourceId), HashSet<RoleId>> targetRolesBySourceAndScope)
        {
            HashSet<RoleId> targetRoles;
            return targetRolesBySourceAndScope.TryGetValue((requirement.Scope, roleSet.Element.Id), out targetRoles) &&
                targetRoles.Contains(requirement.TargetRole);
        }

        private static IReadOnlyList<RoleId> GetRoles(
            IReadOnlyDictionary<BrickElementId, IReadOnlyList<RoleId>> rolesByElement,
            BrickElement element)
        {
            IReadOnlyList<RoleId> roles;
            return rolesByElement.TryGetValue(element.Id, out roles) ? roles : Array.Empty<RoleId>();
        }

        private static bool IsSelfDependency(BrickDependency dependency) =>
            dependency.Source.Id == dependency.Target.Id;

        private static BrickViolation CreateSelfDependencyViolation(
            BrickDependency dependency,
            IReadOnlyDictionary<BrickElementId, IReadOnlyList<RoleId>> rolesByElement)
        {
            var roles = GetRoles(rolesByElement, dependency.Source);
            return new BrickViolation(
                BrickViolationKind.DependencyRule,
                dependency.Source,
                $"Dependency source and target must not be the same element '{dependency.Source.DisplayName}'.",
                BrickSeverity.Error,
                BrickViolationState.Active,
                target: dependency.Target,
                resolvedSourceRoles: roles,
                resolvedTargetRoles: roles,
                dependencyKindId: dependency.KindId,
                scope: dependency.Scope,
                dependencyLayer: dependency.Layer,
                evidenceLevel: dependency.EvidenceLevel);
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
