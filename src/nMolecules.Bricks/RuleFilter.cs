using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Represents a single optional rule filter for <see cref="RuleAttribute"/>.
    /// </summary>
    /// <remarks>
    /// CLR attribute metadata cannot store arbitrary custom filter objects directly.
    /// Because of that, analyzer-visible rule filters are expressed through dedicated
    /// <see cref="RuleFilterAttribute"/> types, while <see cref="RuleFilter"/> and
    /// its specializations provide the typed runtime representation.
    /// </remarks>
    public abstract class RuleFilter : IEquatable<RuleFilter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleFilter"/> class.
        /// </summary>
        /// <param name="tokens">The tokens that make up the filter.</param>
        protected RuleFilter(params string[] tokens)
        {
            Tokens = NormalizeTokens(tokens);
            Value = string.Join("|", Tokens);
        }

        /// <summary>
        /// Gets the normalized tokens that make up this filter.
        /// </summary>
        public string[] Tokens { get; }

        /// <summary>
        /// Gets the pipe-separated string representation used by analyzers.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets a value indicating whether this filter contains no tokens.
        /// </summary>
        public bool IsEmpty => Tokens.Length == 0;

        public bool Equals(RuleFilter other)
        {
            return other != null &&
                GetType() == other.GetType() &&
                string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is RuleFilter other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = GetType().GetHashCode();
                hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);
                return hash;
            }
        }

        /// <summary>
        /// Compares two rule filters for ordinal equality.
        /// </summary>
        public static bool operator ==(RuleFilter left, RuleFilter right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Compares two rule filters for ordinal inequality.
        /// </summary>
        public static bool operator !=(RuleFilter left, RuleFilter right)
        {
            return !(left == right);
        }

        internal static string[] NormalizeTokens(IEnumerable<string> tokens)
        {
            if (tokens is null)
            {
                return Array.Empty<string>();
            }

            return tokens
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .Select(token => token.Trim())
                .Where(token => token.Length > 0)
                .ToArray();
        }
    }

    /// <summary>
    /// Excludes source types whose names contain any configured token.
    /// </summary>
    public sealed class ExcludedSourceNameContainsRuleFilter : RuleFilter
    {
        public ExcludedSourceNameContainsRuleFilter(params string[] tokens) : base(tokens)
        {
        }
    }

    /// <summary>
    /// Excludes target types whose names contain any configured token.
    /// </summary>
    public sealed class ExcludedTargetNameContainsRuleFilter : RuleFilter
    {
        public ExcludedTargetNameContainsRuleFilter(params string[] tokens) : base(tokens)
        {
        }
    }

    /// <summary>
    /// Excludes dependency observations whose member names contain any configured token.
    /// </summary>
    public sealed class ExcludedMemberNameContainsRuleFilter : RuleFilter
    {
        public ExcludedMemberNameContainsRuleFilter(params string[] tokens) : base(tokens)
        {
        }
    }

    /// <summary>
    /// Requires source types to contain at least one configured token in their names.
    /// </summary>
    public sealed class RequiredSourceNameContainsRuleFilter : RuleFilter
    {
        public RequiredSourceNameContainsRuleFilter(params string[] tokens) : base(tokens)
        {
        }
    }

    /// <summary>
    /// Requires target types to contain at least one configured token in their names.
    /// </summary>
    public sealed class RequiredTargetNameContainsRuleFilter : RuleFilter
    {
        public RequiredTargetNameContainsRuleFilter(params string[] tokens) : base(tokens)
        {
        }
    }
}
