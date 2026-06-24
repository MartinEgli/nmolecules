using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
    /// Excludes source types whose names contain any configured token.
    /// </summary>
    public sealed class ExcludedSourceNameContainsRuleFilter : RuleFilter
    {
        public ExcludedSourceNameContainsRuleFilter(params string[] tokens) : base(tokens)
        {
        }
    }
}
