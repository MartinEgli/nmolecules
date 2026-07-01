using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Evaluates reflection evaluator rules against Bricks model data and produces deterministic assessment
/// results.
/// </summary>
public static class BrickReflectionEvaluator
    {
        /// <summary>
        /// Gets the diagnostic rule id used when reporting this Bricks validation condition.
        /// </summary>
        public static readonly RuleId MinimumConfidenceRuleId = RuleId.From("XMoleculesBricks0704");
        /// <summary>
        /// Gets the diagnostic rule id used when reporting this Bricks validation condition.
        /// </summary>
        public static readonly RuleId JustificationRuleId = RuleId.From("XMoleculesBricks0705");

        /// <summary>
        /// Evaluates the supplied Bricks data and returns rule results for developer feedback.
        /// </summary>
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
