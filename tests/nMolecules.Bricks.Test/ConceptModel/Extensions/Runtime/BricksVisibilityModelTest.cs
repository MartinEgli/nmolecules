using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksVisibilityModelTest
    {
        [Fact]
        public void FriendAssemblyGrantCreatesVisibilityDependency()
        {
            var exposing = Assembly("Billing.Domain");
            var friend = Assembly("Billing.Domain.Tests");
            var grant = new BrickFriendAssemblyGrant(
                exposing,
                friend,
                "Tests may inspect internal aggregate builders.",
                BrickEvidenceLevel.AnalyzerInferred);

            var dependency = grant.ToDependency();

            Assert.Equal(exposing, grant.ExposingAssembly);
            Assert.Equal(friend, grant.FriendAssembly);
            Assert.Equal("Tests may inspect internal aggregate builders.", grant.Justification);
            Assert.Equal(BrickEvidenceLevel.AnalyzerInferred, grant.EvidenceLevel);
            Assert.Equal(friend, dependency.Source);
            Assert.Equal(exposing, dependency.Target);
            Assert.Equal(BrickDependencyKindId.From("FriendAssembly"), dependency.KindId);
            Assert.Equal(BrickScope.Assembly, dependency.Scope);
            Assert.Equal(BrickDependencyLayer.Visibility, dependency.Layer);
            Assert.Equal(BrickDependencyStrength.Direct, dependency.Strength);
            Assert.Equal(BrickEvidenceLevel.AnalyzerInferred, dependency.EvidenceLevel);
            Assert.Equal("Tests may inspect internal aggregate builders.", dependency.Detail);
        }

        [Fact]
        public void FriendAssemblyGrantRequiresAssemblies()
        {
            var assembly = Assembly("Billing.Domain");

            Assert.Throws<ArgumentNullException>(() => new BrickFriendAssemblyGrant(null, assembly));
            Assert.Throws<ArgumentNullException>(() => new BrickFriendAssemblyGrant(assembly, null));
        }

        [Fact]
        public void VisibilityPolicyUsesDefaultFriendConsumerRoles()
        {
            var policy = new BrickVisibilityPolicy();

            Assert.Equal(new[] { RoleId.From("FriendConsumer"), RoleId.From("TestOnly") }, policy.AllowedFriendConsumerRoles.ToArray());
            Assert.True(policy.RequireJustification);
            Assert.True(policy.Enabled);
        }

        [Fact]
        public void VisibilityPolicyCopiesAndDistinctsCustomRoles()
        {
            var roles = new[] { RoleId.From("Custom"), RoleId.From("Custom"), RoleId.From("Other") };

            var policy = new BrickVisibilityPolicy(roles, false, false);
            roles[0] = RoleId.From("Mutated");

            Assert.Equal(new[] { RoleId.From("Custom"), RoleId.From("Other") }, policy.AllowedFriendConsumerRoles.ToArray());
            Assert.False(policy.RequireJustification);
            Assert.False(policy.Enabled);
        }

        [Fact]
        public void EvaluateFriendAssembliesAllowsJustifiedFriendConsumer()
        {
            var exposing = Assembly("Billing.Domain");
            var friend = Assembly("Billing.Domain.Tests");
            var grant = new BrickFriendAssemblyGrant(exposing, friend, "Testing internals.");
            var roles = Roles(friend, RoleId.From("TestOnly"));

            var violations = BrickVisibilityEvaluator.EvaluateFriendAssemblies(new[] { grant }, roles);

            Assert.Empty(violations);
        }

        [Fact]
        public void EvaluateFriendAssembliesReportsMissingFriendConsumerRole()
        {
            var exposing = Assembly("Billing.Domain");
            var friend = Assembly("Billing.Domain.Tests");
            var grant = new BrickFriendAssemblyGrant(exposing, friend, "Testing internals.");
            var roles = Roles(friend, RoleId.From("Business.Billing"));

            var violation = BrickVisibilityEvaluator.EvaluateFriendAssemblies(new[] { grant }, roles).Single();

            Assert.Equal(BrickVisibilityEvaluator.FriendConsumerRoleRuleId, violation.RuleId);
            Assert.Equal(BrickViolationKind.DependencyRule, violation.Kind);
            Assert.Equal(friend, violation.Source);
            Assert.Equal(exposing, violation.Target);
            Assert.Equal(BrickSeverity.Warning, violation.Severity);
            Assert.Equal(BrickViolationState.Active, violation.State);
            Assert.Equal("Friend assembly visibility", violation.RuleName);
            Assert.Equal("Friend assembly must carry an allowed friend-consumer role.", violation.Message);
            Assert.Equal(new[] { RoleId.From("Business.Billing") }, violation.ResolvedSourceRoles.ToArray());
            Assert.Empty(violation.ResolvedTargetRoles);
            Assert.Equal(BrickDependencyKindId.From("FriendAssembly"), violation.DependencyKindId);
            Assert.Equal(BrickScope.Assembly, violation.Scope);
            Assert.Equal(BrickDependencyLayer.Visibility, violation.DependencyLayer);
            Assert.Equal(BrickEvidenceLevel.CompilerConfirmed, violation.EvidenceLevel);
            Assert.Equal("Testing internals.", violation.StateReason);
        }

        [Fact]
        public void EvaluateFriendAssembliesReportsMissingJustification()
        {
            var exposing = Assembly("Billing.Domain");
            var friend = Assembly("Billing.Domain.Tests");
            var grant = new BrickFriendAssemblyGrant(exposing, friend, " ");
            var roles = Roles(friend, RoleId.From("FriendConsumer"));

            var violation = BrickVisibilityEvaluator.EvaluateFriendAssemblies(new[] { grant }, roles).Single();

            Assert.Equal(BrickVisibilityEvaluator.FriendJustificationRuleId, violation.RuleId);
            Assert.Equal("Friend assembly exposure must be justified.", violation.Message);
        }

        [Fact]
        public void EvaluateFriendAssembliesCanReportRoleAndJustificationTogether()
        {
            var exposing = Assembly("Billing.Domain");
            var friend = Assembly("Billing.Infrastructure");
            var grant = new BrickFriendAssemblyGrant(exposing, friend);

            var violations = BrickVisibilityEvaluator.EvaluateFriendAssemblies(new[] { grant }, null);

            Assert.Equal(
                new[] { BrickVisibilityEvaluator.FriendConsumerRoleRuleId, BrickVisibilityEvaluator.FriendJustificationRuleId },
                violations.Select(violation => violation.RuleId.Value).ToArray());
        }

        [Fact]
        public void EvaluateFriendAssembliesHonorsCustomPolicyAndNullRoleList()
        {
            var exposing = Assembly("Billing.Domain");
            var friend = Assembly("Billing.Tools");
            var grant = new BrickFriendAssemblyGrant(exposing, friend, "Tooling access.");
            var roles = new Dictionary<BrickElementId, IEnumerable<RoleId>>
            {
                [friend.Id] = null
            };
            var policy = new BrickVisibilityPolicy(new[] { RoleId.From("Tooling") }, false);

            var violation = BrickVisibilityEvaluator.EvaluateFriendAssemblies(new[] { grant }, roles, policy).Single();

            Assert.Equal(BrickVisibilityEvaluator.FriendConsumerRoleRuleId, violation.RuleId);
            Assert.Empty(violation.ResolvedSourceRoles);
        }

        [Fact]
        public void EvaluateFriendAssembliesNormalizesNullInputsAndDisabledPolicy()
        {
            Assert.Empty(BrickVisibilityEvaluator.EvaluateFriendAssemblies(null, null));
            Assert.Empty(BrickVisibilityEvaluator.EvaluateFriendAssemblies(
                new[] { new BrickFriendAssemblyGrant(Assembly("A"), Assembly("B")) },
                null,
                new BrickVisibilityPolicy(enabled: false)));
        }

        private static Dictionary<BrickElementId, IEnumerable<RoleId>> Roles(BrickElement element, params RoleId[] roles) =>
            new Dictionary<BrickElementId, IEnumerable<RoleId>>
            {
                [element.Id] = roles
            };

        private static BrickElement Assembly(string name) =>
            new BrickElement(BrickElementId.From("assembly:" + name), BrickElementKind.Assembly, name, name, null, name);
    }
}
