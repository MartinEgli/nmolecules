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
        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public const string CurrentSchema = "NMolecules.Bricks.RuleProposalQueue/1.0";

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickRuleProposalQueue(DateTimeOffset generatedAt, IEnumerable<BrickRuleProposal> proposals)
            : this(generatedAt, proposals, CurrentSchema)
        {
        }

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickRuleProposalQueue(DateTimeOffset generatedAt, IEnumerable<BrickRuleProposal> proposals, string schema)
        {
            GeneratedAt = generatedAt;
            Schema = schema ?? string.Empty;
            Proposals = (proposals ?? Enumerable.Empty<BrickRuleProposal>())
                .OrderBy(proposal => proposal.ProposalId, StringComparer.Ordinal)
                .ToArray();
        }

        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public string Schema { get; }
        /// <summary>
        /// Gets the timestamp associated with this Bricks model object.
        /// </summary>
        public DateTimeOffset GeneratedAt { get; }
        /// <summary>
        /// Gets the Proposals value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickRuleProposal> Proposals { get; }
        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }
}
