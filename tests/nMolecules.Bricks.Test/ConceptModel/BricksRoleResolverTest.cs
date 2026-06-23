using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksRoleResolverTest
    {
        [Fact]
        public void Resolve_WithExclusiveRule_SuppressesWeakerAssignment()
        {
            var element = Element();
            var weaker = Assignment("Business.Support", BrickAssignmentSpecificity.Namespace, BrickAssignmentAuthority.Alias);
            var stronger = Assignment("Business.Sales", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var rule = new BrickRoleCombinationRule(
                "business-partition-exclusive",
                BrickRoleSelector.From("Business.*"),
                BrickRoleSelector.From("Business.*"),
                BrickCombinationKind.Exclusive,
                "Only one business partition may remain.");

            var resolved = BrickRoleResolver.Resolve(element, new[] { weaker, stronger }, new[] { rule });

            Assert.Equal(new[] { stronger }, resolved.AppliedAssignments);
            Assert.Equal(new[] { weaker }, resolved.SuppressedAssignments);
            Assert.Empty(resolved.Conflicts);
            Assert.Equal(new[] { RoleId.From("Business.Sales") }, resolved.EffectiveRoles);
        }

        [Fact]
        public void Resolve_WithExclusiveRule_SuppressesRightSideWhenLeftIsStronger()
        {
            var element = Element();
            var stronger = Assignment("Business.Sales", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var weaker = Assignment("Business.Support", BrickAssignmentSpecificity.Namespace, BrickAssignmentAuthority.Alias);
            var rule = new BrickRoleCombinationRule(
                "business-partition-exclusive",
                BrickRoleSelector.From("Business.*"),
                BrickRoleSelector.From("Business.*"),
                BrickCombinationKind.Exclusive);

            var resolved = BrickRoleResolver.Resolve(element, new[] { stronger, weaker }, new[] { rule });

            Assert.Equal(new[] { stronger }, resolved.AppliedAssignments);
            Assert.Equal(new[] { weaker }, resolved.SuppressedAssignments);
        }

        [Fact]
        public void Resolve_WithEqualPrecedenceExclusiveRule_RecordsConflictWithoutWinner()
        {
            var element = Element();
            var left = Assignment("Business.Support", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var right = Assignment("Business.Sales", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var rule = new BrickRoleCombinationRule(
                "business-partition-exclusive",
                BrickRoleSelector.From("Business.*"),
                BrickRoleSelector.From("Business.*"),
                BrickCombinationKind.Exclusive,
                "Only one business partition may remain.");

            var resolved = BrickRoleResolver.Resolve(element, new[] { left, right }, new[] { rule });

            Assert.Empty(resolved.AppliedAssignments);
            Assert.Empty(resolved.SuppressedAssignments);
            Assert.Equal(new[] { left, right }, resolved.CandidateAssignments);
            Assert.Single(resolved.Conflicts);
            Assert.Contains("business-partition-exclusive", resolved.Conflicts.Single().Reason);
            Assert.Empty(resolved.EffectiveRoles);
        }

        [Fact]
        public void Resolve_WithAdditiveRule_KeepsBothAssignments()
        {
            var element = Element();
            var contracts = Assignment("Contracts", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var shared = Assignment("Shared", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var rule = new BrickRoleCombinationRule(
                "contracts-shared-additive",
                BrickRoleSelector.From("Contracts"),
                BrickRoleSelector.From("Shared"),
                BrickCombinationKind.Additive,
                "Contracts may also be shared.");

            var resolved = BrickRoleResolver.Resolve(element, new[] { contracts, shared }, new[] { rule });

            Assert.Equal(new[] { contracts, shared }, resolved.AppliedAssignments);
            Assert.Empty(resolved.SuppressedAssignments);
            Assert.Empty(resolved.Conflicts);
            Assert.Equal(new[] { RoleId.From("Contracts"), RoleId.From("Shared") }, resolved.EffectiveRoles);
        }

        [Fact]
        public void Resolve_CollapsesDuplicateEffectiveRoles()
        {
            var element = Element();
            var weaker = Assignment("Shared", BrickAssignmentSpecificity.Namespace, BrickAssignmentAuthority.Alias);
            var stronger = Assignment("Shared", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);

            var resolved = BrickRoleResolver.Resolve(element, new[] { weaker, stronger }, Enumerable.Empty<BrickRoleCombinationRule>());

            Assert.Equal(new[] { weaker, stronger }, resolved.AppliedAssignments);
            Assert.Equal(new[] { RoleId.From("Shared") }, resolved.EffectiveRoles);
        }

        [Fact]
        public void Resolve_NormalizesNullInputs()
        {
            var element = Element();

            var resolved = BrickRoleResolver.Resolve(element, null, null);

            Assert.Equal(element, resolved.Element);
            Assert.Empty(resolved.CandidateAssignments);
            Assert.Empty(resolved.AppliedAssignments);
            Assert.Empty(resolved.SuppressedAssignments);
            Assert.Empty(resolved.Conflicts);
        }

        [Fact]
        public void FindCombinationViolations_WithIncompatibleRule_EmitsRoleCombinationViolation()
        {
            var element = Element();
            var generated = Assignment("Generated", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var business = Assignment("Business.Sales", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var resolved = BrickRoleResolver.Resolve(element, new[] { generated, business }, Enumerable.Empty<BrickRoleCombinationRule>());
            var rule = new BrickRoleCombinationRule(
                "generated-business-incompatible",
                BrickRoleSelector.From("Generated"),
                BrickRoleSelector.From("Business.*"),
                BrickCombinationKind.Incompatible,
                "Generated business code must be reviewed.");

            var violations = BrickRoleResolver.FindCombinationViolations(resolved, new[] { rule }).ToArray();

            Assert.Single(violations);
            Assert.Equal(BrickViolationKind.RoleCombination, violations[0].Kind);
            Assert.Equal(element, violations[0].Source);
            Assert.Equal(BrickSeverity.Warning, violations[0].Severity);
            Assert.Equal(BrickViolationState.Active, violations[0].State);
            Assert.Equal("generated-business-incompatible", violations[0].RuleName);
            Assert.Equal(new[] { RoleId.From("Generated"), RoleId.From("Business.Sales") }, violations[0].ResolvedSourceRoles);
            Assert.Contains("Generated business code must be reviewed.", violations[0].Message);
        }

        [Fact]
        public void FindCombinationViolations_WithNullResolvedRoles_EmitsNoViolations()
        {
            var violations = BrickRoleResolver.FindCombinationViolations(null, null).ToArray();

            Assert.Empty(violations);
        }

        [Fact]
        public void FindCombinationViolations_WithNullRules_EmitsNoViolations()
        {
            var element = Element();
            var left = Assignment("A", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var right = Assignment("B", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var resolved = new BrickResolvedRoles(element, new[] { left, right }, new[] { left, right }, null, null);

            var violations = BrickRoleResolver.FindCombinationViolations(resolved, null).ToArray();

            Assert.Empty(violations);
        }

        [Fact]
        public void FindResolutionViolations_WithExclusiveConflict_EmitsRoleResolutionViolation()
        {
            var element = Element();
            var left = Assignment("Business.Support", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var right = Assignment("Business.Sales", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var rule = new BrickRoleCombinationRule(
                "business-partition-exclusive",
                BrickRoleSelector.From("Business.*"),
                BrickRoleSelector.From("Business.*"),
                BrickCombinationKind.Exclusive,
                "Only one business partition may remain.");
            var resolved = BrickRoleResolver.Resolve(element, new[] { left, right }, new[] { rule });

            var violations = BrickRoleResolver.FindResolutionViolations(resolved).ToArray();

            Assert.Single(violations);
            Assert.Equal(BrickViolationKind.RoleResolution, violations[0].Kind);
            Assert.Equal(element, violations[0].Source);
            Assert.Null(violations[0].Target);
            Assert.Equal(BrickSeverity.Warning, violations[0].Severity);
            Assert.Equal(BrickViolationState.Active, violations[0].State);
            Assert.Equal(new[] { left.RoleId, right.RoleId }, violations[0].ResolvedSourceRoles);
            Assert.Contains("business-partition-exclusive", violations[0].Message);
            Assert.Contains("Only one business partition may remain.", violations[0].Message);
        }

        [Fact]
        public void FindResolutionViolations_WithNullResolvedRoles_EmitsNoViolations()
        {
            var violations = BrickRoleResolver.FindResolutionViolations(null).ToArray();

            Assert.Empty(violations);
        }

        [Fact]
        public void RoleSelectorMatchesWildcardExactAndPrefixPatterns()
        {
            Assert.True(BrickRoleSelector.From("*").Matches(RoleId.From("Anything")));
            Assert.True(BrickRoleSelector.From("Business.*").Matches(RoleId.From("Business.Sales")));
            Assert.True(BrickRoleSelector.From("Contracts").Matches(RoleId.From("Contracts")));
            Assert.False(BrickRoleSelector.From("Business.*").Matches(RoleId.From("Contracts")));
            Assert.False(BrickRoleSelector.From("Contracts").Matches(RoleId.From("Contracts.Internal")));
        }

        [Fact]
        public void RoleSelectorExposesValueSemantics()
        {
            var selector = BrickRoleSelector.From("Business.*");

            Assert.True(selector.Equals((object)BrickRoleSelector.From("Business.*")));
            Assert.False(selector.Equals("Business.*"));
            Assert.Equal(BrickRoleSelector.From("Business.*").GetHashCode(), selector.GetHashCode());
            Assert.True(selector == BrickRoleSelector.From("Business.*"));
            Assert.True(selector != BrickRoleSelector.From("Contracts"));
        }

        [Fact]
        public void RoleSelectorAndCombinationRuleNormalizeNullValues()
        {
            var selector = BrickRoleSelector.From(null);
            var rule = new BrickRoleCombinationRule(null, selector, null, BrickCombinationKind.Additive, null);

            Assert.Equal(string.Empty, selector.Pattern);
            Assert.False(selector.Matches(RoleId.From("Anything")));
            Assert.Equal(BrickRoleSelector.From(null).GetHashCode(), default(BrickRoleSelector).GetHashCode());
            Assert.Equal(string.Empty, rule.Name);
            Assert.Equal(selector, rule.LeftRoles);
            Assert.Equal(default, rule.RightRoles);
            Assert.Equal(BrickCombinationKind.Additive, rule.Kind);
            Assert.Null(rule.Reason);
        }

        [Fact]
        public void CombinationRuleMatchesSymmetricAndNonMatchingPairs()
        {
            var rule = new BrickRoleCombinationRule(
                "shared-contracts",
                BrickRoleSelector.From("Shared"),
                BrickRoleSelector.From("Contracts"),
                BrickCombinationKind.Additive);

            Assert.True(rule.Matches(RoleId.From("Contracts"), RoleId.From("Shared")));
            Assert.False(rule.Matches(RoleId.From("Shared"), RoleId.From("Infrastructure")));
        }

        [Fact]
        public void ViolationMessagesUseFallbackTextForUnnamedRules()
        {
            var element = Element();
            var left = Assignment("A", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var right = Assignment("B", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var exclusive = new BrickRoleCombinationRule(null, BrickRoleSelector.From("A"), BrickRoleSelector.From("B"), BrickCombinationKind.Exclusive);
            var incompatible = new BrickRoleCombinationRule(null, BrickRoleSelector.From("A"), BrickRoleSelector.From("B"), BrickCombinationKind.Incompatible);
            var resolved = BrickRoleResolver.Resolve(element, new[] { left, right }, new[] { exclusive });

            var resolutionViolation = BrickRoleResolver.FindResolutionViolations(resolved).Single();
            var combinationViolation = BrickRoleResolver.FindCombinationViolations(
                new BrickResolvedRoles(element, new[] { left, right }, new[] { left, right }, null, null),
                new[] { incompatible }).Single();

            Assert.Contains("Exclusive role assignments have equal precedence.", resolutionViolation.Message);
            Assert.Contains("Role combination is incompatible.", combinationViolation.Message);
        }

        private static BrickElement Element() =>
            new BrickElement(BrickElementId.From("type:OrderPolicy"), BrickElementKind.Type, "OrderPolicy");

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
                null);
    }
}
