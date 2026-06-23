using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksRuntimeActivationModelTest
    {
        [Fact]
        public void RuntimeActivationCreatesRuntimeDependency()
        {
            var site = Type("Billing.Infrastructure.PluginHost");
            var activated = Type("Billing.Domain.OrderPolicy");
            var activation = new BrickRuntimeActivation(
                site,
                activated,
                "ActivatorUtilities.CreateInstance",
                "Plugin activation boundary.",
                BrickEvidenceLevel.ConfigurationDeclared);

            var dependency = activation.ToDependency();

            Assert.Equal(site, activation.ActivationSite);
            Assert.Equal(activated, activation.ActivatedType);
            Assert.Equal("ActivatorUtilities.CreateInstance", activation.ActivationPattern);
            Assert.Equal("Plugin activation boundary.", activation.Justification);
            Assert.Equal(BrickEvidenceLevel.ConfigurationDeclared, activation.EvidenceLevel);
            Assert.Equal(site, dependency.Source);
            Assert.Equal(activated, dependency.Target);
            Assert.Equal(BrickDependencyKindId.From("RuntimeActivation"), dependency.KindId);
            Assert.Equal(BrickScope.Type, dependency.Scope);
            Assert.Equal(BrickDependencyLayer.Runtime, dependency.Layer);
            Assert.Equal(BrickDependencyStrength.Inferred, dependency.Strength);
            Assert.Equal(BrickEvidenceLevel.ConfigurationDeclared, dependency.EvidenceLevel);
            Assert.Equal("ActivatorUtilities.CreateInstance", dependency.Detail);
        }

        [Fact]
        public void RuntimeActivationNormalizesPatternAndRequiresElements()
        {
            var site = Type("Site");
            var activated = Type("Activated");
            var activation = new BrickRuntimeActivation(site, activated, null);

            Assert.Equal(string.Empty, activation.ActivationPattern);
            Assert.Null(activation.Justification);
            Assert.Equal(BrickEvidenceLevel.RuntimeInferred, activation.EvidenceLevel);
            Assert.Throws<ArgumentNullException>(() => new BrickRuntimeActivation(null, activated, "Activator"));
            Assert.Throws<ArgumentNullException>(() => new BrickRuntimeActivation(site, null, "Activator"));
        }

        [Fact]
        public void RuntimeActivationPolicyUsesDefaultActivatorRoles()
        {
            var policy = new BrickRuntimeActivationPolicy();

            Assert.Equal(new[] { RoleId.From("Infrastructure"), RoleId.From("Platform") }, policy.AllowedActivationSiteRoles.ToArray());
            Assert.True(policy.RequireJustification);
            Assert.True(policy.Enabled);
        }

        [Fact]
        public void RuntimeActivationPolicyCopiesAndDistinctsCustomRoles()
        {
            var roles = new[] { RoleId.From("PluginHost"), RoleId.From("PluginHost"), RoleId.From("CompositionRoot") };

            var policy = new BrickRuntimeActivationPolicy(roles, false, false);
            roles[0] = RoleId.From("Mutated");

            Assert.Equal(new[] { RoleId.From("PluginHost"), RoleId.From("CompositionRoot") }, policy.AllowedActivationSiteRoles.ToArray());
            Assert.False(policy.RequireJustification);
            Assert.False(policy.Enabled);
        }

        [Fact]
        public void EvaluateActivationsAllowsInfrastructureActivationSite()
        {
            var activation = Activation("Billing.Infrastructure.PluginHost", "Runtime boundary.");
            var roles = Roles(activation.ActivationSite, RoleId.From("Infrastructure"));

            var violations = BrickRuntimeActivationEvaluator.EvaluateActivations(new[] { activation }, roles);

            Assert.Empty(violations);
        }

        [Fact]
        public void EvaluateActivationsReportsActivationOutsideAllowedRole()
        {
            var activation = Activation("Billing.Domain.Order", "Runtime boundary.");
            var roles = Roles(activation.ActivationSite, RoleId.From("DDD.Entity"));

            var violation = BrickRuntimeActivationEvaluator.EvaluateActivations(new[] { activation }, roles).Single();

            Assert.Equal(BrickRuntimeActivationEvaluator.ActivationSiteRoleRuleId, violation.RuleId);
            Assert.Equal(BrickViolationKind.DependencyRule, violation.Kind);
            Assert.Equal(activation.ActivationSite, violation.Source);
            Assert.Equal(activation.ActivatedType, violation.Target);
            Assert.Equal(BrickSeverity.Warning, violation.Severity);
            Assert.Equal(BrickViolationState.Active, violation.State);
            Assert.Equal("Runtime activation", violation.RuleName);
            Assert.Equal("Runtime activation must be owned by an allowed activator role.", violation.Message);
            Assert.Equal(new[] { RoleId.From("DDD.Entity") }, violation.ResolvedSourceRoles.ToArray());
            Assert.Empty(violation.ResolvedTargetRoles);
            Assert.Equal(BrickDependencyKindId.From("RuntimeActivation"), violation.DependencyKindId);
            Assert.Equal(BrickScope.Type, violation.Scope);
            Assert.Equal(BrickDependencyLayer.Runtime, violation.DependencyLayer);
            Assert.Equal(BrickEvidenceLevel.RuntimeInferred, violation.EvidenceLevel);
            Assert.Equal("Runtime boundary.", violation.StateReason);
        }

        [Fact]
        public void EvaluateActivationsReportsMissingJustificationWhenRequired()
        {
            var activation = Activation("Billing.Infrastructure.PluginHost", " ");
            var roles = Roles(activation.ActivationSite, RoleId.From("Infrastructure"));

            var violation = BrickRuntimeActivationEvaluator.EvaluateActivations(new[] { activation }, roles).Single();

            Assert.Equal(BrickRuntimeActivationEvaluator.ActivationJustificationRuleId, violation.RuleId);
            Assert.Equal("Runtime activation must be justified.", violation.Message);
        }

        [Fact]
        public void EvaluateActivationsCanReportRoleAndJustificationTogether()
        {
            var activation = new BrickRuntimeActivation(
                Type("Billing.Domain.Order"),
                Type("Billing.Domain.OrderPolicy"),
                "Activator.CreateInstance");

            var violations = BrickRuntimeActivationEvaluator.EvaluateActivations(new[] { activation }, null);

            Assert.Equal(
                new[] { BrickRuntimeActivationEvaluator.ActivationSiteRoleRuleId, BrickRuntimeActivationEvaluator.ActivationJustificationRuleId },
                violations.Select(violation => violation.RuleId.Value).ToArray());
        }

        [Fact]
        public void EvaluateActivationsHonorsCustomPolicyAndNullRoleList()
        {
            var activation = Activation("Billing.Tools.PluginHost", "Runtime boundary.");
            var roles = new Dictionary<BrickElementId, IEnumerable<RoleId>>
            {
                [activation.ActivationSite.Id] = null
            };
            var policy = new BrickRuntimeActivationPolicy(new[] { RoleId.From("PluginHost") }, false);

            var violation = BrickRuntimeActivationEvaluator.EvaluateActivations(new[] { activation }, roles, policy).Single();

            Assert.Equal(BrickRuntimeActivationEvaluator.ActivationSiteRoleRuleId, violation.RuleId);
            Assert.Empty(violation.ResolvedSourceRoles);
        }

        [Fact]
        public void EvaluateActivationsNormalizesNullInputsAndDisabledPolicy()
        {
            Assert.Empty(BrickRuntimeActivationEvaluator.EvaluateActivations(null, null));
            Assert.Empty(BrickRuntimeActivationEvaluator.EvaluateActivations(
                new[] { Activation("Billing.Domain.Order", null) },
                null,
                new BrickRuntimeActivationPolicy(enabled: false)));
        }

        private static BrickRuntimeActivation Activation(string siteName, string justification) =>
            new BrickRuntimeActivation(
                Type(siteName),
                Type("Billing.Domain.OrderPolicy"),
                "ActivatorUtilities.CreateInstance",
                justification);

        private static Dictionary<BrickElementId, IEnumerable<RoleId>> Roles(BrickElement element, params RoleId[] roles) =>
            new Dictionary<BrickElementId, IEnumerable<RoleId>>
            {
                [element.Id] = roles
            };

        private static BrickElement Type(string name) =>
            new BrickElement(BrickElementId.From("type:" + name), BrickElementKind.Type, name, "Billing", Namespace(name), name);

        private static string Namespace(string fullName)
        {
            var lastDot = fullName.LastIndexOf('.');
            return lastDot < 0 ? string.Empty : fullName.Substring(0, lastDot);
        }
    }
}
