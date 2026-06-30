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
        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludedSourceNameContainsRuleFilter"/> class.
        /// </summary>
        /// <param name="tokens">Source-name tokens that suppress matching observations.</param>
        public ExcludedSourceNameContainsRuleFilter(params string[] tokens) : base(tokens)
        {
        }
    }
}
