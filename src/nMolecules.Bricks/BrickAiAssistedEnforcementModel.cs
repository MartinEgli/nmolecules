using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Describes the structural remediation category an AI explanation may suggest for a deterministic Bricks violation.
    /// </summary>
    public enum BrickRemediationKind
    {
        /// <summary>Introduce or reuse an abstraction so the source does not depend on a concrete target.</summary>
        IntroduceContract = 0,
        /// <summary>Move the source or target element to a role, namespace, or assembly that matches the policy.</summary>
        MoveElement = 1,
        /// <summary>Split an overloaded role into more precise roles before applying policy.</summary>
        SplitRole = 2,
        /// <summary>Add an explicit port and adapter boundary between source and target roles.</summary>
        AddPortAndAdapter = 3,
        /// <summary>Reverse or redirect a dependency so it follows the intended architecture direction.</summary>
        ChangeDependencyDirection = 4,
        /// <summary>Move wiring or construction logic to the composition root.</summary>
        MoveToCompositionRoot = 5,
        /// <summary>Change policy only through explicit review when the policy is too narrow or wrong.</summary>
        AdjustPolicy = 6,
        /// <summary>Add a reviewed suppression for an intentional and bounded exception.</summary>
        AddSuppression = 7,
        /// <summary>Add a reviewed baseline entry for an accepted existing violation.</summary>
        AddBaselineEntry = 8,
        /// <summary>Rename or reclassify an element when the current role assignment is misleading.</summary>
        RenameOrReclassifyElement = 9
    }

    /// <summary>
    /// Classifies the expected architectural risk of applying a remediation option.
    /// </summary>
    public enum BrickRemediationRisk
    {
        /// <summary>The remediation is expected to be local, reversible, and policy-aligned.</summary>
        Low = 0,
        /// <summary>The remediation may affect ownership, public API, or multiple project areas.</summary>
        Medium = 1,
        /// <summary>The remediation may hide violations, weaken governance, or require broad migration.</summary>
        High = 2
    }

    /// <summary>
    /// Represents the review lifecycle of an AI-generated rule proposal.
    /// </summary>
    public enum BrickRuleLifecycleState
    {
        /// <summary>AI observed a possible rule, but no human has accepted it for evaluation.</summary>
        Candidate = 0,
        /// <summary>A human marked the proposal as worth shaping and validating.</summary>
        Draft = 1,
        /// <summary>The proposed rule may run in reporting mode without affecting builds.</summary>
        Observing = 2,
        /// <summary>The proposed rule may emit warnings after review.</summary>
        Warning = 3,
        /// <summary>The rule is active enforcement and can break builds after explicit promotion.</summary>
        Enforced = 4,
        /// <summary>The proposal was reviewed and discarded.</summary>
        Rejected = 5,
        /// <summary>The proposal or promoted rule is no longer recommended.</summary>
        Deprecated = 6
    }

    /// <summary>
    /// Configures which AI-assisted Bricks capability is enabled for a run.
    /// </summary>
    public enum BrickAiMode
    {
        /// <summary>Only deterministic Bricks output is produced.</summary>
        Off = 0,
        /// <summary>AI-readable explanations and remediation hints may be produced for deterministic violations.</summary>
        Explain = 1,
        /// <summary>AI may also create advisory rule proposals that require review before enforcement.</summary>
        SuggestRules = 2
    }

    /// <summary>
    /// Selects the output shape for AI-assisted violation comments.
    /// </summary>
    public enum BrickAiCommentFormat
    {
        /// <summary>Emit human-readable Markdown comments.</summary>
        Markdown = 0,
        /// <summary>Emit machine-readable JSON comments.</summary>
        Json = 1,
        /// <summary>Emit both Markdown and JSON comments.</summary>
        Both = 2
    }

    /// <summary>
    /// Describes one possible remediation for a deterministic Bricks violation.
    /// </summary>
    public sealed class BrickRemediationOption
    {
        /// <summary>
        /// Creates a remediation option with its category, decision guidance, risk, and preference flag.
        /// </summary>
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

        /// <summary>Stable option identifier used by JSON output and recommendations.</summary>
        public string Id { get; }
        /// <summary>The structural remediation category.</summary>
        public BrickRemediationKind Kind { get; }
        /// <summary>Human-readable description of the change to make.</summary>
        public string Description { get; }
        /// <summary>Guidance for when this option is appropriate.</summary>
        public string WhenToUse { get; }
        /// <summary>Expected architectural risk of applying this option.</summary>
        public BrickRemediationRisk Risk { get; }
        /// <summary>Whether this option is preferred for the represented violation.</summary>
        public bool IsPreferred { get; }
    }

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

    /// <summary>
    /// Versioned document containing AI-ready comments for deterministic Bricks violations.
    /// </summary>
    public sealed class BrickAiCommentDocument
    {
        /// <summary>Current JSON schema identifier for AI comment documents.</summary>
        public const string CurrentSchema = "NMolecules.Bricks.AIComment/1.0";

        /// <summary>
        /// Creates a comment document using the current schema.
        /// </summary>
        public BrickAiCommentDocument(DateTimeOffset generatedAt, IEnumerable<BrickAiViolationComment> comments)
            : this(generatedAt, comments, CurrentSchema)
        {
        }

        /// <summary>
        /// Creates a comment document with an explicit schema identifier.
        /// </summary>
        public BrickAiCommentDocument(DateTimeOffset generatedAt, IEnumerable<BrickAiViolationComment> comments, string schema)
        {
            GeneratedAt = generatedAt;
            Schema = schema ?? string.Empty;
            Comments = (comments ?? Enumerable.Empty<BrickAiViolationComment>())
                .OrderBy(comment => comment.RuleId?.Value ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(comment => comment.Violation.Source.Id.Value, StringComparer.Ordinal)
                .ToArray();
        }

        /// <summary>Schema identifier used to serialize the document.</summary>
        public string Schema { get; }
        /// <summary>Time the document was generated.</summary>
        public DateTimeOffset GeneratedAt { get; }
        /// <summary>AI-ready comments sorted by rule id and source element id.</summary>
        public IReadOnlyList<BrickAiViolationComment> Comments { get; }
        /// <summary>Indicates whether the document uses <see cref="CurrentSchema"/>.</summary>
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }

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

    /// <summary>
    /// Advisory structural rule suggested by AI and awaiting deterministic review or promotion.
    /// </summary>
    /// <remarks>
    /// Proposals cannot start in the <see cref="BrickRuleLifecycleState.Enforced"/> state.
    /// Enforced rules must be promoted explicitly through a reviewed policy workflow.
    /// </remarks>
    public sealed class BrickRuleProposal
    {
        /// <summary>
        /// Creates an advisory rule proposal with selectors, suggested decision, severity, evidence, and lifecycle state.
        /// </summary>
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

        /// <summary>Stable proposal identifier.</summary>
        public string ProposalId { get; }
        /// <summary>Human-readable proposal title.</summary>
        public string Title { get; }
        /// <summary>Reason why the rule may be useful.</summary>
        public string Rationale { get; }
        /// <summary>Source-role selector for the proposed rule.</summary>
        public BrickRoleSelector SourceRoles { get; }
        /// <summary>Target-role selector for the proposed rule.</summary>
        public BrickRoleSelector TargetRoles { get; }
        /// <summary>Dependency kind the proposed rule applies to.</summary>
        public BrickDependencyKindId DependencyKindId { get; }
        /// <summary>Suggested deterministic decision if the proposal is later promoted.</summary>
        public BrickDecision SuggestedDecision { get; }
        /// <summary>Suggested severity if the proposal is later promoted.</summary>
        public BrickSeverity SuggestedSeverity { get; }
        /// <summary>Evidence supporting and constraining the proposal.</summary>
        public BrickRuleProposalEvidence Evidence { get; }
        /// <summary>Current review lifecycle state.</summary>
        public BrickRuleLifecycleState LifecycleState { get; }
        /// <summary>Indicates whether the proposal is still advisory.</summary>
        public bool IsAdvisory => LifecycleState != BrickRuleLifecycleState.Enforced;
        /// <summary>Indicates whether the proposal can currently break builds.</summary>
        public bool CanBreakBuild => LifecycleState == BrickRuleLifecycleState.Enforced;
        /// <summary>Indicates whether the proposal has the minimum evidence required for review.</summary>
        public bool HasRequiredEvidence => Evidence.HasRequiredEvidence;
    }

    /// <summary>
    /// Captures the safety boundary between deterministic Bricks enforcement and AI assistance.
    /// </summary>
    public sealed class BrickAiTrustBoundary
    {
        /// <summary>
        /// Creates a trust boundary configuration for AI comments, rule proposals, and automation.
        /// </summary>
        /// <remarks>
        /// Automatic enforcement is only preserved when silent policy mutation is explicitly allowed too.
        /// This keeps unsafe one-flag escalation from enabling build-breaking changes by accident.
        /// </remarks>
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

        /// <summary>Safe default: AI assistance off, no proposals, no automatic enforcement.</summary>
        public static BrickAiTrustBoundary Default { get; } = new BrickAiTrustBoundary(
            BrickAiMode.Off,
            BrickAiCommentFormat.Markdown,
            false,
            false,
            false);

        /// <summary>Selected AI assistance mode.</summary>
        public BrickAiMode Mode { get; }
        /// <summary>Selected AI comment output format.</summary>
        public BrickAiCommentFormat CommentFormat { get; }
        /// <summary>Whether AI may create advisory rule proposals.</summary>
        public bool AllowRuleProposal { get; }
        /// <summary>Whether automatic enforcement is allowed by this boundary.</summary>
        public bool AllowAutoEnforcement { get; }
        /// <summary>Whether policy mutations may happen without explicit review.</summary>
        public bool AllowsSilentPolicyMutation { get; }
        /// <summary>Whether a rule can be activated without review.</summary>
        public bool CanActivateRuleWithoutReview => AllowAutoEnforcement && AllowsSilentPolicyMutation;
        /// <summary>Whether a suppression can be created without review.</summary>
        public bool CanCreateSuppressionWithoutReview => AllowsSilentPolicyMutation;
        /// <summary>Whether a baseline entry can be created without review.</summary>
        public bool CanCreateBaselineWithoutReview => AllowsSilentPolicyMutation;
    }

    /// <summary>
    /// Serializes AI-ready Bricks violation comments to the versioned JSON schema.
    /// </summary>
    public static class BrickAiCommentJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes a comment document to compact camel-case JSON.
        /// </summary>
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
