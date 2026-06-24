using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public static class BrickViolationStateProjector
    {
        public static IReadOnlyList<BrickViolation> Project(
            IEnumerable<BrickViolation> violations,
            IEnumerable<BrickSuppression> suppressions,
            IEnumerable<BrickBaselineEntry> baselines,
            DateTimeOffset now)
        {
            var suppressionList = (suppressions ?? Enumerable.Empty<BrickSuppression>()).ToArray();
            var baselineList = (baselines ?? Enumerable.Empty<BrickBaselineEntry>()).ToArray();
            return (violations ?? Enumerable.Empty<BrickViolation>())
                .Select(violation => ProjectOne(violation, suppressionList, baselineList, now))
                .ToArray();
        }

        private static BrickViolation ProjectOne(
            BrickViolation violation,
            IEnumerable<BrickSuppression> suppressions,
            IEnumerable<BrickBaselineEntry> baselines,
            DateTimeOffset now)
        {
            var suppression = suppressions.FirstOrDefault(candidate => MatchesSuppression(candidate, violation));
            if (suppression != null)
            {
                return CopyWithState(
                    violation,
                    suppression.IsExpired(now) ? BrickViolationState.ExpiredSuppression : BrickViolationState.Suppressed,
                    suppression.IsExpired(now) ? $"Suppression expired on {suppression.ExpiresAt.Value:yyyy-MM-dd}." : suppression.Justification);
            }

            var baseline = baselines.FirstOrDefault(candidate => MatchesBaseline(candidate, violation));
            if (baseline != null)
            {
                return CopyWithState(
                    violation,
                    baseline.IsExpired(now) ? BrickViolationState.ExpiredBaseline : BrickViolationState.Baselined,
                    baseline.IsExpired(now) ? $"Baseline expired on {baseline.ExpiresAt.Value:yyyy-MM-dd}." : baseline.Justification);
            }

            return violation;
        }

        private static bool MatchesSuppression(BrickSuppression suppression, BrickViolation violation) =>
            violation.RuleId.HasValue &&
            suppression.RuleId == violation.RuleId.Value &&
            suppression.Selector.Matches(violation.Source);

        private static bool MatchesBaseline(BrickBaselineEntry baseline, BrickViolation violation) =>
            violation.RuleId.HasValue &&
            baseline.RuleId == violation.RuleId.Value &&
            MatchesPattern(baseline.SourcePattern, violation.Source) &&
            MatchesPattern(baseline.TargetPattern, violation.Target);

        private static bool MatchesPattern(string pattern, BrickElement element)
        {
            if (element is null)
            {
                return false;
            }

            return MatchesPattern(pattern, element.FullName) ||
                   MatchesPattern(pattern, element.DisplayName) ||
                   MatchesPattern(pattern, element.Id.Value);
        }

        private static bool MatchesPattern(string pattern, string value)
        {
            if (string.IsNullOrWhiteSpace(pattern) || string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (pattern == "*")
            {
                return true;
            }

            if (pattern.EndsWith("*", StringComparison.Ordinal))
            {
                return value.StartsWith(pattern.Substring(0, pattern.Length - 1), StringComparison.Ordinal);
            }

            return string.Equals(pattern, value, StringComparison.Ordinal);
        }

        private static BrickViolation CopyWithState(BrickViolation violation, BrickViolationState state, string stateReason) =>
            new BrickViolation(
                violation.Kind,
                violation.Source,
                violation.Message,
                violation.Severity,
                state,
                violation.RuleId,
                violation.RuleName,
                violation.Target,
                violation.ResolvedSourceRoles,
                violation.ResolvedTargetRoles,
                violation.DependencyKindId,
                violation.Scope,
                violation.DependencyLayer,
                violation.EvidenceLevel,
                stateReason);
    }
}
