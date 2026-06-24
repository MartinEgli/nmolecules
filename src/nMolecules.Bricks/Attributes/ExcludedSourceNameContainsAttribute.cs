using System;

namespace NMolecules.Bricks
{
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
}
