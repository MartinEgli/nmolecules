using System;

namespace NMolecules.Bricks
{
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
}
