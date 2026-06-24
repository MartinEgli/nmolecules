using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
    /// Requires source types to contain at least one configured token in their names.
    /// </summary>
    public sealed class RequiredSourceNameContainsRuleFilter : RuleFilter
    {
        public RequiredSourceNameContainsRuleFilter(params string[] tokens) : base(tokens)
        {
        }
    }
}
