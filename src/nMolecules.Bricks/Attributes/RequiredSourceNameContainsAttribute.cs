using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Requires source types to contain at least one configured token in their names.
    /// </summary>
    public sealed class RequiredSourceNameContainsAttribute : RuleFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredSourceNameContainsAttribute"/> class.
        /// </summary>
        /// <param name="ruleId">The rule identifier this filter belongs to.</param>
        /// <param name="tokens">Source-name tokens that must match for the rule to apply.</param>
        public RequiredSourceNameContainsAttribute(string ruleId, params string[] tokens)
            : base(ruleId, tokens)
        {
        }

        /// <summary>
        /// Converts this attribute metadata into the runtime filter representation.
        /// </summary>
        /// <returns>A <see cref="RequiredSourceNameContainsRuleFilter"/> with the configured tokens.</returns>
        public override RuleFilter ToFilter() => new RequiredSourceNameContainsRuleFilter(Tokens);
    }
}
