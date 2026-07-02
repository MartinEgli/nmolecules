using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksRuntimeWiringModelTest
    {
        [Fact]
        public void DependencyRegistrationCreatesRuntimeDependency()
        {
            var site = Type("Billing.Api.CompositionRoot");
            var service = Type("Billing.Application.IClock");
            var implementation = Type("Billing.Infrastructure.SystemClock");
            var registration = new BrickDependencyRegistration(
                site,
                service,
                implementation,
                "Singleton",
                "Infrastructure wiring.",
                BrickEvidenceLevel.ConfigurationDeclared);

            var dependency = registration.ToDependency();

            Assert.Equal(site, registration.RegistrationSite);
            Assert.Equal(service, registration.ServiceType);
            Assert.Equal(implementation, registration.ImplementationType);
            Assert.Equal("Singleton", registration.Lifetime);
            Assert.Equal("Infrastructure wiring.", registration.Justification);
            Assert.Equal(BrickEvidenceLevel.ConfigurationDeclared, registration.EvidenceLevel);
            Assert.Equal(site, dependency.Source);
            Assert.Equal(implementation, dependency.Target);
            Assert.Equal(BrickDependencyKindId.From("DependencyRegistration"), dependency.KindId);
            Assert.Equal(BrickScope.Type, dependency.Scope);
            Assert.Equal(BrickDependencyLayer.Runtime, dependency.Layer);
            Assert.Equal(BrickDependencyStrength.Direct, dependency.Strength);
            Assert.Equal(BrickEvidenceLevel.ConfigurationDeclared, dependency.EvidenceLevel);
            Assert.Equal("Singleton", dependency.Detail);
        }

        [Fact]
        public void DependencyRegistrationNormalizesLifetimeAndRequiresElements()
        {
            var site = Type("Site");
            var service = Type("Service");
            var implementation = Type("Implementation");
            var registration = new BrickDependencyRegistration(site, service, implementation, null);

            Assert.Equal(string.Empty, registration.Lifetime);
            Assert.Throws<ArgumentNullException>(() => new BrickDependencyRegistration(null, service, implementation));
            Assert.Throws<ArgumentNullException>(() => new BrickDependencyRegistration(site, null, implementation));
            Assert.Throws<ArgumentNullException>(() => new BrickDependencyRegistration(site, service, null));
        }

        [Fact]
        public void RuntimeWiringPolicyUsesDefaultRegistrationSiteRoles()
        {
            var policy = new BrickRuntimeWiringPolicy();

            Assert.Equal(new[] { RoleId.From("Infrastructure"), RoleId.From("Platform") }, policy.AllowedRegistrationSiteRoles.ToArray());
            Assert.False(policy.RequireJustification);
            Assert.True(policy.Enabled);
        }

        [Fact]
        public void RuntimeWiringPolicyCopiesAndDistinctsCustomRoles()
        {
            var roles = new[] { RoleId.From("CompositionRoot"), RoleId.From("CompositionRoot"), RoleId.From("Host") };

            var policy = new BrickRuntimeWiringPolicy(roles, true, false);
            roles[0] = RoleId.From("Mutated");

            Assert.Equal(new[] { RoleId.From("CompositionRoot"), RoleId.From("Host") }, policy.AllowedRegistrationSiteRoles.ToArray());
            Assert.True(policy.RequireJustification);
            Assert.False(policy.Enabled);
        }

        [Fact]
        public void EvaluateRegistrationsAllowsInfrastructureRegistrationSite()
        {
            var registration = Registration("Billing.Api.CompositionRoot");
            var roles = Roles(registration.RegistrationSite, RoleId.From("Infrastructure"));

            var violations = BrickRuntimeWiringEvaluator.EvaluateRegistrations(new[] { registration }, roles);

            Assert.Empty(violations);
        }

        [Fact]
        public void EvaluateRegistrationsReportsRegistrationOutsideAllowedRole()
        {
            var registration = Registration("Billing.Domain.Order");
            var roles = Roles(registration.RegistrationSite, RoleId.From("DDD.Entity"));

            var violation = BrickRuntimeWiringEvaluator.EvaluateRegistrations(new[] { registration }, roles).Single();

            Assert.Equal(BrickRuntimeWiringEvaluator.RegistrationSiteRoleRuleId, violation.RuleId);
            Assert.Equal(BrickViolationKind.DependencyRule, violation.Kind);
            Assert.Equal(registration.RegistrationSite, violation.Source);
            Assert.Equal(registration.ImplementationType, violation.Target);
            Assert.Equal(BrickSeverity.Warning, violation.Severity);
            Assert.Equal(BrickViolationState.Active, violation.State);
            Assert.Equal("Runtime dependency registration", violation.RuleName);
            Assert.Equal("Dependency registration must be owned by an allowed runtime-wiring role.", violation.Message);
            Assert.Equal(new[] { RoleId.From("DDD.Entity") }, violation.ResolvedSourceRoles.ToArray());
            Assert.Empty(violation.ResolvedTargetRoles);
            Assert.Equal(BrickDependencyKindId.From("DependencyRegistration"), violation.DependencyKindId);
            Assert.Equal(BrickScope.Type, violation.Scope);
            Assert.Equal(BrickDependencyLayer.Runtime, violation.DependencyLayer);
            Assert.Equal(BrickEvidenceLevel.AnalyzerInferred, violation.EvidenceLevel);
            Assert.Equal("Wiring.", violation.StateReason);
        }

        [Fact]
        public void EvaluateRegistrationsReportsMissingJustificationWhenRequired()
        {
            var registration = new BrickDependencyRegistration(
                Type("Billing.Api.CompositionRoot"),
                Type("Billing.Application.IClock"),
                Type("Billing.Infrastructure.SystemClock"),
                "Scoped",
                " ");
            var roles = Roles(registration.RegistrationSite, RoleId.From("Infrastructure"));
            var policy = new BrickRuntimeWiringPolicy(requireJustification: true);

            var violation = BrickRuntimeWiringEvaluator.EvaluateRegistrations(new[] { registration }, roles, policy).Single();

            Assert.Equal(BrickRuntimeWiringEvaluator.RegistrationJustificationRuleId, violation.RuleId);
            Assert.Equal("Dependency registration must be justified.", violation.Message);
        }

        [Fact]
        public void EvaluateRegistrationsCanReportRoleAndJustificationTogether()
        {
            var registration = new BrickDependencyRegistration(
                Type("Billing.Domain.Order"),
                Type("Billing.Application.IClock"),
                Type("Billing.Infrastructure.SystemClock"));
            var policy = new BrickRuntimeWiringPolicy(requireJustification: true);

            var violations = BrickRuntimeWiringEvaluator.EvaluateRegistrations(new[] { registration }, null, policy);

            Assert.Equal(
                new[] { BrickRuntimeWiringEvaluator.RegistrationSiteRoleRuleId, BrickRuntimeWiringEvaluator.RegistrationJustificationRuleId },
                violations.Select(violation => violation.RuleId.Value).ToArray());
        }

        [Fact]
        public void EvaluateRegistrationsHonorsCustomPolicyAndNullRoleList()
        {
            var registration = Registration("Billing.Tools.Wiring");
            var roles = new Dictionary<BrickElementId, IEnumerable<RoleId>>
            {
                [registration.RegistrationSite.Id] = null
            };
            var policy = new BrickRuntimeWiringPolicy(new[] { RoleId.From("CompositionRoot") }, false);

            var violation = BrickRuntimeWiringEvaluator.EvaluateRegistrations(new[] { registration }, roles, policy).Single();

            Assert.Equal(BrickRuntimeWiringEvaluator.RegistrationSiteRoleRuleId, violation.RuleId);
            Assert.Empty(violation.ResolvedSourceRoles);
        }

        [Fact]
        public void EvaluateRegistrationsNormalizesNullInputsAndDisabledPolicy()
        {
            Assert.Empty(BrickRuntimeWiringEvaluator.EvaluateRegistrations(null, null));
            Assert.Empty(BrickRuntimeWiringEvaluator.EvaluateRegistrations(
                new[] { Registration("Billing.Domain.Order") },
                null,
                new BrickRuntimeWiringPolicy(enabled: false)));
        }

        private static BrickDependencyRegistration Registration(string siteName) =>
            new BrickDependencyRegistration(
                Type(siteName),
                Type("Billing.Application.IClock"),
                Type("Billing.Infrastructure.SystemClock"),
                "Scoped",
                "Wiring.");

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
