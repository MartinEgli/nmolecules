using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Evaluates visibility evaluator rules against Bricks model data and produces deterministic assessment
/// results.
/// </summary>
public static class BrickVisibilityEvaluator
    {
        public static readonly RuleId FriendConsumerRoleRuleId = RuleId.From("XMoleculesBricks0700");
        public static readonly RuleId FriendJustificationRuleId = RuleId.From("XMoleculesBricks0701");

        public static IReadOnlyList<BrickViolation> EvaluateFriendAssemblies(
            IEnumerable<BrickFriendAssemblyGrant> grants,
            IReadOnlyDictionary<BrickElementId, IEnumerable<RoleId>> rolesByElement,
            BrickVisibilityPolicy policy = null)
        {
            var activePolicy = policy ?? new BrickVisibilityPolicy();
            if (!activePolicy.Enabled)
            {
                return Enumerable.Empty<BrickViolation>().ToArray();
            }

            var violations = new List<BrickViolation>();
            foreach (var grant in grants ?? Enumerable.Empty<BrickFriendAssemblyGrant>())
            {
                var roles = ResolveRoles(rolesByElement, grant.FriendAssembly).ToArray();
                if (!roles.Any(role => activePolicy.AllowedFriendConsumerRoles.Contains(role)))
                {
                    violations.Add(Violation(
                        grant,
                        FriendConsumerRoleRuleId,
                        "Friend assembly must carry an allowed friend-consumer role.",
                        roles));
                }

                if (activePolicy.RequireJustification && string.IsNullOrWhiteSpace(grant.Justification))
                {
                    violations.Add(Violation(
                        grant,
                        FriendJustificationRuleId,
                        "Friend assembly exposure must be justified.",
                        roles));
                }
            }

            return violations;
        }

        private static IEnumerable<RoleId> ResolveRoles(
            IReadOnlyDictionary<BrickElementId, IEnumerable<RoleId>> rolesByElement,
            BrickElement element)
        {
            if (rolesByElement != null && rolesByElement.TryGetValue(element.Id, out var roles))
            {
                return roles ?? Enumerable.Empty<RoleId>();
            }

            return Enumerable.Empty<RoleId>();
        }

        private static BrickViolation Violation(
            BrickFriendAssemblyGrant grant,
            RuleId ruleId,
            string message,
            IEnumerable<RoleId> friendRoles) =>
            new BrickViolation(
                BrickViolationKind.DependencyRule,
                grant.FriendAssembly,
                message,
                BrickSeverity.Warning,
                BrickViolationState.Active,
                ruleId,
                "Friend assembly visibility",
                grant.ExposingAssembly,
                friendRoles,
                null,
                BrickDependencyKindId.From(BrickFriendAssemblyGrant.DependencyKind),
                BrickScope.Assembly,
                BrickDependencyLayer.Visibility,
                grant.EvidenceLevel,
                grant.Justification);
    }
}
