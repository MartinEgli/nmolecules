using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksRoleCombinationRoundtripTest
    {
        [Fact]
        public void Resolve_WithSuppressAssignmentAtEqualPrecedence_RemovesMatchingAppliedRole()
        {
            var element = Element();
            var legacy = Assignment("Billing.Legacy", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var suppressLegacy = SuppressAssignment("Billing.Legacy", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var application = Assignment("Billing.Application", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);

            var resolved = BrickRoleResolver.Resolve(element, new[] { legacy, suppressLegacy, application }, null);

            Assert.Equal(new[] { application }, resolved.AppliedAssignments);
            Assert.Equal(new[] { legacy }, resolved.SuppressedAssignments);
            Assert.Equal(new[] { RoleId.From("Billing.Application") }, resolved.EffectiveRoles);
        }

        [Fact]
        public void Resolve_WithLowerPrecedenceSuppressAssignment_KeepsStrongerAppliedRole()
        {
            var element = Element();
            var application = Assignment("Billing.Application", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var suppressApplication = SuppressAssignment("Billing.Application", BrickAssignmentSpecificity.Namespace, BrickAssignmentAuthority.External);

            var resolved = BrickRoleResolver.Resolve(element, new[] { application, suppressApplication }, null);

            Assert.Equal(new[] { application }, resolved.AppliedAssignments);
            Assert.Empty(resolved.SuppressedAssignments);
            Assert.Equal(new[] { RoleId.From("Billing.Application") }, resolved.EffectiveRoles);
        }

        [Fact]
        public void Resolve_WithSuppressAssignmentBeforeExclusiveRule_AvoidsFalseExclusiveConflict()
        {
            var element = Element();
            var policy = Assignment("Billing.Business.Policy", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var support = Assignment("Billing.Business.Support", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var suppressSupport = SuppressAssignment("Billing.Business.Support", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var exclusiveRule = new BrickRoleCombinationRule(
                "business-role-exclusive",
                BrickRoleSelector.From("Billing.Business.*"),
                BrickRoleSelector.From("Billing.Business.*"),
                BrickCombinationKind.Exclusive);

            var resolved = BrickRoleResolver.Resolve(element, new[] { policy, support, suppressSupport }, new[] { exclusiveRule });

            Assert.Equal(new[] { policy }, resolved.AppliedAssignments);
            Assert.Equal(new[] { support }, resolved.SuppressedAssignments);
            Assert.Empty(resolved.Conflicts);
            Assert.Empty(BrickRoleResolver.FindResolutionViolations(resolved));
        }

        [Fact]
        public void Resolve_WithAdditiveExclusiveAndIncompatibleRules_ProducesResolvedRoleCombinationViolation()
        {
            var element = Element();
            var contract = Assignment("Billing.Contract", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var shared = Assignment("Billing.Shared", BrickAssignmentSpecificity.Namespace, BrickAssignmentAuthority.External);
            var policy = Assignment("Billing.Business.Policy", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var support = Assignment("Billing.Business.Support", BrickAssignmentSpecificity.Namespace, BrickAssignmentAuthority.Alias);
            var suppressSupport = SuppressAssignment("Billing.Business.Support", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var generated = Assignment("Billing.Generated.Client", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var rules = RoundtripRules();

            var resolved = BrickRoleResolver.Resolve(
                element,
                new[] { contract, shared, policy, support, suppressSupport, generated },
                rules);
            var violations = BrickRoleResolver.FindCombinationViolations(resolved, rules).ToArray();

            Assert.Equal(
                new[]
                {
                    RoleId.From("Billing.Contract"),
                    RoleId.From("Billing.Shared"),
                    RoleId.From("Billing.Business.Policy"),
                    RoleId.From("Billing.Generated.Client")
                },
                resolved.EffectiveRoles);
            Assert.Equal(new[] { support }, resolved.SuppressedAssignments);
            Assert.Empty(resolved.Conflicts);
            var violation = Assert.Single(violations);
            Assert.Equal(BrickViolationKind.RoleCombination, violation.Kind);
            Assert.Equal(RuleId.From("generated-business-policy-incompatible"), violation.RuleId);
            Assert.Equal(new[] { RoleId.From("Billing.Business.Policy"), RoleId.From("Billing.Generated.Client") }, violation.ResolvedSourceRoles);
        }

        [Fact]
        public void FindCombinationViolations_WithUnnamedRule_RemainsWithoutRuleId()
        {
            var element = Element();
            var left = Assignment("Left", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var right = Assignment("Right", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var resolved = BrickRoleResolver.Resolve(element, new[] { left, right }, null);
            var unnamedRule = new BrickRoleCombinationRule(
                null,
                BrickRoleSelector.From("Left"),
                BrickRoleSelector.From("Right"),
                BrickCombinationKind.Incompatible);

            var violation = BrickRoleResolver.FindCombinationViolations(resolved, new[] { unnamedRule }).Single();

            Assert.Null(violation.RuleId);
            Assert.Equal(string.Empty, violation.RuleName);
        }

        [Fact]
        public void ProjectSuppressionMarksRoleCombinationViolationByRuleId()
        {
            var violation = RoleCombinationViolation();
            var suppression = new BrickSuppression(
                RuleId.From("generated-business-policy-incompatible"),
                new BrickElementSelector(BrickElementKind.Type, "Billing.Generated.*", "Billing"),
                "Generated client is temporarily allowed during migration.",
                "Billing Team",
                Now.AddDays(10));

            var projected = BrickViolationStateProjector.Project(new[] { violation }, new[] { suppression }, null, Now).Single();

            Assert.Equal(BrickViolationState.Suppressed, projected.State);
            Assert.Equal("Generated client is temporarily allowed during migration.", projected.StateReason);
            Assert.Equal(BrickViolationState.Active, violation.State);
        }

        [Fact]
        public void ProjectExpiredSuppressionMarksRoleCombinationViolationAsExpired()
        {
            var violation = RoleCombinationViolation();
            var suppression = new BrickSuppression(
                RuleId.From("generated-business-policy-incompatible"),
                new BrickElementSelector(BrickElementKind.Type, "Billing.Generated.*", "Billing"),
                "Generated client exception expired.",
                "Billing Team",
                Now.AddTicks(-1));

            var projected = BrickViolationStateProjector.Project(new[] { violation }, new[] { suppression }, null, Now).Single();

            Assert.Equal(BrickViolationState.ExpiredSuppression, projected.State);
            Assert.Contains("Suppression expired", projected.StateReason);
        }

        private static readonly System.DateTimeOffset Now = new(2026, 6, 28, 10, 0, 0, System.TimeSpan.Zero);

        private static BrickViolation RoleCombinationViolation()
        {
            var element = Element();
            var businessPolicy = Assignment("Billing.Business.Policy", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var generatedClient = Assignment("Billing.Generated.Client", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var resolved = BrickRoleResolver.Resolve(element, new[] { businessPolicy, generatedClient }, null);

            return BrickRoleResolver.FindCombinationViolations(resolved, RoundtripRules()).Single();
        }

        private static BrickRoleCombinationRule[] RoundtripRules() =>
            new[]
            {
                new BrickRoleCombinationRule(
                    "contract-shared-additive",
                    BrickRoleSelector.From("Billing.Contract"),
                    BrickRoleSelector.From("Billing.Shared"),
                    BrickCombinationKind.Additive,
                    "Contract markers may also be shared markers."),
                new BrickRoleCombinationRule(
                    "business-role-exclusive",
                    BrickRoleSelector.From("Billing.Business.*"),
                    BrickRoleSelector.From("Billing.Business.*"),
                    BrickCombinationKind.Exclusive,
                    "Only one business role may remain after policy exceptions."),
                new BrickRoleCombinationRule(
                    "generated-business-policy-incompatible",
                    BrickRoleSelector.From("Billing.Generated.*"),
                    BrickRoleSelector.From("Billing.Business.*"),
                    BrickCombinationKind.Incompatible,
                    "Generated clients must not own business policy.")
            };

        private static BrickElement Element() =>
            new BrickElement(
                BrickElementId.From("type:Billing.Generated.CustomerProjectionPolicy"),
                BrickElementKind.Type,
                "CustomerProjectionPolicy",
                "Billing",
                "Billing.Generated",
                "Billing.Generated.CustomerProjectionPolicy",
                BrickElementOrigin.Source,
                BrickElementSource.Code);

        private static BrickRoleAssignment Assignment(
            string role,
            BrickAssignmentSpecificity specificity,
            BrickAssignmentAuthority authority) =>
            new BrickRoleAssignment(
                new BrickElementSelector(BrickElementKind.Type, "*"),
                RoleId.From(role),
                BrickAssignmentMode.DirectAttribute,
                BrickAssignmentSource.SourceAttribute,
                new BrickAssignmentPrecedence(specificity, authority),
                BrickAssignmentBehavior.Apply,
                "Applied by test policy.");

        private static BrickRoleAssignment SuppressAssignment(
            string role,
            BrickAssignmentSpecificity specificity,
            BrickAssignmentAuthority authority) =>
            new BrickRoleAssignment(
                new BrickElementSelector(BrickElementKind.Type, "*"),
                RoleId.From(role),
                BrickAssignmentMode.ExternalConfiguration,
                BrickAssignmentSource.PolicyFile,
                new BrickAssignmentPrecedence(specificity, authority),
                BrickAssignmentBehavior.Suppress,
                "Suppressed by test policy exception.");
    }
}
