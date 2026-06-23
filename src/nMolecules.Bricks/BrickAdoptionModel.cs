using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    public readonly struct BrickElementSelector : IEquatable<BrickElementSelector>
    {
        public BrickElementSelector(BrickElementKind kind, string pattern, string assemblyName = null)
        {
            Kind = kind;
            Pattern = pattern ?? string.Empty;
            AssemblyName = assemblyName;
        }

        public BrickElementKind Kind { get; }
        public string Pattern { get; }
        public string AssemblyName { get; }

        public bool Matches(BrickElement element)
        {
            if (element is null)
            {
                return false;
            }

            if (Kind != BrickElementKind.Unknown && element.Kind != Kind)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(AssemblyName) && !string.Equals(AssemblyName, element.AssemblyName, StringComparison.Ordinal))
            {
                return false;
            }

            return MatchesPattern(Pattern, element.FullName) ||
                   MatchesPattern(Pattern, element.DisplayName) ||
                   MatchesPattern(Pattern, element.Id.Value);
        }

        public bool Equals(BrickElementSelector other) =>
            Kind == other.Kind &&
            string.Equals(Pattern, other.Pattern, StringComparison.Ordinal) &&
            string.Equals(AssemblyName, other.AssemblyName, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is BrickElementSelector other && Equals(other);
        public override int GetHashCode() => Kind.GetHashCode() ^ StringComparer.Ordinal.GetHashCode(Pattern ?? string.Empty) ^ StringComparer.Ordinal.GetHashCode(AssemblyName ?? string.Empty);
        public static bool operator ==(BrickElementSelector left, BrickElementSelector right) => left.Equals(right);
        public static bool operator !=(BrickElementSelector left, BrickElementSelector right) => !left.Equals(right);

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
    }

    public sealed class BrickBaselineEntry
    {
        public BrickBaselineEntry(
            RuleId ruleId,
            string sourcePattern,
            string targetPattern,
            string justification = null,
            string owner = null,
            DateTimeOffset? expiresAt = null)
        {
            RuleId = ruleId;
            SourcePattern = sourcePattern ?? string.Empty;
            TargetPattern = targetPattern ?? string.Empty;
            Justification = justification;
            Owner = owner;
            ExpiresAt = expiresAt;
        }

        public RuleId RuleId { get; }
        public string SourcePattern { get; }
        public string TargetPattern { get; }
        public string Justification { get; }
        public string Owner { get; }
        public DateTimeOffset? ExpiresAt { get; }

        public bool IsExpired(DateTimeOffset now) => ExpiresAt.HasValue && now > ExpiresAt.Value;
    }

    public sealed class BrickSuppression
    {
        public BrickSuppression(
            RuleId ruleId,
            BrickElementSelector? selector,
            string justification,
            string owner = null,
            DateTimeOffset? expiresAt = null)
        {
            RuleId = ruleId;
            Selector = selector ?? new BrickElementSelector(BrickElementKind.Unknown, string.Empty);
            Justification = justification ?? string.Empty;
            Owner = owner;
            ExpiresAt = expiresAt;
        }

        public RuleId RuleId { get; }
        public BrickElementSelector Selector { get; }
        public string Justification { get; }
        public string Owner { get; }
        public DateTimeOffset? ExpiresAt { get; }

        public bool IsExpired(DateTimeOffset now) => ExpiresAt.HasValue && now > ExpiresAt.Value;
    }

    public sealed class BrickAdoptionDocument
    {
        public const string CurrentSchema = "NMolecules.Bricks.Adoption/1.0";

        public BrickAdoptionDocument(
            DateTimeOffset generatedAt,
            IEnumerable<BrickBaselineEntry> baselines,
            IEnumerable<BrickSuppression> suppressions)
            : this(generatedAt, baselines, suppressions, CurrentSchema)
        {
        }

        public BrickAdoptionDocument(
            DateTimeOffset generatedAt,
            IEnumerable<BrickBaselineEntry> baselines,
            IEnumerable<BrickSuppression> suppressions,
            string schema)
        {
            GeneratedAt = generatedAt;
            Baselines = (baselines ?? Enumerable.Empty<BrickBaselineEntry>())
                .OrderBy(baseline => baseline.RuleId.Value, StringComparer.Ordinal)
                .ThenBy(baseline => baseline.SourcePattern, StringComparer.Ordinal)
                .ThenBy(baseline => baseline.TargetPattern, StringComparer.Ordinal)
                .ToArray();
            Suppressions = (suppressions ?? Enumerable.Empty<BrickSuppression>())
                .OrderBy(suppression => suppression.RuleId.Value, StringComparer.Ordinal)
                .ThenBy(suppression => suppression.Selector.Pattern, StringComparer.Ordinal)
                .ToArray();
            Schema = schema ?? string.Empty;
        }

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickBaselineEntry> Baselines { get; }
        public IReadOnlyList<BrickSuppression> Suppressions { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
        public bool HasEntries => Baselines.Count > 0 || Suppressions.Count > 0;
    }

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
