using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Creates AI-ready advisory comments from deterministic Bricks violations.
    /// </summary>
    /// <remarks>
    /// This factory is the handoff point for analyzer, CI and PR adapters. It never
    /// changes policy, suppressions, baselines, severity or build enforcement.
    /// </remarks>
    public static class BrickAiCommentFactory
    {
        /// <summary>
        /// Creates a current-schema AI comment document when the trust boundary allows explanations.
        /// </summary>
        public static BrickAiCommentDocument CreateDocument(
            IEnumerable<BrickViolation> violations,
            DateTimeOffset generatedAt,
            BrickAiTrustBoundary trustBoundary)
        {
            var boundary = trustBoundary ?? BrickAiTrustBoundary.Default;
            if (boundary.Mode == BrickAiMode.Off)
            {
                return new BrickAiCommentDocument(generatedAt, Array.Empty<BrickAiViolationComment>());
            }

            var comments = (violations ?? Enumerable.Empty<BrickViolation>())
                .Where(violation => violation != null)
                .Select(CreateComment)
                .ToArray();

            return new BrickAiCommentDocument(generatedAt, comments);
        }

        private static BrickAiViolationComment CreateComment(BrickViolation violation)
        {
            var preferred = PreferredOption(violation);
            var suppression = new BrickRemediationOption(
                "reviewed-suppression",
                BrickRemediationKind.AddSuppression,
                "Add an explicit, reviewed suppression with owner, reason and expiry.",
                "Use only when the architecture exception is intentional and time-boxed.",
                BrickRemediationRisk.High,
                isPreferred: false);

            return new BrickAiViolationComment(
                violation,
                Problem(violation),
                Reason(violation),
                new[] { preferred, suppression },
                preferred,
                Hints(violation),
                "Suppress only with owner, justification, expiry and deterministic review evidence.");
        }

        private static BrickRemediationOption PreferredOption(BrickViolation violation)
        {
            switch (violation.Kind)
            {
                case BrickViolationKind.RequiredDependency:
                    return new BrickRemediationOption(
                        "introduce-required-contract",
                        BrickRemediationKind.IntroduceContract,
                        "Introduce or reference the required contract dependency.",
                        "Use when a rule requires collaboration with a target role that is currently missing.",
                        BrickRemediationRisk.Low,
                        isPreferred: true);
                case BrickViolationKind.RoleCombination:
                case BrickViolationKind.RoleResolution:
                    return new BrickRemediationOption(
                        "split-or-reclassify-role",
                        BrickRemediationKind.SplitRole,
                        "Split the element or reclassify the conflicting role assignment.",
                        "Use when one element carries roles that should not be active together.",
                        BrickRemediationRisk.Medium,
                        isPreferred: true);
                case BrickViolationKind.MemberCardinality:
                    return new BrickRemediationOption(
                        "rename-or-add-required-member",
                        BrickRemediationKind.RenameOrReclassifyElement,
                        "Adjust the member set so it satisfies the declared member contract.",
                        "Use when a custom role contract requires a specific marker or member shape.",
                        BrickRemediationRisk.Low,
                        isPreferred: true);
                default:
                    return new BrickRemediationOption(
                        "adjust-architecture-boundary",
                        BrickRemediationKind.ChangeDependencyDirection,
                        "Move the dependency behind the intended boundary or reverse the dependency direction.",
                        "Use when code crosses a deterministic Bricks architecture rule.",
                        BrickRemediationRisk.Medium,
                        isPreferred: true);
            }
        }

        private static string Problem(BrickViolation violation)
        {
            if (!string.IsNullOrWhiteSpace(violation.Message))
            {
                return violation.Message;
            }

            return $"Bricks reported a {violation.Kind} violation on '{violation.Source.DisplayName}'.";
        }

        private static string Reason(BrickViolation violation)
        {
            var ruleName = string.IsNullOrWhiteSpace(violation.RuleName)
                ? "the deterministic Bricks policy"
                : $"rule '{violation.RuleName}'";

            return $"This comment explains {ruleName}. The violation remains the source of truth.";
        }

        private static IReadOnlyList<string> Hints(BrickViolation violation)
        {
            var target = violation.Target == null ? "the expected target boundary" : violation.Target.DisplayName;
            return new[]
            {
                $"Inspect '{violation.Source.DisplayName}' before editing policy.",
                $"Prefer a deterministic code or policy change over suppressing the finding.",
                $"Keep the relationship to '{target}' reviewable."
            };
        }
    }
}
