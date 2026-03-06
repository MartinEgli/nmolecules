using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Base type for specialized rule-filter attributes that attach optional filter metadata
    /// to a concrete <see cref="RuleAttribute"/> declaration through its rule identifier.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class,
        AllowMultiple = true)]
    public abstract class RuleFilterAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleFilterAttribute"/> class.
        /// </summary>
        /// <param name="ruleId">The rule identifier this filter belongs to.</param>
        /// <param name="tokens">The configured filter tokens.</param>
        protected RuleFilterAttribute(string ruleId, params string[] tokens)
        {
            Rule = ruleId ?? string.Empty;
            Tokens = RuleFilter.NormalizeTokens(tokens);
        }

        /// <summary>
        /// Gets the rule identifier this filter belongs to.
        /// </summary>
        public string Rule { get; }

        /// <summary>
        /// Gets the typed rule identifier representation of <see cref="Rule"/>.
        /// </summary>
        public RuleId RuleId => RuleId.From(Rule);

        /// <summary>
        /// Gets the normalized filter tokens.
        /// </summary>
        public string[] Tokens { get; }

        /// <summary>
        /// Converts the attribute metadata into its typed runtime filter representation.
        /// </summary>
        public abstract RuleFilter ToFilter();
    }

    /// <summary>
    /// Excludes source types whose names contain any configured token.
    /// </summary>
    public sealed class ExcludedSourceNameContainsAttribute : RuleFilterAttribute
    {
        public ExcludedSourceNameContainsAttribute(string ruleId, params string[] tokens)
            : base(ruleId, tokens)
        {
        }

        public override RuleFilter ToFilter() => new ExcludedSourceNameContainsRuleFilter(Tokens);
    }

    /// <summary>
    /// Excludes target types whose names contain any configured token.
    /// </summary>
    public sealed class ExcludedTargetNameContainsAttribute : RuleFilterAttribute
    {
        public ExcludedTargetNameContainsAttribute(string ruleId, params string[] tokens)
            : base(ruleId, tokens)
        {
        }

        public override RuleFilter ToFilter() => new ExcludedTargetNameContainsRuleFilter(Tokens);
    }

    /// <summary>
    /// Excludes dependency observations whose member names contain any configured token.
    /// </summary>
    public sealed class ExcludedMemberNameContainsAttribute : RuleFilterAttribute
    {
        public ExcludedMemberNameContainsAttribute(string ruleId, params string[] tokens)
            : base(ruleId, tokens)
        {
        }

        public override RuleFilter ToFilter() => new ExcludedMemberNameContainsRuleFilter(Tokens);
    }

    /// <summary>
    /// Requires source types to contain at least one configured token in their names.
    /// </summary>
    public sealed class RequiredSourceNameContainsAttribute : RuleFilterAttribute
    {
        public RequiredSourceNameContainsAttribute(string ruleId, params string[] tokens)
            : base(ruleId, tokens)
        {
        }

        public override RuleFilter ToFilter() => new RequiredSourceNameContainsRuleFilter(Tokens);
    }

    /// <summary>
    /// Requires target types to contain at least one configured token in their names.
    /// </summary>
    public sealed class RequiredTargetNameContainsAttribute : RuleFilterAttribute
    {
        public RequiredTargetNameContainsAttribute(string ruleId, params string[] tokens)
            : base(ruleId, tokens)
        {
        }

        public override RuleFilter ToFilter() => new RequiredTargetNameContainsRuleFilter(Tokens);
    }
}
