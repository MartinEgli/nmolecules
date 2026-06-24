using System;

namespace NMolecules.Bricks
{
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
}
