using System;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksReflectionModelTest
    {
        [Fact]
        public void ReflectionAccessCreatesRuntimeDependency()
        {
            var site = Type("Billing.Infrastructure.ReflectionActivator");
            var target = Type("Billing.Domain.Order");
            var access = new BrickReflectionAccess(
                site,
                target,
                "Activator.CreateInstance",
                BrickReflectionConfidence.High,
                "Serializer boundary.",
                BrickEvidenceLevel.ConfigurationDeclared);

            var dependency = access.ToDependency();

            Assert.Equal(site, access.AccessSite);
            Assert.Equal(target, access.Target);
            Assert.Equal("Activator.CreateInstance", access.AccessPattern);
            Assert.Equal(BrickReflectionConfidence.High, access.Confidence);
            Assert.Equal("Serializer boundary.", access.Justification);
            Assert.Equal(BrickEvidenceLevel.ConfigurationDeclared, access.EvidenceLevel);
            Assert.Equal(site, dependency.Source);
            Assert.Equal(target, dependency.Target);
            Assert.Equal(BrickDependencyKindId.From("ReflectionAccess"), dependency.KindId);
            Assert.Equal(BrickScope.Type, dependency.Scope);
            Assert.Equal(BrickDependencyLayer.Runtime, dependency.Layer);
            Assert.Equal(BrickDependencyStrength.Inferred, dependency.Strength);
            Assert.Equal(BrickEvidenceLevel.ConfigurationDeclared, dependency.EvidenceLevel);
            Assert.Equal("Activator.CreateInstance", dependency.Detail);
        }

        [Fact]
        public void ReflectionAccessNormalizesPatternAndRequiresElements()
        {
            var site = Type("Site");
            var target = Type("Target");
            var access = new BrickReflectionAccess(site, target, null);

            Assert.Equal(string.Empty, access.AccessPattern);
            Assert.Equal(BrickReflectionConfidence.Low, access.Confidence);
            Assert.Equal(BrickEvidenceLevel.RuntimeInferred, access.EvidenceLevel);
            Assert.Throws<ArgumentNullException>(() => new BrickReflectionAccess(null, target, "typeof"));
            Assert.Throws<ArgumentNullException>(() => new BrickReflectionAccess(site, null, "typeof"));
        }

        [Fact]
        public void ReflectionPolicyUsesDefaultConfidenceAndJustification()
        {
            var policy = new BrickReflectionPolicy();

            Assert.Equal(BrickReflectionConfidence.Medium, policy.MinimumConfidence);
            Assert.True(policy.RequireJustification);
            Assert.True(policy.Enabled);
        }

        [Fact]
        public void ReflectionPolicyStoresCustomValues()
        {
            var policy = new BrickReflectionPolicy(BrickReflectionConfidence.High, false, false);

            Assert.Equal(BrickReflectionConfidence.High, policy.MinimumConfidence);
            Assert.False(policy.RequireJustification);
            Assert.False(policy.Enabled);
        }

        [Fact]
        public void EvaluateAccessesAllowsConfidentJustifiedReflection()
        {
            var access = Access(BrickReflectionConfidence.Medium, "Known plugin activation.");

            var violations = BrickReflectionEvaluator.EvaluateAccesses(new[] { access });

            Assert.Empty(violations);
        }

        [Fact]
        public void EvaluateAccessesReportsLowConfidenceReflection()
        {
            var access = Access(BrickReflectionConfidence.Low, "Known plugin activation.");

            var violation = BrickReflectionEvaluator.EvaluateAccesses(new[] { access }).Single();

            Assert.Equal(BrickReflectionEvaluator.MinimumConfidenceRuleId, violation.RuleId);
            Assert.Equal(BrickViolationKind.DependencyRule, violation.Kind);
            Assert.Equal(access.AccessSite, violation.Source);
            Assert.Equal(access.Target, violation.Target);
            Assert.Equal(BrickSeverity.Warning, violation.Severity);
            Assert.Equal(BrickViolationState.Active, violation.State);
            Assert.Equal("Reflection access confidence", violation.RuleName);
            Assert.Equal("Reflection access must meet the configured confidence threshold.", violation.Message);
            Assert.Empty(violation.ResolvedSourceRoles);
            Assert.Empty(violation.ResolvedTargetRoles);
            Assert.Equal(BrickDependencyKindId.From("ReflectionAccess"), violation.DependencyKindId);
            Assert.Equal(BrickScope.Type, violation.Scope);
            Assert.Equal(BrickDependencyLayer.Runtime, violation.DependencyLayer);
            Assert.Equal(BrickEvidenceLevel.RuntimeInferred, violation.EvidenceLevel);
            Assert.Equal("Known plugin activation.", violation.StateReason);
        }

        [Fact]
        public void EvaluateAccessesReportsMissingJustificationWhenRequired()
        {
            var access = Access(BrickReflectionConfidence.High, " ");

            var violation = BrickReflectionEvaluator.EvaluateAccesses(new[] { access }).Single();

            Assert.Equal(BrickReflectionEvaluator.JustificationRuleId, violation.RuleId);
            Assert.Equal("Reflection access must be justified.", violation.Message);
        }

        [Fact]
        public void EvaluateAccessesCanReportConfidenceAndJustificationTogether()
        {
            var access = Access(BrickReflectionConfidence.Low, null);

            var violations = BrickReflectionEvaluator.EvaluateAccesses(new[] { access });

            Assert.Equal(
                new[] { BrickReflectionEvaluator.MinimumConfidenceRuleId, BrickReflectionEvaluator.JustificationRuleId },
                violations.Select(violation => violation.RuleId.Value).ToArray());
        }

        [Fact]
        public void EvaluateAccessesHonorsCustomPolicy()
        {
            var access = Access(BrickReflectionConfidence.Medium, null);
            var policy = new BrickReflectionPolicy(BrickReflectionConfidence.High, false);

            var violation = BrickReflectionEvaluator.EvaluateAccesses(new[] { access }, policy).Single();

            Assert.Equal(BrickReflectionEvaluator.MinimumConfidenceRuleId, violation.RuleId);
        }

        [Fact]
        public void EvaluateAccessesNormalizesNullInputsAndDisabledPolicy()
        {
            Assert.Empty(BrickReflectionEvaluator.EvaluateAccesses(null));
            Assert.Empty(BrickReflectionEvaluator.EvaluateAccesses(
                new[] { Access(BrickReflectionConfidence.Low, null) },
                new BrickReflectionPolicy(enabled: false)));
        }

        private static BrickReflectionAccess Access(BrickReflectionConfidence confidence, string justification) =>
            new BrickReflectionAccess(
                Type("Billing.Infrastructure.ReflectionActivator"),
                Type("Billing.Domain.Order"),
                "Activator.CreateInstance",
                confidence,
                justification);

        private static BrickElement Type(string name) =>
            new BrickElement(BrickElementId.From("type:" + name), BrickElementKind.Type, name, "Billing", Namespace(name), name);

        private static string Namespace(string fullName)
        {
            var lastDot = fullName.LastIndexOf('.');
            return lastDot < 0 ? string.Empty : fullName.Substring(0, lastDot);
        }
    }
}
