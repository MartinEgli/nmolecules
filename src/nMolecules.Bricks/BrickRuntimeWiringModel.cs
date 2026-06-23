using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    public sealed class BrickDependencyRegistration
    {
        public const string DependencyKind = "DependencyRegistration";

        public BrickDependencyRegistration(
            BrickElement registrationSite,
            BrickElement serviceType,
            BrickElement implementationType,
            string lifetime = null,
            string justification = null,
            BrickEvidenceLevel evidenceLevel = BrickEvidenceLevel.AnalyzerInferred)
        {
            RegistrationSite = registrationSite ?? throw new ArgumentNullException(nameof(registrationSite));
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            Lifetime = lifetime ?? string.Empty;
            Justification = justification;
            EvidenceLevel = evidenceLevel;
        }

        public BrickElement RegistrationSite { get; }
        public BrickElement ServiceType { get; }
        public BrickElement ImplementationType { get; }
        public string Lifetime { get; }
        public string Justification { get; }
        public BrickEvidenceLevel EvidenceLevel { get; }

        public BrickDependency ToDependency() =>
            new BrickDependency(
                RegistrationSite,
                ImplementationType,
                BrickDependencyKindId.From(DependencyKind),
                BrickScope.Type,
                BrickDependencyLayer.Runtime,
                BrickDependencyStrength.Direct,
                EvidenceLevel,
                detail: Lifetime);
    }

    public sealed class BrickRuntimeWiringPolicy
    {
        public BrickRuntimeWiringPolicy(
            IEnumerable<RoleId> allowedRegistrationSiteRoles = null,
            bool requireJustification = false,
            bool enabled = true)
        {
            AllowedRegistrationSiteRoles = (allowedRegistrationSiteRoles ?? Defaults()).Distinct().ToArray();
            RequireJustification = requireJustification;
            Enabled = enabled;
        }

        public IReadOnlyList<RoleId> AllowedRegistrationSiteRoles { get; }
        public bool RequireJustification { get; }
        public bool Enabled { get; }

        private static IEnumerable<RoleId> Defaults()
        {
            yield return RoleId.From("Infrastructure");
            yield return RoleId.From("Platform");
        }
    }

    public static class BrickRuntimeWiringEvaluator
    {
        public static readonly RuleId RegistrationSiteRoleRuleId = RuleId.From("XMoleculesBricks0702");
        public static readonly RuleId RegistrationJustificationRuleId = RuleId.From("XMoleculesBricks0703");

        public static IReadOnlyList<BrickViolation> EvaluateRegistrations(
            IEnumerable<BrickDependencyRegistration> registrations,
            IReadOnlyDictionary<BrickElementId, IEnumerable<RoleId>> rolesByElement,
            BrickRuntimeWiringPolicy policy = null)
        {
            var activePolicy = policy ?? new BrickRuntimeWiringPolicy();
            if (!activePolicy.Enabled)
            {
                return Enumerable.Empty<BrickViolation>().ToArray();
            }

            var violations = new List<BrickViolation>();
            foreach (var registration in registrations ?? Enumerable.Empty<BrickDependencyRegistration>())
            {
                var siteRoles = ResolveRoles(rolesByElement, registration.RegistrationSite).ToArray();
                if (!siteRoles.Any(role => activePolicy.AllowedRegistrationSiteRoles.Contains(role)))
                {
                    violations.Add(Violation(
                        registration,
                        RegistrationSiteRoleRuleId,
                        "Dependency registration must be owned by an allowed runtime-wiring role.",
                        siteRoles));
                }

                if (activePolicy.RequireJustification && string.IsNullOrWhiteSpace(registration.Justification))
                {
                    violations.Add(Violation(
                        registration,
                        RegistrationJustificationRuleId,
                        "Dependency registration must be justified.",
                        siteRoles));
                }
            }

            return violations;
        }

        private static IEnumerable<RoleId> ResolveRoles(
            IReadOnlyDictionary<BrickElementId, IEnumerable<RoleId>> rolesByElement,
            BrickElement element)
        {
            if (rolesByElement != null && rolesByElement.TryGetValue(element.Id, out var roles))
            {
                return roles ?? Enumerable.Empty<RoleId>();
            }

            return Enumerable.Empty<RoleId>();
        }

        private static BrickViolation Violation(
            BrickDependencyRegistration registration,
            RuleId ruleId,
            string message,
            IEnumerable<RoleId> siteRoles) =>
            new BrickViolation(
                BrickViolationKind.DependencyRule,
                registration.RegistrationSite,
                message,
                BrickSeverity.Warning,
                BrickViolationState.Active,
                ruleId,
                "Runtime dependency registration",
                registration.ImplementationType,
                siteRoles,
                null,
                BrickDependencyKindId.From(BrickDependencyRegistration.DependencyKind),
                BrickScope.Type,
                BrickDependencyLayer.Runtime,
                registration.EvidenceLevel,
                registration.Justification);
    }
}
