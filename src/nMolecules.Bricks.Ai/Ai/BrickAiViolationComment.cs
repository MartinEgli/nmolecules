using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// AI-ready explanation for one deterministic <see cref="BrickViolation"/>.
    /// </summary>
    /// <remarks>
    /// This type is advisory only. It does not create, suppress, baseline, or enforce violations.
    /// The referenced <see cref="BrickViolation"/> remains the source of truth.
    /// </remarks>
    public sealed class BrickAiViolationComment
    {
        /// <summary>
        /// Creates an AI-ready violation comment from deterministic violation evidence and remediation guidance.
        /// </summary>
        public BrickAiViolationComment(
            BrickViolation violation,
            string problemSummary,
            string architecturalReason,
            IEnumerable<BrickRemediationOption> options,
            BrickRemediationOption recommendedOption,
            IEnumerable<string> aiRepairHints,
            string suppressionGuidance)
        {
            Violation = violation ?? throw new ArgumentNullException(nameof(violation));
            ProblemSummary = problemSummary ?? string.Empty;
            ArchitecturalReason = architecturalReason ?? string.Empty;
            Options = (options ?? Enumerable.Empty<BrickRemediationOption>()).ToArray();
            RecommendedOption = ResolveRecommendedOption(Options, recommendedOption);
            AiRepairHints = (aiRepairHints ?? Enumerable.Empty<string>())
                .Where(hint => hint != null)
                .ToArray();
            SuppressionGuidance = suppressionGuidance ?? string.Empty;
        }

        /// <summary>Rule identifier copied from the deterministic violation when available.</summary>
        public RuleId? RuleId => Violation.RuleId;
        /// <summary>The deterministic Bricks violation this AI explanation is based on.</summary>
        public BrickViolation Violation { get; }
        /// <summary>Rule name copied from the deterministic violation.</summary>
        public string RuleName => Violation.RuleName ?? string.Empty;
        /// <summary>The deterministic decision represented by the violation.</summary>
        public BrickDecision Decision => Violation.Kind == BrickViolationKind.RequiredDependency
            ? BrickDecision.Require
            : BrickDecision.Deny;
        /// <summary>Severity copied from the deterministic violation.</summary>
        public BrickSeverity Severity => Violation.Severity;
        /// <summary>Evidence level copied from the deterministic violation.</summary>
        public BrickEvidenceLevel EvidenceLevel => Violation.EvidenceLevel;
        /// <summary>Short problem statement suitable for human comments and AI coding agents.</summary>
        public string ProblemSummary { get; }
        /// <summary>Architectural impact explanation for why the violation matters.</summary>
        public string ArchitecturalReason { get; }
        /// <summary>Available remediation options for the violation.</summary>
        public IReadOnlyList<BrickRemediationOption> Options { get; }
        /// <summary>The preferred remediation option, if one was selected from <see cref="Options"/>.</summary>
        public BrickRemediationOption RecommendedOption { get; }
        /// <summary>Concrete repair hints intended for AI coding agents.</summary>
        public IReadOnlyList<string> AiRepairHints { get; }
        /// <summary>Guidance for when suppression is legitimate and when it is not.</summary>
        public string SuppressionGuidance { get; }
        /// <summary>Indicates whether the comment is backed by non-unknown deterministic evidence.</summary>
        public bool IsTraceableToDeterministicEvidence =>
            Violation.EvidenceLevel != BrickEvidenceLevel.Unknown;

        private static BrickRemediationOption ResolveRecommendedOption(
            IReadOnlyList<BrickRemediationOption> options,
            BrickRemediationOption recommendedOption)
        {
            if (recommendedOption == null)
            {
                return null;
            }

            var match = options.FirstOrDefault(option => string.Equals(option.Id, recommendedOption.Id, StringComparison.Ordinal));
            if (match == null)
            {
                throw new ArgumentException("Recommended remediation option must be part of the options.", nameof(recommendedOption));
            }

            return match;
        }
    }
}
