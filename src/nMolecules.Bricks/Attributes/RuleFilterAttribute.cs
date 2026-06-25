using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Attribute used to declare rule filter metadata for attribute-based role, rule, dependency, and contract
    /// configuration.
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
}
