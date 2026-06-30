using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Requires target types to contain at least one configured token in their names.
    /// </summary>
    public sealed class RequiredTargetNameContainsAttribute : RuleFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredTargetNameContainsAttribute"/> class.
        /// </summary>
        /// <param name="ruleId">The rule identifier this filter belongs to.</param>
        /// <param name="tokens">Target-name tokens that must match for the rule to apply.</param>
        public RequiredTargetNameContainsAttribute(string ruleId, params string[] tokens)
            : base(ruleId, tokens)
        {
        }

        /// <summary>
        /// Converts this attribute metadata into the runtime filter representation.
        /// </summary>
        /// <returns>A <see cref="RequiredTargetNameContainsRuleFilter"/> with the configured tokens.</returns>
        public override RuleFilter ToFilter() => new RequiredTargetNameContainsRuleFilter(Tokens);
    }
}
