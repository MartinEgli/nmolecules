using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
    /// Excludes dependency observations whose member names contain any configured token.
    /// </summary>
    public sealed class ExcludedMemberNameContainsRuleFilter : RuleFilter
    {
        public ExcludedMemberNameContainsRuleFilter(params string[] tokens) : base(tokens)
        {
        }
    }
}
