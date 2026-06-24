using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Evidence package that must accompany an AI-generated structural rule proposal.
    /// </summary>
    public sealed class BrickRuleProposalEvidence
    {
        /// <summary>
        /// Creates proposal evidence from observed structure, examples, risks, affected scopes, and migration impact.
        /// </summary>
        public BrickRuleProposalEvidence(
            string observedStructure,
            IEnumerable<string> positiveExamples,
            IEnumerable<string> negativeExamples,
            IEnumerable<string> falsePositiveRisks,
            IEnumerable<string> affectedScopes,
            string migrationImpact)
        {
            ObservedStructure = observedStructure ?? string.Empty;
            PositiveExamples = CopyStrings(positiveExamples);
            NegativeExamples = CopyStrings(negativeExamples);
            FalsePositiveRisks = CopyStrings(falsePositiveRisks);
            AffectedScopes = CopyStrings(affectedScopes);
            MigrationImpact = migrationImpact ?? string.Empty;
        }

        /// <summary>Description of the structural pattern the AI observed.</summary>
        public string ObservedStructure { get; }
        /// <summary>Examples that support the proposed rule.</summary>
        public IReadOnlyList<string> PositiveExamples { get; }
        /// <summary>Counterexamples or legitimate cases that should not be flagged.</summary>
        public IReadOnlyList<string> NegativeExamples { get; }
        /// <summary>Known situations that may produce false positives.</summary>
        public IReadOnlyList<string> FalsePositiveRisks { get; }
        /// <summary>Projects, assemblies, namespaces, or roles affected by the proposal.</summary>
        public IReadOnlyList<string> AffectedScopes { get; }
        /// <summary>Expected migration impact if the rule is adopted.</summary>
        public string MigrationImpact { get; }

        /// <summary>Indicates whether the evidence is complete enough for human review.</summary>
        public bool HasRequiredEvidence =>
            !string.IsNullOrWhiteSpace(ObservedStructure) &&
            PositiveExamples.Count > 0 &&
            NegativeExamples.Count > 0 &&
            FalsePositiveRisks.Count > 0 &&
            AffectedScopes.Count > 0 &&
            !string.IsNullOrWhiteSpace(MigrationImpact);

        private static IReadOnlyList<string> CopyStrings(IEnumerable<string> values) =>
            (values ?? Enumerable.Empty<string>())
                .Where(value => value != null)
                .ToArray();
    }
}
