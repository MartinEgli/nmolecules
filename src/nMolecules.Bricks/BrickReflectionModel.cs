using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    public sealed class BrickReflectionAccess
    {
        public const string DependencyKind = "ReflectionAccess";

        public BrickReflectionAccess(
            BrickElement accessSite,
            BrickElement target,
            string accessPattern,
            BrickReflectionConfidence confidence = BrickReflectionConfidence.Low,
            string justification = null,
            BrickEvidenceLevel evidenceLevel = BrickEvidenceLevel.RuntimeInferred)
        {
            AccessSite = accessSite ?? throw new ArgumentNullException(nameof(accessSite));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            AccessPattern = accessPattern ?? string.Empty;
            Confidence = confidence;
            Justification = justification;
            EvidenceLevel = evidenceLevel;
        }

        public BrickElement AccessSite { get; }
        public BrickElement Target { get; }
        public string AccessPattern { get; }
        public BrickReflectionConfidence Confidence { get; }
        public string Justification { get; }
        public BrickEvidenceLevel EvidenceLevel { get; }

        public BrickDependency ToDependency() =>
            new BrickDependency(
                AccessSite,
                Target,
                BrickDependencyKindId.From(DependencyKind),
                BrickScope.Type,
                BrickDependencyLayer.Runtime,
                BrickDependencyStrength.Inferred,
                EvidenceLevel,
                detail: AccessPattern);
    }

    public sealed class BrickReflectionPolicy
    {
        public BrickReflectionPolicy(
            BrickReflectionConfidence minimumConfidence = BrickReflectionConfidence.Medium,
            bool requireJustification = true,
            bool enabled = true)
        {
            MinimumConfidence = minimumConfidence;
            RequireJustification = requireJustification;
            Enabled = enabled;
        }

        public BrickReflectionConfidence MinimumConfidence { get; }
        public bool RequireJustification { get; }
        public bool Enabled { get; }
    }

    public static class BrickReflectionEvaluator
    {
        public static readonly RuleId MinimumConfidenceRuleId = RuleId.From("XMoleculesBricks0704");
        public static readonly RuleId JustificationRuleId = RuleId.From("XMoleculesBricks0705");

        public static IReadOnlyList<BrickViolation> EvaluateAccesses(
            IEnumerable<BrickReflectionAccess> accesses,
            BrickReflectionPolicy policy = null)
        {
            var activePolicy = policy ?? new BrickReflectionPolicy();
            if (!activePolicy.Enabled)
            {
                return Enumerable.Empty<BrickViolation>().ToArray();
            }

            var violations = new List<BrickViolation>();
            foreach (var access in accesses ?? Enumerable.Empty<BrickReflectionAccess>())
            {
                if (access.Confidence < activePolicy.MinimumConfidence)
                {
                    violations.Add(Violation(
                        access,
                        MinimumConfidenceRuleId,
                        "Reflection access must meet the configured confidence threshold.",
                        "Reflection access confidence"));
                }

                if (activePolicy.RequireJustification && string.IsNullOrWhiteSpace(access.Justification))
                {
                    violations.Add(Violation(
                        access,
                        JustificationRuleId,
                        "Reflection access must be justified.",
                        "Reflection access justification"));
                }
            }

            return violations;
        }

        private static BrickViolation Violation(
            BrickReflectionAccess access,
            RuleId ruleId,
            string message,
            string ruleName) =>
            new BrickViolation(
                BrickViolationKind.DependencyRule,
                access.AccessSite,
                message,
                BrickSeverity.Warning,
                BrickViolationState.Active,
                ruleId,
                ruleName,
                access.Target,
                null,
                null,
                BrickDependencyKindId.From(BrickReflectionAccess.DependencyKind),
                BrickScope.Type,
                BrickDependencyLayer.Runtime,
                access.EvidenceLevel,
                access.Justification);
    }
}
