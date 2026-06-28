using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Versioned review queue for AI-generated rule proposals.
    /// </summary>
    public sealed class BrickRuleProposalQueue
    {
        public const string CurrentSchema = "NMolecules.Bricks.RuleProposalQueue/1.0";

        public BrickRuleProposalQueue(DateTimeOffset generatedAt, IEnumerable<BrickRuleProposal> proposals)
            : this(generatedAt, proposals, CurrentSchema)
        {
        }

        public BrickRuleProposalQueue(DateTimeOffset generatedAt, IEnumerable<BrickRuleProposal> proposals, string schema)
        {
            GeneratedAt = generatedAt;
            Schema = schema ?? string.Empty;
            Proposals = (proposals ?? Enumerable.Empty<BrickRuleProposal>())
                .OrderBy(proposal => proposal.ProposalId, StringComparer.Ordinal)
                .ToArray();
        }

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickRuleProposal> Proposals { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }
}
