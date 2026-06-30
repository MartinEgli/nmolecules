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
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredSourceNameContainsRuleFilter"/> class.
        /// </summary>
        /// <param name="tokens">Source-name tokens that must match for the rule to apply.</param>
        public RequiredSourceNameContainsRuleFilter(params string[] tokens) : base(tokens)
        {
        }
    }
}
