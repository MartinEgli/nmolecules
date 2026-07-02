using System;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksPolicyModelTest
    {
        [Fact]
        public void BrickAliasMapsExternalShapeToCanonicalRole()
        {
            var selector = new BrickElementSelector(BrickElementKind.Type, "MediatR.IRequestHandler<,>", "MediatR");
            var alias = new BrickAlias(
                "MediatR handler",
                selector,
                RoleId.From("Architecture.Application.Handler"),
                new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.External),
                BrickAssignmentBehavior.Apply,
                "Adapter pack maps MediatR handlers to application handlers.");

            Assert.Equal("MediatR handler", alias.Name);
            Assert.Equal(selector, alias.Selector);
            Assert.Equal(RoleId.From("Architecture.Application.Handler"), alias.CanonicalRoleId);
            Assert.Equal(new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.External), alias.Precedence);
            Assert.Equal(BrickAssignmentBehavior.Apply, alias.Behavior);
            Assert.Equal("Adapter pack maps MediatR handlers to application handlers.", alias.Reason);
        }

        [Fact]
        public void BrickAliasNormalizesNullTextAndSelector()
        {
            var alias = new BrickAlias(null, null, default, default, BrickAssignmentBehavior.Suppress);

            Assert.Equal(string.Empty, alias.Name);
            Assert.Equal(default, alias.Selector);
            Assert.Equal(default, alias.CanonicalRoleId);
            Assert.Equal(default, alias.Precedence);
            Assert.Equal(BrickAssignmentBehavior.Suppress, alias.Behavior);
            Assert.Null(alias.Reason);
        }

        [Fact]
        public void BrickPolicyCopiesAllCompositionMembers()
        {
            var import = new BrickPolicyImport(BrickPolicyId.From("Base"), BrickPolicyImportMode.Extend);
            var rule = new BrickRule(RuleId.From("BRK-001"), "No infrastructure", RoleId.From("Domain"), RoleId.From("Infrastructure"), BrickDecision.Deny);
            var combinationRule = new BrickRoleCombinationRule(
                "No domain and infrastructure",
                BrickRoleSelector.From("Domain"),
                BrickRoleSelector.From("Infrastructure"),
                BrickCombinationKind.Incompatible);
            var assignment = new BrickRoleAssignment(
                new BrickElementSelector(BrickElementKind.Namespace, "Billing.Domain.*"),
                RoleId.From("Domain"),
                BrickAssignmentMode.ExternalConfiguration,
                BrickAssignmentSource.PolicyFile,
                new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Namespace, BrickAssignmentAuthority.External),
                BrickAssignmentBehavior.Apply);
            var alias = new BrickAlias(
                "Repository alias",
                new BrickElementSelector(BrickElementKind.Type, "*.Repository"),
                RoleId.From("DDD.Repository"),
                new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Alias),
                BrickAssignmentBehavior.Apply);
            var imports = new[] { import };
            var rules = new[] { rule };
            var combinationRules = new[] { combinationRule };
            var externalAssignments = new[] { assignment };
            var aliases = new[] { alias };

            var policy = new BrickPolicy(
                BrickPolicyId.From("Strict"),
                "Strict",
                imports,
                rules,
                combinationRules,
                externalAssignments,
                aliases,
                BrickPermissionDefault.Deny,
                BrickEnforcementMode.Analyze);

            imports[0] = new BrickPolicyImport(BrickPolicyId.From("Other"), BrickPolicyImportMode.Disable);
            rules[0] = new BrickRule(RuleId.From("BRK-999"), "Other", RoleId.From("A"), RoleId.From("B"), BrickDecision.Allow);
            combinationRules[0] = new BrickRoleCombinationRule("Other", BrickRoleSelector.From("*"), BrickRoleSelector.From("*"), BrickCombinationKind.Additive);
            externalAssignments[0] = new BrickRoleAssignment(null, RoleId.From("Other"), BrickAssignmentMode.AliasMapping, BrickAssignmentSource.Convention, default, BrickAssignmentBehavior.Suppress);
            aliases[0] = new BrickAlias("Other", null, RoleId.From("Other"), default, BrickAssignmentBehavior.Suppress);

            Assert.Equal(import, policy.Imports.Single());
            Assert.Equal(rule, policy.Rules.Single());
            Assert.Same(combinationRule, policy.CombinationRules.Single());
            Assert.Same(assignment, policy.ExternalAssignments.Single());
            Assert.Same(alias, policy.Aliases.Single());
            Assert.Equal(BrickPermissionDefault.Deny, policy.DefaultDecision);
            Assert.Equal(BrickEnforcementMode.Analyze, policy.Enforcement);
        }

        [Fact]
        public void BrickPolicyKeepsLegacyConstructorWithEmptyCompositionMembers()
        {
            var policy = new BrickPolicy(
                BrickPolicyId.From("Default"),
                null,
                null,
                null,
                BrickPermissionDefault.Allow,
                BrickEnforcementMode.Disabled);

            Assert.Equal(string.Empty, policy.Name);
            Assert.Empty(policy.Imports);
            Assert.Empty(policy.Rules);
            Assert.Empty(policy.CombinationRules);
            Assert.Empty(policy.ExternalAssignments);
            Assert.Empty(policy.Aliases);
        }

        [Fact]
        public void BrickPolicyDocumentCarriesSchemaVersionAndPolicy()
        {
            var policy = new BrickPolicy(
                BrickPolicyId.From("DefaultArchitecturePolicy"),
                "Default Architecture Policy",
                null,
                null,
                BrickPermissionDefault.Deny,
                BrickEnforcementMode.Analyze);

            var document = new BrickPolicyDocument(policy);

            Assert.Equal(BrickPolicyDocument.CurrentSchema, document.Schema);
            Assert.True(document.IsCurrentSchema);
            Assert.Same(policy, document.Policy);
        }

        [Fact]
        public void BrickPolicyDocumentNormalizesNullSchemaButRequiresPolicy()
        {
            var policy = new BrickPolicy(default, null, null, null, BrickPermissionDefault.Deny, BrickEnforcementMode.Disabled);
            var document = new BrickPolicyDocument(policy, null);

            Assert.Equal(string.Empty, document.Schema);
            Assert.False(document.IsCurrentSchema);
            Assert.Throws<ArgumentNullException>(() => new BrickPolicyDocument(null));
        }

        [Fact]
        public void BrickPolicyDocumentValidatorAcceptsCurrentSchema()
        {
            var policy = new BrickPolicy(default, null, null, null, BrickPermissionDefault.Deny, BrickEnforcementMode.Disabled);
            var document = new BrickPolicyDocument(policy);

            var issues = BrickPolicyDocumentValidator.Validate(document);

            Assert.Empty(issues);
        }

        [Fact]
        public void BrickPolicyDocumentValidatorRejectsMissingDocument()
        {
            var issue = BrickPolicyDocumentValidator.Validate(null).Single();

            Assert.Equal(BrickPolicyDocumentValidator.MissingDocumentRuleId, issue.RuleId);
            Assert.Equal(BrickSeverity.Error, issue.Severity);
            Assert.Equal("Policy document is required.", issue.Message);
        }

        [Fact]
        public void BrickPolicyDocumentValidatorRejectsUnsupportedSchema()
        {
            var policy = new BrickPolicy(default, null, null, null, BrickPermissionDefault.Deny, BrickEnforcementMode.Disabled);
            var document = new BrickPolicyDocument(policy, "NMolecules.Bricks.Policy/0.9");

            var issue = BrickPolicyDocumentValidator.Validate(document).Single();

            Assert.Equal(BrickPolicyDocumentValidator.UnsupportedSchemaRuleId, issue.RuleId);
            Assert.Equal(BrickSeverity.Error, issue.Severity);
            Assert.Contains("NMolecules.Bricks.Policy/0.9", issue.Message);
            Assert.Contains(BrickPolicyDocument.CurrentSchema, issue.Message);
        }

        [Fact]
        public void BrickPolicyDocumentValidatorReportsRuleDuplicatesAndDecisionConflicts()
        {
            var duplicateId = Rule("BRK-DUP", BrickDecision.Allow, priority: 0);
            var duplicateSignature = Rule("BRK-DUP-SIGNATURE", BrickDecision.Allow, priority: 0);
            var denyConflict = Rule("BRK-DENY", BrickDecision.Deny, priority: 10);
            var allowConflict = Rule("BRK-ALLOW", BrickDecision.Allow, priority: 20);
            var policy = new BrickPolicy(
                BrickPolicyId.From("Policy"),
                "Policy",
                null,
                new[] { duplicateId, duplicateId, duplicateSignature, denyConflict, allowConflict },
                BrickPermissionDefault.Allow,
                BrickEnforcementMode.Analyze);

            var issues = BrickPolicyDocumentValidator.Validate(new BrickPolicyDocument(policy));

            Assert.Contains(issues, issue => issue.RuleId == BrickPolicyDocumentValidator.DuplicateRuleId);
            Assert.Contains(issues, issue => issue.RuleId == BrickPolicyDocumentValidator.DuplicateRuleSignatureRuleId);
            Assert.Contains(issues, issue => issue.RuleId == BrickPolicyDocumentValidator.ConflictingRuleDecisionRuleId);
        }

        [Fact]
        public void BrickPolicyDocumentValidatorReportsRoleCombinationKindConflicts()
        {
            var additive = new BrickRoleCombinationRule(
                "additive",
                BrickRoleSelector.From("Domain"),
                BrickRoleSelector.From("Infrastructure"),
                BrickCombinationKind.Additive);
            var incompatible = new BrickRoleCombinationRule(
                "incompatible",
                BrickRoleSelector.From("Infrastructure"),
                BrickRoleSelector.From("Domain"),
                BrickCombinationKind.Incompatible);
            var policy = new BrickPolicy(
                BrickPolicyId.From("Policy"),
                "Policy",
                null,
                null,
                new[] { additive, incompatible },
                null,
                null,
                BrickPermissionDefault.Allow,
                BrickEnforcementMode.Analyze);

            var issue = BrickPolicyDocumentValidator.Validate(new BrickPolicyDocument(policy)).Single();

            Assert.Equal(BrickPolicyDocumentValidator.ConflictingRoleCombinationRuleId, issue.RuleId);
            Assert.Contains("conflicting kinds", issue.Message);
        }

        [Fact]
        public void BrickPolicyDocumentIssueNormalizesNullMessage()
        {
            var issue = new BrickPolicyDocumentIssue(RuleId.From("XMoleculesBricks0201"), BrickSeverity.Warning, null);

            Assert.Equal(RuleId.From("XMoleculesBricks0201"), issue.RuleId);
            Assert.Equal(BrickSeverity.Warning, issue.Severity);
            Assert.Equal(string.Empty, issue.Message);
        }

        private static BrickRule Rule(string id, BrickDecision decision, int priority) =>
            new BrickRule(
                RuleId.From(id),
                id,
                RoleId.From("Source"),
                RoleId.From("Target"),
                decision,
                BrickScope.Type,
                BrickSeverity.Error,
                priority);
    }
}
