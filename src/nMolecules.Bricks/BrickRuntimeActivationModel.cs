using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    public sealed class BrickRuntimeActivation
    {
        public const string DependencyKind = "RuntimeActivation";

        public BrickRuntimeActivation(
            BrickElement activationSite,
            BrickElement activatedType,
            string activationPattern,
            string justification = null,
            BrickEvidenceLevel evidenceLevel = BrickEvidenceLevel.RuntimeInferred)
        {
            ActivationSite = activationSite ?? throw new ArgumentNullException(nameof(activationSite));
            ActivatedType = activatedType ?? throw new ArgumentNullException(nameof(activatedType));
            ActivationPattern = activationPattern ?? string.Empty;
            Justification = justification;
            EvidenceLevel = evidenceLevel;
        }

        public BrickElement ActivationSite { get; }
        public BrickElement ActivatedType { get; }
        public string ActivationPattern { get; }
        public string Justification { get; }
        public BrickEvidenceLevel EvidenceLevel { get; }

        public BrickDependency ToDependency() =>
            new BrickDependency(
                ActivationSite,
                ActivatedType,
                BrickDependencyKindId.From(DependencyKind),
                BrickScope.Type,
                BrickDependencyLayer.Runtime,
                BrickDependencyStrength.Inferred,
                EvidenceLevel,
                detail: ActivationPattern);
    }

    public sealed class BrickRuntimeActivationPolicy
    {
        public BrickRuntimeActivationPolicy(
            IEnumerable<RoleId> allowedActivationSiteRoles = null,
            bool requireJustification = true,
            bool enabled = true)
        {
            AllowedActivationSiteRoles = (allowedActivationSiteRoles ?? Defaults()).Distinct().ToArray();
            RequireJustification = requireJustification;
            Enabled = enabled;
        }

        public IReadOnlyList<RoleId> AllowedActivationSiteRoles { get; }
        public bool RequireJustification { get; }
        public bool Enabled { get; }

        private static IEnumerable<RoleId> Defaults()
        {
            yield return RoleId.From("Infrastructure");
            yield return RoleId.From("Platform");
        }
    }

    public static class BrickRuntimeActivationEvaluator
    {
        public static readonly RuleId ActivationSiteRoleRuleId = RuleId.From("XMoleculesBricks0706");
        public static readonly RuleId ActivationJustificationRuleId = RuleId.From("XMoleculesBricks0707");

        public static IReadOnlyList<BrickViolation> EvaluateActivations(
            IEnumerable<BrickRuntimeActivation> activations,
            IReadOnlyDictionary<BrickElementId, IEnumerable<RoleId>> rolesByElement,
            BrickRuntimeActivationPolicy policy = null)
        {
            var activePolicy = policy ?? new BrickRuntimeActivationPolicy();
            if (!activePolicy.Enabled)
            {
                return Enumerable.Empty<BrickViolation>().ToArray();
            }

            var violations = new List<BrickViolation>();
            foreach (var activation in activations ?? Enumerable.Empty<BrickRuntimeActivation>())
            {
                var siteRoles = ResolveRoles(rolesByElement, activation.ActivationSite).ToArray();
                if (!siteRoles.Any(role => activePolicy.AllowedActivationSiteRoles.Contains(role)))
                {
                    violations.Add(Violation(
                        activation,
                        ActivationSiteRoleRuleId,
                        "Runtime activation must be owned by an allowed activator role.",
                        siteRoles));
                }

                if (activePolicy.RequireJustification && string.IsNullOrWhiteSpace(activation.Justification))
                {
                    violations.Add(Violation(
                        activation,
                        ActivationJustificationRuleId,
                        "Runtime activation must be justified.",
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
            BrickRuntimeActivation activation,
            RuleId ruleId,
            string message,
            IEnumerable<RoleId> siteRoles) =>
            new BrickViolation(
                BrickViolationKind.DependencyRule,
                activation.ActivationSite,
                message,
                BrickSeverity.Warning,
                BrickViolationState.Active,
                ruleId,
                "Runtime activation",
                activation.ActivatedType,
                siteRoles,
                null,
                BrickDependencyKindId.From(BrickRuntimeActivation.DependencyKind),
                BrickScope.Type,
                BrickDependencyLayer.Runtime,
                activation.EvidenceLevel,
                activation.Justification);
    }
}
