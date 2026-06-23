using System;

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

        public bool Equals(BrickElementSelector other) =>
            Kind == other.Kind &&
            string.Equals(Pattern, other.Pattern, StringComparison.Ordinal) &&
            string.Equals(AssemblyName, other.AssemblyName, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is BrickElementSelector other && Equals(other);
        public override int GetHashCode() => Kind.GetHashCode() ^ StringComparer.Ordinal.GetHashCode(Pattern ?? string.Empty) ^ StringComparer.Ordinal.GetHashCode(AssemblyName ?? string.Empty);
        public static bool operator ==(BrickElementSelector left, BrickElementSelector right) => left.Equals(right);
        public static bool operator !=(BrickElementSelector left, BrickElementSelector right) => !left.Equals(right);
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
}
