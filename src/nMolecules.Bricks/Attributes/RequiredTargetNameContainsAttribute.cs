using System;

namespace NMolecules.Bricks
{
/// <summary>
    /// Requires target types to contain at least one configured token in their names.
    /// </summary>
    public sealed class RequiredTargetNameContainsAttribute : RuleFilterAttribute
    {
        public RequiredTargetNameContainsAttribute(string ruleId, params string[] tokens)
            : base(ruleId, tokens)
        {
        }

        public override RuleFilter ToFilter() => new RequiredTargetNameContainsRuleFilter(Tokens);
    }
}
