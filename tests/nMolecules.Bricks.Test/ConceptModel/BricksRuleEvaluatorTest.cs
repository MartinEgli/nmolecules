using System;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksRuleEvaluatorTest
    {
        [Fact]
        public void EvaluatePermission_WithMatchingDenyRule_EmitsDependencyViolation()
        {
            var source = Element("type:OrderService", "OrderService");
            var target = Element("type:SqlGateway", "SqlGateway");
            var dependency = Dependency(source, target);
            var rule = Rule("BRK-001", "No infrastructure", "Domain", "Infrastructure", BrickDecision.Deny, priority: 10);
            var policy = Policy(BrickPermissionDefault.Allow, BrickEnforcementMode.Analyze, rule);

            var violations = BrickRuleEvaluator.Evaluate(
                policy,
                new[] { dependency },
                new[] { Resolved(source, "Domain"), Resolved(target, "Infrastructure") });

            Assert.Single(violations);
            Assert.Equal(BrickViolationKind.DependencyRule, violations[0].Kind);
            Assert.Equal(rule.RuleId, violations[0].RuleId);
            Assert.Equal(rule.Name, violations[0].RuleName);
            Assert.Equal(source, violations[0].Source);
            Assert.Equal(target, violations[0].Target);
            Assert.Equal(new[] { RoleId.From("Domain") }, violations[0].ResolvedSourceRoles);
            Assert.Equal(new[] { RoleId.From("Infrastructure") }, violations[0].ResolvedTargetRoles);
            Assert.Equal(dependency.KindId, violations[0].DependencyKindId);
            Assert.Equal(dependency.Scope, violations[0].Scope);
            Assert.Equal(dependency.Layer, violations[0].DependencyLayer);
            Assert.Equal(dependency.EvidenceLevel, violations[0].EvidenceLevel);
            Assert.Equal(rule.Severity, violations[0].Severity);
            Assert.Equal(BrickViolationState.Active, violations[0].State);
            Assert.Contains("No infrastructure", violations[0].Message);
        }

        [Fact]
        public void EvaluatePermission_WithMatchingAllowRule_AllowsClosedPolicyDependency()
        {
            var source = Element("type:OrderService", "OrderService");
            var target = Element("type:Contracts", "Contracts");
            var policy = Policy(
                BrickPermissionDefault.Deny,
                BrickEnforcementMode.Analyze,
                Rule("BRK-ALLOW", "Domain may use contracts", "Domain", "Contracts", BrickDecision.Allow));

            var violations = BrickRuleEvaluator.Evaluate(
                policy,
                new[] { Dependency(source, target) },
                new[] { Resolved(source, "Domain"), Resolved(target, "Contracts") });

            Assert.Empty(violations);
        }

        [Fact]
        public void EvaluatePermission_WithNoMatchingRule_UsesExplicitDefaultDecision()
        {
            var source = Element("type:OrderService", "OrderService");
            var target = Element("type:SqlGateway", "SqlGateway");

            var closedViolations = BrickRuleEvaluator.Evaluate(
                Policy(BrickPermissionDefault.Deny, BrickEnforcementMode.Analyze),
                new[] { Dependency(source, target) },
                new[] { Resolved(source, "Domain"), Resolved(target, "Infrastructure") });
            var openViolations = BrickRuleEvaluator.Evaluate(
                Policy(BrickPermissionDefault.Allow, BrickEnforcementMode.Analyze),
                new[] { Dependency(source, target) },
                new[] { Resolved(source, "Domain"), Resolved(target, "Infrastructure") });

            Assert.Single(closedViolations);
            Assert.Null(closedViolations[0].RuleId);
            Assert.Null(closedViolations[0].RuleName);
            Assert.Equal(BrickSeverity.Error, closedViolations[0].Severity);
            Assert.Contains("Default decision Deny", closedViolations[0].Message);
            Assert.Empty(openViolations);
        }

        [Fact]
        public void EvaluatePermission_WithConflictingTopPriorityRules_DenyWins()
        {
            var source = Element("type:OrderService", "OrderService");
            var target = Element("type:SqlGateway", "SqlGateway");
            var allow = Rule("BRK-ALLOW", "Allow infrastructure", "Domain", "Infrastructure", BrickDecision.Allow, priority: 5);
            var deny = Rule("BRK-DENY", "Deny infrastructure", "Domain", "Infrastructure", BrickDecision.Deny, priority: 5);

            var violations = BrickRuleEvaluator.Evaluate(
                Policy(BrickPermissionDefault.Allow, BrickEnforcementMode.Analyze, allow, deny),
                new[] { Dependency(source, target) },
                new[] { Resolved(source, "Domain"), Resolved(target, "Infrastructure") });

            Assert.Single(violations);
            Assert.Equal(deny.RuleId, violations[0].RuleId);
        }

        [Fact]
        public void EvaluatePermission_WithHigherPriorityAllow_OverridesLowerPriorityDeny()
        {
            var source = Element("type:OrderService", "OrderService");
            var target = Element("type:SqlGateway", "SqlGateway");
            var deny = Rule("BRK-DENY", "Deny infrastructure", "Domain", "Infrastructure", BrickDecision.Deny);
            var allow = Rule("BRK-ALLOW", "Allow explicit gateway", "Domain", "Infrastructure", BrickDecision.Allow, priority: 10);

            var violations = BrickRuleEvaluator.Evaluate(
                Policy(BrickPermissionDefault.Allow, BrickEnforcementMode.Analyze, deny, allow),
                new[] { Dependency(source, target) },
                new[] { Resolved(source, "Domain"), Resolved(target, "Infrastructure") });

            Assert.Empty(violations);
        }

        [Fact]
        public void EvaluatePermission_WithEmptyTopPriorityDeny_DoesNotReplaceItWithLaterDeny()
        {
            var source = Element("type:OrderService", "OrderService");
            var target = Element("type:SqlGateway", "SqlGateway");
            var emptyDeny = Rule(null, "Deny without id", "Domain", "Infrastructure", BrickDecision.Deny, priority: 10);
            var laterDeny = Rule("BRK-DENY", "Later deny", "Domain", "Infrastructure", BrickDecision.Deny, priority: 10);

            var violations = BrickRuleEvaluator.Evaluate(
                Policy(BrickPermissionDefault.Allow, BrickEnforcementMode.Analyze, emptyDeny, laterDeny),
                new[] { Dependency(source, target) },
                new[] { Resolved(source, "Domain"), Resolved(target, "Infrastructure") });

            Assert.Empty(violations);
        }

        [Fact]
        public void EvaluatePermission_WithMultipleRoleMatches_PreservesPolicyOrderForEqualPriorityDeny()
        {
            var source = Element("type:OrderService", "OrderService");
            var target = Element("type:SqlGateway", "SqlGateway");
            var first = Rule("BRK-FIRST", "First deny", "B", "Y", BrickDecision.Deny, priority: 10);
            var second = Rule("BRK-SECOND", "Second deny", "A", "X", BrickDecision.Deny, priority: 10);

            var violations = BrickRuleEvaluator.Evaluate(
                Policy(BrickPermissionDefault.Allow, BrickEnforcementMode.Analyze, first, second),
                new[] { Dependency(source, target) },
                new[] { Resolved(source, "A", "B"), Resolved(target, "X", "Y") });

            Assert.Single(violations);
            Assert.Equal(first.RuleId, violations[0].RuleId);
        }

        [Fact]
        public void EvaluatePermission_WithLaterEqualPriorityAllow_KeepsEarlierDeny()
        {
            var source = Element("type:OrderService", "OrderService");
            var target = Element("type:SqlGateway", "SqlGateway");
            var deny = Rule("BRK-DENY", "Deny", "Domain", "Infrastructure", BrickDecision.Deny, priority: 10);
            var allow = Rule("BRK-ALLOW", "Allow", "Domain", "Infrastructure", BrickDecision.Allow, priority: 10);

            var violations = BrickRuleEvaluator.Evaluate(
                Policy(BrickPermissionDefault.Allow, BrickEnforcementMode.Analyze, deny, allow),
                new[] { Dependency(source, target) },
                new[] { Resolved(source, "Domain"), Resolved(target, "Infrastructure") });

            Assert.Single(violations);
            Assert.Equal(deny.RuleId, violations[0].RuleId);
        }

        [Fact]
        public void EvaluatePermission_WithLaterLowerPriorityDeny_KeepsHigherPriorityAllow()
        {
            var source = Element("type:OrderService", "OrderService");
            var target = Element("type:SqlGateway", "SqlGateway");
            var allow = Rule("BRK-ALLOW", "Allow", "Domain", "Infrastructure", BrickDecision.Allow, priority: 10);
            var deny = Rule("BRK-DENY", "Deny", "Domain", "Infrastructure", BrickDecision.Deny, priority: 5);

            var violations = BrickRuleEvaluator.Evaluate(
                Policy(BrickPermissionDefault.Allow, BrickEnforcementMode.Analyze, allow, deny),
                new[] { Dependency(source, target) },
                new[] { Resolved(source, "Domain"), Resolved(target, "Infrastructure") });

            Assert.Empty(violations);
        }

        [Fact]
        public void EvaluateRequirement_EmitsViolationWhenMatchingSourceHasNoTargetDependency()
        {
            var source = Element("type:OrderAggregate", "OrderAggregate");
            var other = Element("type:CustomerAggregate", "CustomerAggregate");
            var requirement = Rule("BRK-REQ", "Aggregate must raise event", "AggregateRoot", "DomainEvent", BrickDecision.Require);

            var violations = BrickRuleEvaluator.Evaluate(
                Policy(BrickPermissionDefault.Allow, BrickEnforcementMode.Analyze, requirement),
                Enumerable.Empty<BrickDependency>(),
                new[] { Resolved(source, "AggregateRoot"), Resolved(other, "AggregateRoot") });

            Assert.Equal(2, violations.Count);
            Assert.All(violations, violation => Assert.Equal(BrickViolationKind.RequiredDependency, violation.Kind));
            Assert.All(violations, violation => Assert.Equal(requirement.RuleId, violation.RuleId));
            Assert.All(violations, violation => Assert.Equal(new[] { RoleId.From("AggregateRoot") }, violation.ResolvedSourceRoles));
            Assert.All(violations, violation => Assert.Equal(new[] { RoleId.From("DomainEvent") }, violation.ResolvedTargetRoles));
        }

        [Fact]
        public void EvaluateRequirement_DoesNotUseDefaultDecisionWhenSatisfied()
        {
            var source = Element("type:OrderAggregate", "OrderAggregate");
            var target = Element("type:OrderCreated", "OrderCreated");
            var requirement = Rule("BRK-REQ", "Aggregate must raise event", "AggregateRoot", "DomainEvent", BrickDecision.Require);
            var allow = Rule("BRK-ALLOW", "Aggregate may publish event", "AggregateRoot", "DomainEvent", BrickDecision.Allow);

            var violations = BrickRuleEvaluator.Evaluate(
                Policy(BrickPermissionDefault.Deny, BrickEnforcementMode.Analyze, requirement, allow),
                new[] { Dependency(source, target) },
                new[] { Resolved(source, "AggregateRoot"), Resolved(target, "DomainEvent") });

            Assert.Empty(violations);
        }

        [Fact]
        public void Evaluate_WithDisabledPolicyOrNullInputs_EmitsNoViolations()
        {
            var policy = Policy(BrickPermissionDefault.Deny, BrickEnforcementMode.Disabled);

            Assert.Empty(BrickRuleEvaluator.Evaluate(policy, null, null));
        }

        [Fact]
        public void Evaluate_WithActivePolicyAndNullInputs_NormalizesCollections()
        {
            var policy = Policy(BrickPermissionDefault.Deny, BrickEnforcementMode.Analyze);

            Assert.Empty(BrickRuleEvaluator.Evaluate(policy, null, null));
        }

        [Fact]
        public void Evaluate_WithDuplicateResolvedRoleSets_KeepsDeterministicFirstEntry()
        {
            var source = Element("type:OrderService", "OrderService");
            var target = Element("type:Contracts", "Contracts");
            var policy = Policy(
                BrickPermissionDefault.Deny,
                BrickEnforcementMode.Analyze,
                Rule("BRK-ALLOW", "Domain may use contracts", "Domain", "Contracts", BrickDecision.Allow));

            var violations = BrickRuleEvaluator.Evaluate(
                policy,
                new[] { Dependency(source, target) },
                new[] { Resolved(source, "Domain"), Resolved(source, "Other"), Resolved(target, "Contracts") });

            Assert.Empty(violations);
        }

        [Fact]
        public void EvaluatePermission_IgnoresRulesWithMismatchingScopeOrRoles()
        {
            var source = Element("type:OrderService", "OrderService");
            var target = Element("type:Contracts", "Contracts");
            var rule = Rule("BRK-ALLOW", "Domain may use contracts", "Domain", "Contracts", BrickDecision.Allow);
            var policy = Policy(BrickPermissionDefault.Allow, BrickEnforcementMode.Analyze, rule);

            var wrongScopeViolations = BrickRuleEvaluator.Evaluate(
                policy,
                new[] { Dependency(source, target, BrickScope.Member) },
                new[] { Resolved(source, "Domain"), Resolved(target, "Contracts") });
            var wrongSourceViolations = BrickRuleEvaluator.Evaluate(
                policy,
                new[] { Dependency(source, target) },
                new[] { Resolved(source, "Application"), Resolved(target, "Contracts") });
            var wrongTargetViolations = BrickRuleEvaluator.Evaluate(
                policy,
                new[] { Dependency(source, target) },
                new[] { Resolved(source, "Domain"), Resolved(target, "Infrastructure") });

            Assert.Empty(wrongScopeViolations);
            Assert.Empty(wrongSourceViolations);
            Assert.Empty(wrongTargetViolations);
        }

        [Fact]
        public void EvaluateRequirement_RequiresMatchingScopeSourceAndTargetRole()
        {
            var source = Element("type:OrderAggregate", "OrderAggregate");
            var otherSource = Element("type:CustomerAggregate", "CustomerAggregate");
            var target = Element("type:OrderCreated", "OrderCreated");
            var wrongTarget = Element("type:SqlGateway", "SqlGateway");
            var otherWrongTarget = Element("type:CacheGateway", "CacheGateway");
            var requirement = Rule("BRK-REQ", "Aggregate must raise event", "AggregateRoot", "DomainEvent", BrickDecision.Require);

            var violations = BrickRuleEvaluator.Evaluate(
                Policy(BrickPermissionDefault.Allow, BrickEnforcementMode.Analyze, requirement),
                new[]
                {
                    Dependency(source, target, BrickScope.Member),
                    Dependency(otherSource, target),
                    Dependency(source, wrongTarget),
                    Dependency(source, otherWrongTarget)
                },
                new[]
                {
                    Resolved(source, "AggregateRoot"),
                    Resolved(otherSource, "Other"),
                    Resolved(target, "DomainEvent"),
                    Resolved(wrongTarget, "Infrastructure"),
                    Resolved(otherWrongTarget, "Infrastructure")
                });

            Assert.Single(violations);
            Assert.Equal(source, violations[0].Source);
            Assert.Equal(BrickViolationKind.RequiredDependency, violations[0].Kind);
        }

        [Fact]
        public void EvaluatePermission_WithMissingResolvedRoles_UsesDefaultDecision()
        {
            var source = Element("type:OrderService", "OrderService");
            var target = Element("type:SqlGateway", "SqlGateway");

            var violations = BrickRuleEvaluator.Evaluate(
                Policy(BrickPermissionDefault.Deny, BrickEnforcementMode.Analyze),
                new[] { Dependency(source, target) },
                null);

            Assert.Single(violations);
            Assert.Empty(violations[0].ResolvedSourceRoles);
            Assert.Empty(violations[0].ResolvedTargetRoles);
        }

        [Fact]
        public void Evaluate_RequiresPolicy()
        {
            Assert.Throws<ArgumentNullException>(() => BrickRuleEvaluator.Evaluate(null, null, null));
        }

        private static BrickPolicy Policy(BrickPermissionDefault defaultDecision, BrickEnforcementMode enforcement, params BrickRule[] rules) =>
            new BrickPolicy(BrickPolicyId.From("Default"), "Default", null, rules, defaultDecision, enforcement);

        private static BrickRule Rule(string id, string name, string sourceRole, string targetRole, BrickDecision decision, int priority = 0) =>
            new BrickRule(
                RuleId.From(id),
                name,
                RoleId.From(sourceRole),
                RoleId.From(targetRole),
                decision,
                BrickScope.Type,
                BrickSeverity.Error,
                priority);

        private static BrickDependency Dependency(BrickElement source, BrickElement target, BrickScope scope = BrickScope.Type) =>
            new BrickDependency(
                source,
                target,
                BrickDependencyKindId.From(BrickDependencyKinds.TypeReference),
                scope,
                BrickDependencyLayer.Static,
                BrickDependencyStrength.Direct,
                BrickEvidenceLevel.CompilerConfirmed);

        private static BrickResolvedRoles Resolved(BrickElement element, params string[] roles) =>
            new BrickResolvedRoles(
                element,
                roles.Select(role => Assignment(role)).ToArray(),
                roles.Select(role => Assignment(role)).ToArray(),
                Enumerable.Empty<BrickRoleAssignment>(),
                Enumerable.Empty<BrickRoleConflict>());

        private static BrickRoleAssignment Assignment(string role) =>
            new BrickRoleAssignment(
                new BrickElementSelector(BrickElementKind.Type, "*"),
                RoleId.From(role),
                BrickAssignmentMode.DirectAttribute,
                BrickAssignmentSource.SourceAttribute,
                new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct),
                BrickAssignmentBehavior.Apply);

        private static BrickElement Element(string id, string displayName) =>
            new BrickElement(BrickElementId.From(id), BrickElementKind.Type, displayName);
    }
}
