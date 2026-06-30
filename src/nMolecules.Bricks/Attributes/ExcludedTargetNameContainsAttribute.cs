using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Excludes target types whose names contain any configured token.
    /// </summary>
    public sealed class ExcludedTargetNameContainsAttribute : RuleFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludedTargetNameContainsAttribute"/> class.
        /// </summary>
        /// <param name="ruleId">The rule identifier this filter belongs to.</param>
        /// <param name="tokens">Target-name tokens that suppress matching observations.</param>
        public ExcludedTargetNameContainsAttribute(string ruleId, params string[] tokens)
            : base(ruleId, tokens)
        {
        }

        /// <summary>
        /// Converts this attribute metadata into the runtime filter representation.
        /// </summary>
        /// <returns>An <see cref="ExcludedTargetNameContainsRuleFilter"/> with the configured tokens.</returns>
        public override RuleFilter ToFilter() => new ExcludedTargetNameContainsRuleFilter(Tokens);
    }
}
