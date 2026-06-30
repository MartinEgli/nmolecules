using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Excludes source types whose names contain any configured token.
    /// </summary>
    public sealed class ExcludedSourceNameContainsAttribute : RuleFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludedSourceNameContainsAttribute"/> class.
        /// </summary>
        /// <param name="ruleId">The rule identifier this filter belongs to.</param>
        /// <param name="tokens">Source-name tokens that suppress matching observations.</param>
        public ExcludedSourceNameContainsAttribute(string ruleId, params string[] tokens)
            : base(ruleId, tokens)
        {
        }

        /// <summary>
        /// Converts this attribute metadata into the runtime filter representation.
        /// </summary>
        /// <returns>An <see cref="ExcludedSourceNameContainsRuleFilter"/> with the configured tokens.</returns>
        public override RuleFilter ToFilter() => new ExcludedSourceNameContainsRuleFilter(Tokens);
    }
}
