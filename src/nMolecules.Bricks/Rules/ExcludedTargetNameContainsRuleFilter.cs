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
        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludedTargetNameContainsRuleFilter"/> class.
        /// </summary>
        /// <param name="tokens">Target-name tokens that suppress matching observations.</param>
        public ExcludedTargetNameContainsRuleFilter(params string[] tokens) : base(tokens)
        {
        }
    }
}
