using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    public sealed class BrickFriendAssemblyGrant
    {
        public const string DependencyKind = "FriendAssembly";

        public BrickFriendAssemblyGrant(
            BrickElement exposingAssembly,
            BrickElement friendAssembly,
            string justification = null,
            BrickEvidenceLevel evidenceLevel = BrickEvidenceLevel.CompilerConfirmed)
        {
            ExposingAssembly = exposingAssembly ?? throw new ArgumentNullException(nameof(exposingAssembly));
            FriendAssembly = friendAssembly ?? throw new ArgumentNullException(nameof(friendAssembly));
            Justification = justification;
            EvidenceLevel = evidenceLevel;
        }

        public BrickElement ExposingAssembly { get; }
        public BrickElement FriendAssembly { get; }
        public string Justification { get; }
        public BrickEvidenceLevel EvidenceLevel { get; }

        public BrickDependency ToDependency() =>
            new BrickDependency(
                FriendAssembly,
                ExposingAssembly,
                BrickDependencyKindId.From(DependencyKind),
                BrickScope.Assembly,
                BrickDependencyLayer.Visibility,
                BrickDependencyStrength.Direct,
                EvidenceLevel,
                detail: Justification);
    }

    public sealed class BrickVisibilityPolicy
    {
        public BrickVisibilityPolicy(
            IEnumerable<RoleId> allowedFriendConsumerRoles = null,
            bool requireJustification = true,
            bool enabled = true)
        {
            AllowedFriendConsumerRoles = (allowedFriendConsumerRoles ?? Defaults()).Distinct().ToArray();
            RequireJustification = requireJustification;
            Enabled = enabled;
        }

        public IReadOnlyList<RoleId> AllowedFriendConsumerRoles { get; }
        public bool RequireJustification { get; }
        public bool Enabled { get; }

        private static IEnumerable<RoleId> Defaults()
        {
            yield return RoleId.From("FriendConsumer");
            yield return RoleId.From("TestOnly");
        }
    }

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
