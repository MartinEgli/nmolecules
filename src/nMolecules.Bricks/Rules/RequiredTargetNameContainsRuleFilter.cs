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
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredTargetNameContainsRuleFilter"/> class.
        /// </summary>
        /// <param name="tokens">Target-name tokens that must match for the rule to apply.</param>
        public RequiredTargetNameContainsRuleFilter(params string[] tokens) : base(tokens)
        {
        }
    }
}
