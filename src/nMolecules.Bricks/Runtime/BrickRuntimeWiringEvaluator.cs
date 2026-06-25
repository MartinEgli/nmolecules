using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Evaluates runtime wiring evaluator rules against Bricks model data and produces deterministic assessment
/// results.
/// </summary>
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
