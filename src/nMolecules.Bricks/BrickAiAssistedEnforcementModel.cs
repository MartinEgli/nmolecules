using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
    public enum BrickRemediationKind
    {
        IntroduceContract = 0,
        MoveElement = 1,
        SplitRole = 2,
        AddPortAndAdapter = 3,
        ChangeDependencyDirection = 4,
        MoveToCompositionRoot = 5,
        AdjustPolicy = 6,
        AddSuppression = 7,
        AddBaselineEntry = 8,
        RenameOrReclassifyElement = 9
    }

    public enum BrickRemediationRisk
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    public enum BrickRuleLifecycleState
    {
        Candidate = 0,
        Draft = 1,
        Observing = 2,
        Warning = 3,
        Enforced = 4,
        Rejected = 5,
        Deprecated = 6
    }

    public enum BrickAiMode
    {
        Off = 0,
        Explain = 1,
        SuggestRules = 2
    }

    public enum BrickAiCommentFormat
    {
        Markdown = 0,
        Json = 1,
        Both = 2
    }

    public sealed class BrickRemediationOption
    {
        public BrickRemediationOption(
            string id,
            BrickRemediationKind kind,
            string description,
            string whenToUse,
            BrickRemediationRisk risk,
            bool isPreferred)
        {
            Id = id ?? string.Empty;
            Kind = kind;
            Description = description ?? string.Empty;
            WhenToUse = whenToUse ?? string.Empty;
            Risk = risk;
            IsPreferred = isPreferred;
        }

        public string Id { get; }
        public BrickRemediationKind Kind { get; }
        public string Description { get; }
        public string WhenToUse { get; }
        public BrickRemediationRisk Risk { get; }
        public bool IsPreferred { get; }
    }

    public sealed class BrickAiViolationComment
    {
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

        public RuleId? RuleId => Violation.RuleId;
        public BrickViolation Violation { get; }
        public string RuleName => Violation.RuleName ?? string.Empty;
        public BrickDecision Decision => Violation.Kind == BrickViolationKind.RequiredDependency
            ? BrickDecision.Require
            : BrickDecision.Deny;
        public BrickSeverity Severity => Violation.Severity;
        public BrickEvidenceLevel EvidenceLevel => Violation.EvidenceLevel;
        public string ProblemSummary { get; }
        public string ArchitecturalReason { get; }
        public IReadOnlyList<BrickRemediationOption> Options { get; }
        public BrickRemediationOption RecommendedOption { get; }
        public IReadOnlyList<string> AiRepairHints { get; }
        public string SuppressionGuidance { get; }
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

    public sealed class BrickAiCommentDocument
    {
        public const string CurrentSchema = "NMolecules.Bricks.AIComment/1.0";

        public BrickAiCommentDocument(DateTimeOffset generatedAt, IEnumerable<BrickAiViolationComment> comments)
            : this(generatedAt, comments, CurrentSchema)
        {
        }

        public BrickAiCommentDocument(DateTimeOffset generatedAt, IEnumerable<BrickAiViolationComment> comments, string schema)
        {
            GeneratedAt = generatedAt;
            Schema = schema ?? string.Empty;
            Comments = (comments ?? Enumerable.Empty<BrickAiViolationComment>())
                .OrderBy(comment => comment.RuleId?.Value ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(comment => comment.Violation.Source.Id.Value, StringComparer.Ordinal)
                .ToArray();
        }

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickAiViolationComment> Comments { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }

    public sealed class BrickRuleProposalEvidence
    {
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

        public string ObservedStructure { get; }
        public IReadOnlyList<string> PositiveExamples { get; }
        public IReadOnlyList<string> NegativeExamples { get; }
        public IReadOnlyList<string> FalsePositiveRisks { get; }
        public IReadOnlyList<string> AffectedScopes { get; }
        public string MigrationImpact { get; }

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

    public sealed class BrickRuleProposal
    {
        public BrickRuleProposal(
            string proposalId,
            string title,
            string rationale,
            BrickRoleSelector sourceRoles,
            BrickRoleSelector targetRoles,
            BrickDependencyKindId dependencyKindId,
            BrickDecision suggestedDecision,
            BrickSeverity suggestedSeverity,
            BrickRuleProposalEvidence evidence,
            BrickRuleLifecycleState lifecycleState)
        {
            if (lifecycleState == BrickRuleLifecycleState.Enforced)
            {
                throw new ArgumentOutOfRangeException(nameof(lifecycleState), "AI-generated rule proposals cannot start enforced.");
            }

            ProposalId = proposalId ?? string.Empty;
            Title = title ?? string.Empty;
            Rationale = rationale ?? string.Empty;
            SourceRoles = sourceRoles;
            TargetRoles = targetRoles;
            DependencyKindId = dependencyKindId;
            SuggestedDecision = suggestedDecision;
            SuggestedSeverity = suggestedSeverity;
            Evidence = evidence ?? new BrickRuleProposalEvidence(null, null, null, null, null, null);
            LifecycleState = lifecycleState;
        }

        public string ProposalId { get; }
        public string Title { get; }
        public string Rationale { get; }
        public BrickRoleSelector SourceRoles { get; }
        public BrickRoleSelector TargetRoles { get; }
        public BrickDependencyKindId DependencyKindId { get; }
        public BrickDecision SuggestedDecision { get; }
        public BrickSeverity SuggestedSeverity { get; }
        public BrickRuleProposalEvidence Evidence { get; }
        public BrickRuleLifecycleState LifecycleState { get; }
        public bool IsAdvisory => LifecycleState != BrickRuleLifecycleState.Enforced;
        public bool CanBreakBuild => LifecycleState == BrickRuleLifecycleState.Enforced;
        public bool HasRequiredEvidence => Evidence.HasRequiredEvidence;
    }

    public sealed class BrickAiTrustBoundary
    {
        public BrickAiTrustBoundary(
            BrickAiMode mode,
            BrickAiCommentFormat commentFormat,
            bool allowRuleProposal,
            bool allowAutoEnforcement,
            bool allowSilentPolicyMutation)
        {
            Mode = mode;
            CommentFormat = commentFormat;
            AllowRuleProposal = allowRuleProposal;
            AllowAutoEnforcement = allowAutoEnforcement && allowSilentPolicyMutation;
            AllowsSilentPolicyMutation = allowSilentPolicyMutation && AllowAutoEnforcement;
        }

        public static BrickAiTrustBoundary Default { get; } = new BrickAiTrustBoundary(
            BrickAiMode.Off,
            BrickAiCommentFormat.Markdown,
            false,
            false,
            false);

        public BrickAiMode Mode { get; }
        public BrickAiCommentFormat CommentFormat { get; }
        public bool AllowRuleProposal { get; }
        public bool AllowAutoEnforcement { get; }
        public bool AllowsSilentPolicyMutation { get; }
        public bool CanActivateRuleWithoutReview => AllowAutoEnforcement && AllowsSilentPolicyMutation;
        public bool CanCreateSuppressionWithoutReview => AllowsSilentPolicyMutation;
        public bool CanCreateBaselineWithoutReview => AllowsSilentPolicyMutation;
    }

    public static class BrickAiCommentJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        public static string Serialize(BrickAiCommentDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return JsonSerializer.Serialize(ToDto(document), Options);
        }

        private static DocumentDto ToDto(BrickAiCommentDocument document) =>
            new DocumentDto
            {
                Schema = document.Schema,
                GeneratedAt = document.GeneratedAt,
                Comments = document.Comments.Select(ToDto).ToArray()
            };

        private static CommentDto ToDto(BrickAiViolationComment comment) =>
            new CommentDto
            {
                RuleId = comment.RuleId?.Value,
                RuleName = comment.RuleName,
                Decision = comment.Decision.ToString(),
                Severity = comment.Severity.ToString(),
                EvidenceLevel = comment.EvidenceLevel.ToString(),
                Source = ToElementDto(comment.Violation.Source, comment.Violation.ResolvedSourceRoles),
                Target = comment.Violation.Target == null ? null : ToElementDto(comment.Violation.Target, comment.Violation.ResolvedTargetRoles),
                Dependency = ToDependencyDto(comment.Violation),
                Problem = comment.ProblemSummary,
                WhyItMatters = comment.ArchitecturalReason,
                RecommendedOption = comment.RecommendedOption == null ? null : comment.RecommendedOption.Id,
                RepairOptions = comment.Options.Select(ToDto).ToArray(),
                AiRepairHints = comment.AiRepairHints.ToArray(),
                SuppressionGuidance = comment.SuppressionGuidance
            };

        private static ElementDto ToElementDto(BrickElement element, IEnumerable<RoleId> roles) =>
            new ElementDto
            {
                Name = string.IsNullOrWhiteSpace(element.FullName) ? element.DisplayName : element.FullName,
                Roles = roles.Select(role => role.Value).ToArray()
            };

        private static DependencyDto ToDependencyDto(BrickViolation violation) =>
            violation.DependencyKindId.HasValue || violation.DependencyLayer.HasValue
                ? new DependencyDto
                {
                    Kind = violation.DependencyKindId?.Value,
                    Layer = violation.DependencyLayer?.ToString()
                }
                : null;

        private static RepairOptionDto ToDto(BrickRemediationOption option) =>
            new RepairOptionDto
            {
                Id = option.Id,
                Kind = option.Kind.ToString(),
                Description = option.Description,
                WhenToUse = option.WhenToUse,
                Risk = option.Risk.ToString(),
                Preferred = option.IsPreferred
            };

        private sealed class DocumentDto
        {
            public string Schema { get; set; }
            public DateTimeOffset GeneratedAt { get; set; }
            public CommentDto[] Comments { get; set; }
        }

        private sealed class CommentDto
        {
            public string RuleId { get; set; }
            public string RuleName { get; set; }
            public string Decision { get; set; }
            public string Severity { get; set; }
            public string EvidenceLevel { get; set; }
            public ElementDto Source { get; set; }
            public ElementDto Target { get; set; }
            public DependencyDto Dependency { get; set; }
            public string Problem { get; set; }
            public string WhyItMatters { get; set; }
            public string RecommendedOption { get; set; }
            public RepairOptionDto[] RepairOptions { get; set; }
            public string[] AiRepairHints { get; set; }
            public string SuppressionGuidance { get; set; }
        }

        private sealed class ElementDto
        {
            public string Name { get; set; }
            public string[] Roles { get; set; }
        }

        private sealed class DependencyDto
        {
            public string Kind { get; set; }
            public string Layer { get; set; }
        }

        private sealed class RepairOptionDto
        {
            public string Id { get; set; }
            public string Kind { get; set; }
            public string Description { get; set; }
            public string WhenToUse { get; set; }
            public string Risk { get; set; }
            public bool Preferred { get; set; }
        }
    }
}
