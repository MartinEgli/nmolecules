using System;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksPolicyCompositionModelTest
    {
        [Fact]
        public void ComposeImportsPoliciesBeforeLocalRulesAndClearsImports()
        {
            var imported = Policy("Base", Rule("BRK-001", BrickDecision.Deny, BrickSeverity.Error));
            var root = Policy(
                "Root",
                new[] { new BrickPolicyImport(BrickPolicyId.From("Base"), BrickPolicyImportMode.Import) },
                Rule("BRK-002", BrickDecision.Allow, BrickSeverity.Info));

            var result = BrickPolicyComposer.Compose(root, new[] { imported });

            Assert.Empty(result.Issues);
            Assert.Equal(new[] { RuleId.From("BRK-001"), RuleId.From("BRK-002") }, result.Policy.Rules.Select(rule => rule.RuleId).ToArray());
            Assert.Empty(result.Policy.Imports);
            Assert.Equal(BrickPolicyId.From("Root"), result.Policy.Id);
            Assert.Equal("Root", result.Policy.Name);
            Assert.Equal(BrickPermissionDefault.Deny, result.Policy.DefaultDecision);
            Assert.Equal(BrickEnforcementMode.Analyze, result.Policy.Enforcement);
            Assert.Equal(new[] { BrickPolicyId.From("Base"), BrickPolicyId.From("Root") }, result.Steps.Select(step => step.PolicyId).ToArray());
            Assert.All(result.Steps, step => Assert.True(step.Applied));
        }

        [Fact]
        public void ComposeRecursesImportsInDeclaredOrder()
        {
            var platform = Policy("Platform", Rule("BRK-001"));
            var basePolicy = Policy(
                "Base",
                new[] { new BrickPolicyImport(BrickPolicyId.From("Platform"), BrickPolicyImportMode.Extend) },
                Rule("BRK-002"));
            var root = Policy(
                "Root",
                new[] { new BrickPolicyImport(BrickPolicyId.From("Base"), BrickPolicyImportMode.Extend) },
                Rule("BRK-003"));

            var result = BrickPolicyComposer.Compose(root, new[] { platform, basePolicy });

            Assert.Equal(new[] { RuleId.From("BRK-001"), RuleId.From("BRK-002"), RuleId.From("BRK-003") }, result.Policy.Rules.Select(rule => rule.RuleId).ToArray());
            Assert.Equal(new[] { BrickPolicyId.From("Platform"), BrickPolicyId.From("Base"), BrickPolicyId.From("Root") }, result.Steps.Select(step => step.PolicyId).ToArray());
        }

        [Fact]
        public void ComposeOverrideReplacesImportedRuleWithLocalRule()
        {
            var imported = Policy("Base", Rule("BRK-001", BrickDecision.Allow, BrickSeverity.Info));
            var local = Rule("BRK-001", BrickDecision.Deny, BrickSeverity.Error);
            var additional = Rule("BRK-002", BrickDecision.Require, BrickSeverity.Warning);
            var root = Policy(
                "Root",
                new[] { new BrickPolicyImport(BrickPolicyId.From("Base"), BrickPolicyImportMode.Override) },
                local,
                additional);

            var result = BrickPolicyComposer.Compose(root, new[] { imported });

            Assert.Equal(new[] { local, additional }, result.Policy.Rules.ToArray());
        }

        [Fact]
        public void ComposeDisableSkipsImportedPolicy()
        {
            var imported = Policy("Base", Rule("BRK-001"));
            var root = Policy(
                "Root",
                new[] { new BrickPolicyImport(BrickPolicyId.From("Base"), BrickPolicyImportMode.Disable) },
                Rule("BRK-002"));

            var result = BrickPolicyComposer.Compose(root, new[] { imported });

            Assert.Equal(new[] { RuleId.From("BRK-002") }, result.Policy.Rules.Select(rule => rule.RuleId).ToArray());
            var disabled = result.Steps.Single(step => step.PolicyId == BrickPolicyId.From("Base"));
            Assert.Equal(BrickPolicyImportMode.Disable, disabled.Mode);
            Assert.False(disabled.Applied);
        }

        [Fact]
        public void ComposeNarrowReplacesImportedRuleOnlyWhenLocalRuleIsStricter()
        {
            var imported = Policy(
                "Base",
                Rule("BRK-001", BrickDecision.Allow, BrickSeverity.Info),
                Rule("BRK-002", BrickDecision.Deny, BrickSeverity.Error),
                Rule("BRK-004", BrickDecision.Allow, BrickSeverity.Info));
            var stricter = Rule("BRK-001", BrickDecision.Deny, BrickSeverity.Warning);
            var weaker = Rule("BRK-002", BrickDecision.Allow, BrickSeverity.Info);
            var additional = Rule("BRK-003", BrickDecision.Require, BrickSeverity.Warning);
            var required = Rule("BRK-004", BrickDecision.Require, BrickSeverity.Warning);
            var root = Policy(
                "Root",
                new[] { new BrickPolicyImport(BrickPolicyId.From("Base"), BrickPolicyImportMode.Narrow) },
                stricter,
                weaker,
                additional,
                required);

            var result = BrickPolicyComposer.Compose(root, new[] { imported });

            Assert.Equal(new[] { stricter, imported.Rules[1], required, additional }, result.Policy.Rules.ToArray());
            var issue = result.Issues.Single();
            Assert.Equal(BrickPolicyComposer.WeakerNarrowingRuleId, issue.RuleId);
            Assert.Contains("BRK-002", issue.Message);
        }

        [Fact]
        public void ComposeCopiesCompositionMembers()
        {
            var combination = new BrickRoleCombinationRule(
                "No domain infrastructure mix",
                BrickRoleSelector.From("Domain"),
                BrickRoleSelector.From("Infrastructure"),
                BrickCombinationKind.Incompatible);
            var assignment = new BrickRoleAssignment(
                new BrickElementSelector(BrickElementKind.Namespace, "Billing.Domain.*"),
                RoleId.From("Domain"),
                BrickAssignmentMode.ExternalConfiguration,
                BrickAssignmentSource.PolicyFile,
                default,
                BrickAssignmentBehavior.Apply);
            var alias = new BrickAlias("Entity", new BrickElementSelector(BrickElementKind.Type, "*.Entity"), RoleId.From("DDD.Entity"), default, BrickAssignmentBehavior.Apply);
            var imported = new BrickPolicy(
                BrickPolicyId.From("Base"),
                "Base",
                null,
                null,
                new[] { combination },
                new[] { assignment },
                new[] { alias },
                BrickPermissionDefault.Allow,
                BrickEnforcementMode.Document);
            var root = Policy("Root", new[] { new BrickPolicyImport(BrickPolicyId.From("Base"), BrickPolicyImportMode.Import) });

            var result = BrickPolicyComposer.Compose(root, new[] { imported });

            Assert.Same(combination, result.Policy.CombinationRules.Single());
            Assert.Same(assignment, result.Policy.ExternalAssignments.Single());
            Assert.Same(alias, result.Policy.Aliases.Single());
        }

        [Fact]
        public void ComposeReportsMissingImports()
        {
            var root = Policy(
                "Root",
                new[] { new BrickPolicyImport(BrickPolicyId.From("Missing"), BrickPolicyImportMode.Import) },
                Rule("BRK-001"));

            var result = BrickPolicyComposer.Compose(root, null);

            var issue = result.Issues.Single();
            Assert.Equal(BrickPolicyComposer.MissingPolicyRuleId, issue.RuleId);
            Assert.Equal(BrickSeverity.Error, issue.Severity);
            Assert.Contains("Missing", issue.Message);
            Assert.Equal(new[] { RuleId.From("BRK-001") }, result.Policy.Rules.Select(rule => rule.RuleId).ToArray());
        }

        [Fact]
        public void ComposeReportsCircularImports()
        {
            var alpha = Policy(
                "Alpha",
                new[] { new BrickPolicyImport(BrickPolicyId.From("Beta"), BrickPolicyImportMode.Import) },
                Rule("BRK-001"));
            var beta = Policy(
                "Beta",
                new[] { new BrickPolicyImport(BrickPolicyId.From("Alpha"), BrickPolicyImportMode.Import) },
                Rule("BRK-002"));

            var result = BrickPolicyComposer.Compose(alpha, new[] { alpha, beta });

            var issue = result.Issues.Single();
            Assert.Equal(BrickPolicyComposer.CircularImportRuleId, issue.RuleId);
            Assert.Contains("Alpha", issue.Message);
            Assert.Equal(new[] { RuleId.From("BRK-002"), RuleId.From("BRK-001") }, result.Policy.Rules.Select(rule => rule.RuleId).ToArray());
        }

        [Fact]
        public void ComposeRequiresRootPolicyAndNormalizesCatalog()
        {
            Assert.Throws<ArgumentNullException>(() => BrickPolicyComposer.Compose(null, null));

            var root = Policy("Root", Rule("BRK-001"));
            var result = BrickPolicyComposer.Compose(root, new BrickPolicy[] { null });

            Assert.Equal(RuleId.From("BRK-001"), result.Policy.Rules.Single().RuleId);
        }

        [Fact]
        public void CompositionResultNormalizesCollectionsButRequiresPolicy()
        {
            var policy = Policy("Root", Rule("BRK-001"));
            var result = new BrickPolicyCompositionResult(policy, null, null);

            Assert.Same(policy, result.Policy);
            Assert.Empty(result.Steps);
            Assert.Empty(result.Issues);
            Assert.Throws<ArgumentNullException>(() => new BrickPolicyCompositionResult(null, null, null));
        }

        private static BrickPolicy Policy(string id, params BrickRule[] rules) =>
            Policy(id, null, rules);

        private static BrickPolicy Policy(string id, BrickPolicyImport[] imports, params BrickRule[] rules) =>
            new BrickPolicy(
                BrickPolicyId.From(id),
                id,
                imports,
                rules,
                BrickPermissionDefault.Deny,
                BrickEnforcementMode.Analyze);

        private static BrickRule Rule(
            string id,
            BrickDecision decision = BrickDecision.Deny,
            BrickSeverity severity = BrickSeverity.Error) =>
            new BrickRule(
                RuleId.From(id),
                id,
                RoleId.From("Source"),
                RoleId.From("Target"),
                decision,
                BrickScope.Type,
                severity);
    }
}
