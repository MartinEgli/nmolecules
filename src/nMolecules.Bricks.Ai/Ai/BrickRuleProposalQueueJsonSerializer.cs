using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Serializes and deserializes AI rule proposal review queues.
    /// </summary>
    public static class BrickRuleProposalQueueJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes the Bricks document to its external JSON representation.
        /// </summary>
        public static string Serialize(BrickRuleProposalQueue queue)
        {
            if (queue == null)
            {
                throw new ArgumentNullException(nameof(queue));
            }

            return JsonSerializer.Serialize(ToDto(queue), Options);
        }

        /// <summary>
        /// Deserializes the Bricks document from its external JSON representation.
        /// </summary>
        public static BrickRuleProposalQueue Deserialize(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            var dto = JsonSerializer.Deserialize<QueueDto>(json, Options);
            if (dto == null)
            {
                throw new ArgumentException("Rule proposal queue JSON could not be deserialized.", nameof(json));
            }

            return new BrickRuleProposalQueue(
                dto.GeneratedAt,
                (dto.Proposals ?? Array.Empty<ProposalDto>()).Select(ToProposal),
                dto.Schema);
        }

        private static QueueDto ToDto(BrickRuleProposalQueue queue) =>
            new QueueDto
            {
                Schema = queue.Schema,
                GeneratedAt = queue.GeneratedAt,
                Proposals = queue.Proposals.Select(ToDto).ToArray()
            };

        private static ProposalDto ToDto(BrickRuleProposal proposal) =>
            new ProposalDto
            {
                ProposalId = proposal.ProposalId,
                Title = proposal.Title,
                Rationale = proposal.Rationale,
                SourceRoles = proposal.SourceRoles.Pattern,
                TargetRoles = proposal.TargetRoles.Pattern,
                DependencyKind = proposal.DependencyKindId.Value,
                SuggestedDecision = proposal.SuggestedDecision.ToString(),
                SuggestedSeverity = proposal.SuggestedSeverity.ToString(),
                LifecycleState = proposal.LifecycleState.ToString(),
                Evidence = ToDto(proposal.Evidence)
            };

        private static EvidenceDto ToDto(BrickRuleProposalEvidence evidence) =>
            new EvidenceDto
            {
                ObservedStructure = evidence.ObservedStructure,
                PositiveExamples = evidence.PositiveExamples.ToArray(),
                NegativeExamples = evidence.NegativeExamples.ToArray(),
                FalsePositiveRisks = evidence.FalsePositiveRisks.ToArray(),
                AffectedScopes = evidence.AffectedScopes.ToArray(),
                MigrationImpact = evidence.MigrationImpact
            };

        private static BrickRuleProposal ToProposal(ProposalDto dto) =>
            new BrickRuleProposal(
                dto.ProposalId,
                dto.Title,
                dto.Rationale,
                BrickRoleSelector.From(dto.SourceRoles),
                BrickRoleSelector.From(dto.TargetRoles),
                BrickDependencyKindId.From(dto.DependencyKind),
                Parse(dto.SuggestedDecision, BrickDecision.Deny),
                Parse(dto.SuggestedSeverity, BrickSeverity.Warning),
                ToEvidence(dto.Evidence),
                ParseLifecycle(dto.LifecycleState));

        private static BrickRuleProposalEvidence ToEvidence(EvidenceDto dto) =>
            dto == null
                ? new BrickRuleProposalEvidence(null, null, null, null, null, null)
                : new BrickRuleProposalEvidence(
                    dto.ObservedStructure,
                    dto.PositiveExamples,
                    dto.NegativeExamples,
                    dto.FalsePositiveRisks,
                    dto.AffectedScopes,
                    dto.MigrationImpact);

        private static TEnum Parse<TEnum>(string value, TEnum defaultValue)
            where TEnum : struct
        {
            TEnum parsed;
            return Enum.TryParse(value, ignoreCase: false, result: out parsed) ? parsed : defaultValue;
        }

        private static BrickRuleLifecycleState ParseLifecycle(string value)
        {
            var parsed = Parse(value, BrickRuleLifecycleState.Candidate);
            return parsed == BrickRuleLifecycleState.Enforced ? BrickRuleLifecycleState.Candidate : parsed;
        }

        private sealed class QueueDto
        {
            public string Schema { get; set; }
            public DateTimeOffset GeneratedAt { get; set; }
            public ProposalDto[] Proposals { get; set; }
        }

        private sealed class ProposalDto
        {
            public string ProposalId { get; set; }
            public string Title { get; set; }
            public string Rationale { get; set; }
            public string SourceRoles { get; set; }
            public string TargetRoles { get; set; }
            public string DependencyKind { get; set; }
            public string SuggestedDecision { get; set; }
            public string SuggestedSeverity { get; set; }
            public string LifecycleState { get; set; }
            public EvidenceDto Evidence { get; set; }
        }

        private sealed class EvidenceDto
        {
            public string ObservedStructure { get; set; }
            public string[] PositiveExamples { get; set; }
            public string[] NegativeExamples { get; set; }
            public string[] FalsePositiveRisks { get; set; }
            public string[] AffectedScopes { get; set; }
            public string MigrationImpact { get; set; }
        }
    }
}
