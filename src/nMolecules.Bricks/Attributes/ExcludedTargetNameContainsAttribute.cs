using System;

namespace NMolecules.Bricks
{
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
}
