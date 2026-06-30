using System;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Excludes dependency observations whose member names contain any configured token.
    /// </summary>
    public sealed class ExcludedMemberNameContainsAttribute : RuleFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludedMemberNameContainsAttribute"/> class.
        /// </summary>
        /// <param name="ruleId">The rule identifier this filter belongs to.</param>
        /// <param name="tokens">Member-name tokens that suppress matching observations.</param>
        public ExcludedMemberNameContainsAttribute(string ruleId, params string[] tokens)
            : base(ruleId, tokens)
        {
        }

        /// <summary>
        /// Converts this attribute metadata into the runtime filter representation.
        /// </summary>
        /// <returns>An <see cref="ExcludedMemberNameContainsRuleFilter"/> with the configured tokens.</returns>
        public override RuleFilter ToFilter() => new ExcludedMemberNameContainsRuleFilter(Tokens);
    }
}
