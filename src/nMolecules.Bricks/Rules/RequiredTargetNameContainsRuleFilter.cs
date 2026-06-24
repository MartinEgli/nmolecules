using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
    /// Requires target types to contain at least one configured token in their names.
    /// </summary>
    public sealed class RequiredTargetNameContainsRuleFilter : RuleFilter
    {
        public RequiredTargetNameContainsRuleFilter(params string[] tokens) : base(tokens)
        {
        }
    }
}
