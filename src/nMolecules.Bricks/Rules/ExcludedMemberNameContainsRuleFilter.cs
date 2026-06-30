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
        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludedMemberNameContainsRuleFilter"/> class.
        /// </summary>
        /// <param name="tokens">Member-name tokens that suppress matching observations.</param>
        public ExcludedMemberNameContainsRuleFilter(params string[] tokens) : base(tokens)
        {
        }
    }
}
