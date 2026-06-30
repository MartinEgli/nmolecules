using System;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksConceptModelTest
    {
        [Fact]
        public void EvidenceLevelDefaultsToUnknown()
        {
            Assert.Equal(BrickEvidenceLevel.Unknown, default);
            Assert.Equal(0, (int)BrickEvidenceLevel.Unknown);
            Assert.Equal(1, (int)BrickEvidenceLevel.CompilerConfirmed);
            Assert.Equal(2, (int)BrickEvidenceLevel.AnalyzerInferred);
            Assert.Equal(3, (int)BrickEvidenceLevel.ConfigurationDeclared);
            Assert.Equal(4, (int)BrickEvidenceLevel.RuntimeInferred);
        }

        [Fact]
        public void TypedIdsNormalizeNullAndCompareByValue()
        {
            Assert.True(BrickElementId.From(null).IsEmpty);
            Assert.True(BrickDimensionId.From(null).IsEmpty);
            Assert.True(BrickPolicyId.From(null).IsEmpty);
            Assert.True(BrickDependencyKindId.From(null).IsEmpty);

            Assert.Equal(new BrickElementId("type:Order"), BrickElementId.From("type:Order"));
            Assert.Equal(new BrickDimensionId("Architecture"), BrickDimensionId.From("Architecture"));
            Assert.Equal(new BrickPolicyId("Default"), BrickPolicyId.From("Default"));
            Assert.Equal(new BrickDependencyKindId(BrickDependencyKinds.TypeReference), BrickDependencyKindId.From(BrickDependencyKinds.TypeReference));

            Assert.Equal("type:Order", BrickElementId.From("type:Order"));
            Assert.Equal("Architecture", BrickDimensionId.From("Architecture"));
            Assert.Equal("Default", BrickPolicyId.From("Default"));
            Assert.Equal(BrickDependencyKinds.TypeReference, BrickDependencyKindId.From(BrickDependencyKinds.TypeReference));

            Assert.NotEqual(BrickElementId.From("type:Order"), BrickElementId.From("type:Invoice"));
            Assert.NotEqual(BrickDimensionId.From("Architecture"), BrickDimensionId.From("Business"));
            Assert.NotEqual(BrickPolicyId.From("Default"), BrickPolicyId.From("Strict"));
            Assert.NotEqual(BrickDependencyKindId.From(BrickDependencyKinds.TypeReference), BrickDependencyKindId.From("MethodCall"));
        }

        [Fact]
        public void TypedIdsExposeValueSemantics()
        {
            BrickElementId elementId = "type:Order";
            BrickDimensionId dimensionId = "Architecture";
            BrickPolicyId policyId = "Default";
            BrickDependencyKindId dependencyKindId = BrickDependencyKinds.TypeReference;

            Assert.Equal("type:Order", elementId.ToString());
            Assert.Equal("Architecture", dimensionId.ToString());
            Assert.Equal("Default", policyId.ToString());
            Assert.Equal(BrickDependencyKinds.TypeReference, dependencyKindId.ToString());

            Assert.True(elementId.Equals((object)BrickElementId.From("type:Order")));
            Assert.True(dimensionId.Equals((object)BrickDimensionId.From("Architecture")));
            Assert.True(policyId.Equals((object)BrickPolicyId.From("Default")));
            Assert.True(dependencyKindId.Equals((object)BrickDependencyKindId.From(BrickDependencyKinds.TypeReference)));

            Assert.False(elementId.Equals((object)"type:Order"));
            Assert.False(dimensionId.Equals((object)"Architecture"));
            Assert.False(policyId.Equals((object)"Default"));
            Assert.False(dependencyKindId.Equals((object)BrickDependencyKinds.TypeReference));

            Assert.Equal(BrickElementId.From("type:Order").GetHashCode(), elementId.GetHashCode());
            Assert.Equal(BrickDimensionId.From("Architecture").GetHashCode(), dimensionId.GetHashCode());
            Assert.Equal(BrickPolicyId.From("Default").GetHashCode(), policyId.GetHashCode());
            Assert.Equal(BrickDependencyKindId.From(BrickDependencyKinds.TypeReference).GetHashCode(), dependencyKindId.GetHashCode());

            Assert.True(elementId == BrickElementId.From("type:Order"));
            Assert.True(dimensionId == BrickDimensionId.From("Architecture"));
            Assert.True(policyId == BrickPolicyId.From("Default"));
            Assert.True(dependencyKindId == BrickDependencyKindId.From(BrickDependencyKinds.TypeReference));

            Assert.True(elementId != BrickElementId.From("type:Invoice"));
            Assert.True(dimensionId != BrickDimensionId.From("Business"));
            Assert.True(policyId != BrickPolicyId.From("Strict"));
            Assert.True(dependencyKindId != BrickDependencyKindId.From("MethodCall"));
        }

        [Fact]
        public void BrickRoleDimensionCapturesExclusivityRules()
        {
            var dimension = new BrickRoleDimension(
                BrickDimensionId.From("ArchitectureStyle"),
                "Architecture Style",
                allowsMultipleRoles: false,
                isExclusiveByDefault: true,
                description: "Primary architecture family.");

            Assert.Equal(BrickDimensionId.From("ArchitectureStyle"), dimension.Id);
            Assert.Equal("Architecture Style", dimension.DisplayName);
            Assert.False(dimension.AllowsMultipleRoles);
            Assert.True(dimension.IsExclusiveByDefault);
            Assert.Equal("Primary architecture family.", dimension.Description);
        }

        [Fact]
        public void BrickRoleCapturesRoleMetadata()
        {
            var role = new BrickRole(
                RoleId.From("Architecture.Layer.Domain"),
                BrickDimensionId.From("ArchitectureLayer"),
                "Domain Layer",
                category: "Architecture",
                description: "Domain model and rules.",
                isBuiltin: true);

            Assert.Equal(RoleId.From("Architecture.Layer.Domain"), role.Id);
            Assert.Equal(BrickDimensionId.From("ArchitectureLayer"), role.DimensionId);
            Assert.Equal("Domain Layer", role.DisplayName);
            Assert.Equal("Architecture", role.Category);
            Assert.Equal("Domain model and rules.", role.Description);
            Assert.True(role.IsBuiltin);
        }

        [Fact]
        public void BrickElementCapturesAddressableStructuralArtifact()
        {
            var element = new BrickElement(
                BrickElementId.From("type:Billing.Order"),
                BrickElementKind.Type,
                "Order",
                assemblyName: "Billing",
                namespaceName: "Billing.Domain",
                fullName: "Billing.Domain.Order",
                origin: BrickElementOrigin.Source,
                source: BrickElementSource.Code);

            Assert.Equal(BrickElementId.From("type:Billing.Order"), element.Id);
            Assert.Equal(BrickElementKind.Type, element.Kind);
            Assert.Equal("Order", element.DisplayName);
            Assert.Equal("Billing", element.AssemblyName);
            Assert.Equal("Billing.Domain", element.NamespaceName);
            Assert.Equal("Billing.Domain.Order", element.FullName);
            Assert.Equal(BrickElementOrigin.Source, element.Origin);
            Assert.Equal(BrickElementSource.Code, element.Source);
        }

        [Fact]
        public void BrickDependencyCapturesEvidenceAndScope()
        {
            var source = Element("type:Billing.OrderPolicy", "OrderPolicy");
            var target = Element("type:Billing.SqlGateway", "SqlGateway");
            var location = new BrickSourceLocation("OrderPolicy.cs", 12, 28);
            var dependency = new BrickDependency(
                source,
                target,
                BrickDependencyKindId.From(BrickDependencyKinds.TypeReference),
                BrickScope.Type,
                BrickDependencyLayer.Static,
                BrickDependencyStrength.Direct,
                BrickEvidenceLevel.CompilerConfirmed,
                location,
                "field gateway");

            Assert.Equal(source, dependency.Source);
            Assert.Equal(target, dependency.Target);
            Assert.Equal(BrickDependencyKindId.From(BrickDependencyKinds.TypeReference), dependency.KindId);
            Assert.Equal(BrickScope.Type, dependency.Scope);
            Assert.Equal(BrickDependencyLayer.Static, dependency.Layer);
            Assert.Equal(BrickDependencyStrength.Direct, dependency.Strength);
            Assert.Equal(BrickEvidenceLevel.CompilerConfirmed, dependency.EvidenceLevel);
            Assert.Equal(location, dependency.Location);
            Assert.Equal("field gateway", dependency.Detail);
        }

        [Fact]
        public void BrickSourceLocationExposesValueSemantics()
        {
            var location = new BrickSourceLocation("OrderPolicy.cs", 12, 28);

            Assert.True(location.Equals((object)new BrickSourceLocation("OrderPolicy.cs", 12, 28)));
            Assert.False(location.Equals("OrderPolicy.cs"));
            Assert.Equal(new BrickSourceLocation("OrderPolicy.cs", 12, 28).GetHashCode(), location.GetHashCode());
            Assert.True(location == new BrickSourceLocation("OrderPolicy.cs", 12, 28));
            Assert.True(location != new BrickSourceLocation("OrderPolicy.cs", 13, 28));
        }

        [Fact]
        public void BrickPolicyCopiesRulesAndImports()
        {
            var import = new BrickPolicyImport(BrickPolicyId.From("Base"), BrickPolicyImportMode.Extend);
            var rule = new BrickRule(
                RuleId.From("BRK-001"),
                "No infrastructure in domain",
                RoleId.From("Domain"),
                RoleId.From("Infrastructure"),
                BrickDecision.Deny,
                BrickScope.Type,
                BrickSeverity.Error,
                priority: 10,
                reason: "Keep domain independent.");
            var imports = new[] { import };
            var rules = new[] { rule };
            var policy = new BrickPolicy(
                BrickPolicyId.From("Strict"),
                "Strict Architecture",
                imports,
                rules,
                BrickPermissionDefault.Allow,
                BrickEnforcementMode.Analyze);

            imports[0] = new BrickPolicyImport(BrickPolicyId.From("Other"), BrickPolicyImportMode.Disable);
            rules[0] = new BrickRule(RuleId.From("BRK-999"), "Other", RoleId.From("A"), RoleId.From("B"), BrickDecision.Allow);

            Assert.Equal(BrickPolicyId.From("Strict"), policy.Id);
            Assert.Equal("Strict Architecture", policy.Name);
            Assert.Equal(BrickPermissionDefault.Allow, policy.DefaultDecision);
            Assert.Equal(BrickEnforcementMode.Analyze, policy.Enforcement);
            Assert.Equal(import, policy.Imports.Single());
            Assert.Equal(rule, policy.Rules.Single());
        }

        [Fact]
        public void BrickRuleAndPolicyImportExposeValueSemantics()
        {
            var rule = new BrickRule(RuleId.From("BRK-001"), "Rule", RoleId.From("A"), RoleId.From("B"), BrickDecision.Deny);
            var import = new BrickPolicyImport(BrickPolicyId.From("Base"), BrickPolicyImportMode.Import);

            Assert.True(rule.Equals((object)new BrickRule(RuleId.From("BRK-001"), "Rule", RoleId.From("A"), RoleId.From("B"), BrickDecision.Deny)));
            Assert.False(rule.Equals("Rule"));
            Assert.Equal(rule.RuleId.GetHashCode(), rule.GetHashCode());
            Assert.True(rule == new BrickRule(RuleId.From("BRK-001"), "Rule", RoleId.From("A"), RoleId.From("B"), BrickDecision.Deny));
            Assert.True(rule != new BrickRule(RuleId.From("BRK-002"), "Rule", RoleId.From("A"), RoleId.From("B"), BrickDecision.Deny));

            Assert.True(import.Equals((object)new BrickPolicyImport(BrickPolicyId.From("Base"), BrickPolicyImportMode.Import)));
            Assert.False(import.Equals("Base"));
            Assert.Equal((BrickPolicyId.From("Base").GetHashCode() ^ BrickPolicyImportMode.Import.GetHashCode()), import.GetHashCode());
            Assert.True(import == new BrickPolicyImport(BrickPolicyId.From("Base"), BrickPolicyImportMode.Import));
            Assert.True(import != new BrickPolicyImport(BrickPolicyId.From("Base"), BrickPolicyImportMode.Disable));
        }

        [Fact]
        public void BrickViolationNormalizesRuleEvaluationOutput()
        {
            var source = Element("type:Billing.OrderPolicy", "OrderPolicy");
            var target = Element("type:Billing.SqlGateway", "SqlGateway");
            var violation = new BrickViolation(
                BrickViolationKind.DependencyRule,
                source,
                "Domain must not use infrastructure.",
                BrickSeverity.Error,
                BrickViolationState.Active,
                RuleId.From("BRK-001"),
                "No infrastructure in domain",
                target,
                new[] { RoleId.From("Domain") },
                new[] { RoleId.From("Infrastructure") },
                BrickDependencyKindId.From(BrickDependencyKinds.TypeReference),
                BrickScope.Type,
                BrickDependencyLayer.Static,
                BrickEvidenceLevel.CompilerConfirmed);

            Assert.Equal(BrickViolationKind.DependencyRule, violation.Kind);
            Assert.Equal(RuleId.From("BRK-001"), violation.RuleId);
            Assert.Equal("No infrastructure in domain", violation.RuleName);
            Assert.Equal(source, violation.Source);
            Assert.Equal(target, violation.Target);
            Assert.Equal(new[] { RoleId.From("Domain") }, violation.ResolvedSourceRoles);
            Assert.Equal(new[] { RoleId.From("Infrastructure") }, violation.ResolvedTargetRoles);
            Assert.Equal(BrickDependencyKindId.From(BrickDependencyKinds.TypeReference), violation.DependencyKindId);
            Assert.Equal(BrickScope.Type, violation.Scope);
            Assert.Equal(BrickDependencyLayer.Static, violation.DependencyLayer);
            Assert.Equal(BrickSeverity.Error, violation.Severity);
            Assert.Equal("Domain must not use infrastructure.", violation.Message);
            Assert.Equal(BrickEvidenceLevel.CompilerConfirmed, violation.EvidenceLevel);
            Assert.Equal(BrickViolationState.Active, violation.State);
        }

        [Fact]
        public void ConstructorsNormalizeNullCollectionsAndText()
        {
            var role = new BrickRole(default, default, null, null, null, false);
            var dimension = new BrickRoleDimension(default, null, false, false, null);
            var element = new BrickElement(default, BrickElementKind.Unknown, null, null, null, null, BrickElementOrigin.Unknown, BrickElementSource.Unknown);
            var policy = new BrickPolicy(default, null, null, null, BrickPermissionDefault.Deny, BrickEnforcementMode.Disabled);
            var violation = new BrickViolation(BrickViolationKind.RoleResolution, element, null, BrickSeverity.Info, BrickViolationState.Suppressed);

            Assert.Equal(string.Empty, role.DisplayName);
            Assert.Null(role.Category);
            Assert.Null(role.Description);
            Assert.Equal(string.Empty, dimension.DisplayName);
            Assert.Null(dimension.Description);
            Assert.Equal(string.Empty, element.DisplayName);
            Assert.Null(element.AssemblyName);
            Assert.Empty(policy.Imports);
            Assert.Empty(policy.Rules);
            Assert.Equal(string.Empty, policy.Name);
            Assert.Equal(string.Empty, violation.Message);
            Assert.Empty(violation.ResolvedSourceRoles);
            Assert.Empty(violation.ResolvedTargetRoles);
        }

        [Fact]
        public void GuardAndDefaultBranchesAreExplicitlyCovered()
        {
            var source = Element("type:Billing.Source", "Source");
            var target = Element("type:Billing.Target", "Target");
            var nullNameRule = new BrickRule(RuleId.From("BRK-NULL"), null, RoleId.From("A"), RoleId.From("B"), BrickDecision.Allow);
            var import = new BrickPolicyImport(BrickPolicyId.From("Base"), BrickPolicyImportMode.Import);

            Assert.Equal(string.Empty, nullNameRule.Name);
            Assert.Equal(new BrickElementId(null).GetHashCode(), default(BrickElementId).GetHashCode());
            Assert.Equal(new BrickDimensionId(null).GetHashCode(), default(BrickDimensionId).GetHashCode());
            Assert.Equal(new BrickPolicyId(null).GetHashCode(), default(BrickPolicyId).GetHashCode());
            Assert.Equal(new BrickDependencyKindId(null).GetHashCode(), default(BrickDependencyKindId).GetHashCode());
            Assert.Equal(new BrickSourceLocation(null, 0, 0).GetHashCode(), default(BrickSourceLocation).GetHashCode());
            Assert.False(new BrickSourceLocation("a", 1, 1).Equals(default(BrickSourceLocation)));
            Assert.False(import.Equals((object)"Base"));
            Assert.False(import.Equals(new BrickPolicyImport(BrickPolicyId.From("Other"), BrickPolicyImportMode.Import)));
            Assert.Throws<ArgumentNullException>(() => new BrickDependency(null, target, default, BrickScope.Type, BrickDependencyLayer.Static, BrickDependencyStrength.Direct, BrickEvidenceLevel.Unknown));
            Assert.Throws<ArgumentNullException>(() => new BrickDependency(source, null, default, BrickScope.Type, BrickDependencyLayer.Static, BrickDependencyStrength.Direct, BrickEvidenceLevel.Unknown));
            Assert.Throws<ArgumentNullException>(() => new BrickViolation(BrickViolationKind.DependencyRule, null, "message", BrickSeverity.Error, BrickViolationState.Active));
        }

        private static BrickElement Element(string id, string displayName) =>
            new BrickElement(
                BrickElementId.From(id),
                BrickElementKind.Type,
                displayName,
                assemblyName: "Billing",
                namespaceName: "Billing.Domain",
                fullName: displayName,
                origin: BrickElementOrigin.Source,
                source: BrickElementSource.Code);
    }
}
