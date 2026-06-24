using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
    /// Excludes target types whose names contain any configured token.
    /// </summary>
    public sealed class ExcludedTargetNameContainsRuleFilter : RuleFilter
    {
        public ExcludedTargetNameContainsRuleFilter(params string[] tokens) : base(tokens)
        {
        }
    }
}
