using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
    public enum BrickRoadmapStage
    {
        V1 = 0,
        V1_1 = 1,
        V1_2 = 2,
        V2 = 3
    }

    public enum BrickRoadmapItemStatus
    {
        Completed = 0,
        Missing = 1,
        NotRequired = 2
    }

    public enum BrickRoadmapStageStatus
    {
        Complete = 0,
        Partial = 1,
        NotStarted = 2
    }

    public sealed class BrickRoadmapItem
    {
        public BrickRoadmapItem(
            string id,
            string displayName,
            bool required,
            string rationale = null)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Required = required;
            Rationale = rationale ?? string.Empty;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public bool Required { get; }
        public string Rationale { get; }
    }

    public sealed class BrickRoadmapStageDefinition
    {
        public BrickRoadmapStageDefinition(
            BrickRoadmapStage stage,
            string displayName,
            string description,
            IEnumerable<BrickRoadmapItem> includedItems,
            IEnumerable<BrickRoadmapItem> excludedItems)
        {
            Stage = stage;
            DisplayName = displayName ?? string.Empty;
            Description = description ?? string.Empty;
            IncludedItems = NormalizeItems(includedItems);
            ExcludedItems = NormalizeItems(excludedItems);
        }

        public BrickRoadmapStage Stage { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public IReadOnlyList<BrickRoadmapItem> IncludedItems { get; }
        public IReadOnlyList<BrickRoadmapItem> ExcludedItems { get; }

        private static IReadOnlyList<BrickRoadmapItem> NormalizeItems(
            IEnumerable<BrickRoadmapItem> items) =>
            (items ?? Enumerable.Empty<BrickRoadmapItem>())
                .Where(item => item != null)
                .OrderBy(item => item.Id, StringComparer.Ordinal)
                .ToArray();
    }

    public sealed class BrickRoadmapItemResult
    {
        public BrickRoadmapItemResult(
            BrickRoadmapItem item,
            BrickRoadmapItemStatus status,
            string evidence = null)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Status = status;
            Evidence = evidence ?? string.Empty;
            SatisfiesRequirement = !Item.Required || Status == BrickRoadmapItemStatus.Completed;
        }

        public BrickRoadmapItem Item { get; }
        public BrickRoadmapItemStatus Status { get; }
        public string Evidence { get; }
        public bool SatisfiesRequirement { get; }
    }

    public static class BrickBuiltInRoadmapStages
    {
        public static BrickRoadmapStageDefinition V1 => Stage(
            BrickRoadmapStage.V1,
            "V1",
            "Deliberately small baseline for direct marking and static validation.",
            new[]
            {
                Item("direct-role-assignment", "Direct role assignment", true, "Roles can be assigned directly."),
                Item("alias-based-role-mapping", "Alias-based role mapping", true, "Aliases can map existing markers to roles."),
                Item("typed-role-rule-identifiers", "Typed role and rule identifiers", true, "Identifiers are stable value objects."),
                Item("minimal-role-dimensions", "Minimal role dimensions", true, "Role dimensions exist for core classification."),
                Item("deterministic-role-resolution", "Deterministic role resolution", true, "Role resolution is stable for the same input."),
                Item("basic-resolution-conflict-diagnostics", "Basic resolution conflict diagnostics", true, "Conflicts are represented explicitly."),
                Item("deterministic-rule-evaluation", "Deterministic rule evaluation", true, "Policy rules evaluate deterministically."),
                Item("explicit-policy-defaults", "Explicit policy defaults", true, "Policies expose explicit defaults."),
                Item("static-role-based-dependency-validation", "Static role-based dependency validation", true, "Static dependencies can be evaluated by role."),
                Item("allow-deny-require-decision-semantics", "Allow, Deny, and Require semantics", true, "Decision types remain distinct."),
                Item("type-scope-support", "Type scope support", true, "At least type-level evaluation is supported."),
                Item("member-cardinality-contract-validation", "Member cardinality contract validation", true, "Member cardinality contracts can be checked."),
                Item("analyzer-diagnostics", "Analyzer diagnostics", true, "Validation can produce diagnostics."),
                Item("minimal-suppression-support", "Minimal suppression support", true, "Accepted exceptions can be represented.")
            },
            new[]
            {
                Item("full-external-policy-files", "Full external policy files", false, "V1 does not require full external policy files."),
                Item("di-registration-analysis", "DI registration analysis", false, "V1 does not require DI registration analysis."),
                Item("reflection-analysis", "Reflection analysis", false, "V1 does not require reflection analysis."),
                Item("runtime-wiring-analysis", "Runtime wiring analysis", false, "V1 does not require runtime wiring analysis."),
                Item("rich-export-reporting", "Rich export reporting", false, "V1 does not require rich export reporting."),
                Item("broad-ide-visualisation", "Broad IDE visualisation", false, "V1 does not require broad IDE visualisation."),
                Item("complete-pack-bridge-ecosystem", "Complete pack bridge ecosystem", false, "V1 does not require a complete bridge ecosystem.")
            });

        public static BrickRoadmapStageDefinition V1_1 => Stage(
            BrickRoadmapStage.V1_1,
            "V1.1",
            "Policy file and report foundations.",
            new[]
            {
                Item("external-policy-file-prototype", "External policy file prototype", true, "External policy input has a first contract."),
                Item("schema-versioning-policy-files", "Schema versioning for policy files", true, "Policy files declare a schema."),
                Item("resolution-trace-export", "Resolution trace export", true, "Resolution traces can be exported."),
                Item("baseline-file-support", "Baseline file support", true, "Accepted legacy findings can be baselined."),
                Item("first-report-output", "First report output", true, "Validation output can be serialized."),
                Item("stricter-diagnostic-governance", "Stricter diagnostic governance", true, "Diagnostic IDs are governed."),
                Item("basic-pack-bridge-ddd", "Basic pack bridge for DDD", true, "DDD bridge metadata is represented.")
            },
            null);

        public static BrickRoadmapStageDefinition V1_2 => Stage(
            BrickRoadmapStage.V1_2,
            "V1.2",
            "Testing APIs and richer export surfaces.",
            new[]
            {
                Item("architecture-testing-api", "Architecture testing API", true, "Bricks can support architecture tests."),
                Item("json-report-export", "JSON report export", true, "Reports can be serialized to JSON."),
                Item("role-map-export", "Role map export", true, "Resolved roles can be exported."),
                Item("dependency-graph-export", "Dependency graph export", true, "Dependency graphs can be exported."),
                Item("suppression-baseline-reports", "Suppression and baseline reports", true, "Suppression and baseline state can be reported."),
                Item("pack-bridges-events-architecture", "Pack bridges for Events and Architecture", true, "Events and Architecture bridges are represented.")
            },
            null);

        public static BrickRoadmapStageDefinition V2 => Stage(
            BrickRoadmapStage.V2,
            "V2",
            "Runtime-aware analysis and richer profiles.",
            new[]
            {
                Item("di-registration-dependency-analysis", "DI registration dependency analysis", true, "DI registration dependencies can be analysed."),
                Item("internals-visible-to-analysis", "InternalsVisibleTo analysis", true, "Friend assembly visibility is represented."),
                Item("visibility-dependency-layer", "Visibility dependency layer", true, "Visibility dependencies are first-class."),
                Item("runtime-wiring-dependency-layer", "Runtime and wiring dependency layer", true, "Runtime wiring dependencies are first-class."),
                Item("reflection-modelling-confidence", "Reflection modelling with confidence", true, "Reflection dependencies carry confidence levels."),
                Item("advanced-policy-composition", "Advanced policy composition", true, "Policies can compose through explicit modes."),
                Item("profile-packages", "Profile packages", true, "Layered, Onion, Hexagonal, and CQRS profiles exist."),
                Item("richer-ide-support", "Richer IDE support", true, "IDE support can consume richer Bricks state.")
            },
            null);

        public static IReadOnlyList<BrickRoadmapStageDefinition> All => new[] { V1, V1_1, V1_2, V2 };

        private static BrickRoadmapStageDefinition Stage(
            BrickRoadmapStage stage,
            string displayName,
            string description,
            IEnumerable<BrickRoadmapItem> includedItems,
            IEnumerable<BrickRoadmapItem> excludedItems) =>
            new BrickRoadmapStageDefinition(stage, displayName, description, includedItems, excludedItems);

        private static BrickRoadmapItem Item(
            string id,
            string displayName,
            bool required,
            string rationale) =>
            new BrickRoadmapItem(id, displayName, required, rationale);
    }

    public sealed class BrickRoadmapStageAssessment
    {
        public BrickRoadmapStageAssessment(
            BrickRoadmapStageDefinition definition,
            IEnumerable<BrickRoadmapItemResult> results)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Results = (results ?? Enumerable.Empty<BrickRoadmapItemResult>())
                .Where(result => result != null)
                .OrderBy(result => result.Item.Id, StringComparer.Ordinal)
                .ToArray();

            var requiredItems = Definition.IncludedItems.Where(item => item.Required).ToArray();
            RequiredItemCount = requiredItems.Length;
            CompletedRequiredItemCount = requiredItems.Count(IsCompleted);
            MissingRequiredItemCount = RequiredItemCount - CompletedRequiredItemCount;
            CompletionRatio = RequiredItemCount == 0
                ? 1d
                : (double)CompletedRequiredItemCount / RequiredItemCount;
            Status = ResolveStatus();
        }

        public BrickRoadmapStageDefinition Definition { get; }
        public IReadOnlyList<BrickRoadmapItemResult> Results { get; }
        public int RequiredItemCount { get; }
        public int CompletedRequiredItemCount { get; }
        public int MissingRequiredItemCount { get; }
        public double CompletionRatio { get; }
        public BrickRoadmapStageStatus Status { get; }

        private bool IsCompleted(BrickRoadmapItem item) =>
            Results.Any(result =>
                string.Equals(result.Item.Id, item.Id, StringComparison.Ordinal) &&
                result.SatisfiesRequirement);

        private BrickRoadmapStageStatus ResolveStatus()
        {
            if (MissingRequiredItemCount == 0)
            {
                return BrickRoadmapStageStatus.Complete;
            }

            return CompletedRequiredItemCount > 0
                ? BrickRoadmapStageStatus.Partial
                : BrickRoadmapStageStatus.NotStarted;
        }
    }

    public sealed class BrickRoadmapSummary
    {
        private BrickRoadmapSummary(
            int totalStages,
            int completeStages,
            int partialStages,
            int notStartedStages,
            int missingRequiredItems,
            BrickRoadmapStage? highestContiguousCompleteStage)
        {
            TotalStages = totalStages;
            CompleteStages = completeStages;
            PartialStages = partialStages;
            NotStartedStages = notStartedStages;
            MissingRequiredItems = missingRequiredItems;
            HighestContiguousCompleteStage = highestContiguousCompleteStage;
        }

        public int TotalStages { get; }
        public int CompleteStages { get; }
        public int PartialStages { get; }
        public int NotStartedStages { get; }
        public int MissingRequiredItems { get; }
        public BrickRoadmapStage? HighestContiguousCompleteStage { get; }

        public static BrickRoadmapSummary FromAssessments(
            IEnumerable<BrickRoadmapStageAssessment> assessments)
        {
            var items = (assessments ?? Enumerable.Empty<BrickRoadmapStageAssessment>())
                .Where(assessment => assessment != null)
                .OrderBy(assessment => assessment.Definition.Stage)
                .ToArray();

            return new BrickRoadmapSummary(
                items.Length,
                items.Count(assessment => assessment.Status == BrickRoadmapStageStatus.Complete),
                items.Count(assessment => assessment.Status == BrickRoadmapStageStatus.Partial),
                items.Count(assessment => assessment.Status == BrickRoadmapStageStatus.NotStarted),
                items.Sum(assessment => assessment.MissingRequiredItemCount),
                ResolveHighestContiguous(items));
        }

        private static BrickRoadmapStage? ResolveHighestContiguous(
            IReadOnlyList<BrickRoadmapStageAssessment> assessments)
        {
            BrickRoadmapStage? highest = null;
            foreach (var expectedStage in BrickBuiltInRoadmapStages.All.Select(stage => stage.Stage))
            {
                var assessment = assessments.FirstOrDefault(item => item.Definition.Stage == expectedStage);
                if (assessment == null || assessment.Status != BrickRoadmapStageStatus.Complete)
                {
                    break;
                }

                highest = expectedStage;
            }

            return highest;
        }
    }

    public sealed class BrickRoadmapReport
    {
        public const string CurrentSchema = "NMolecules.Bricks.Roadmap/1.0";

        public BrickRoadmapReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickRoadmapStageAssessment> assessments)
            : this(generatedAt, assessments, CurrentSchema)
        {
        }

        public BrickRoadmapReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickRoadmapStageAssessment> assessments,
            string schema)
        {
            GeneratedAt = generatedAt;
            Assessments = (assessments ?? Enumerable.Empty<BrickRoadmapStageAssessment>())
                .Where(assessment => assessment != null)
                .OrderBy(assessment => assessment.Definition.Stage)
                .ToArray();
            Schema = schema ?? string.Empty;
            Summary = BrickRoadmapSummary.FromAssessments(Assessments);
        }

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickRoadmapStageAssessment> Assessments { get; }
        public BrickRoadmapSummary Summary { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }

    public static class BrickRoadmapReportJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string Serialize(BrickRoadmapReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            return JsonSerializer.Serialize(ToDto(report), Options);
        }

        private static ReportDto ToDto(BrickRoadmapReport report) =>
            new ReportDto
            {
                Schema = report.Schema,
                GeneratedAt = report.GeneratedAt,
                Summary = new SummaryDto
                {
                    TotalStages = report.Summary.TotalStages,
                    CompleteStages = report.Summary.CompleteStages,
                    PartialStages = report.Summary.PartialStages,
                    NotStartedStages = report.Summary.NotStartedStages,
                    MissingRequiredItems = report.Summary.MissingRequiredItems,
                    HighestContiguousCompleteStage = report.Summary.HighestContiguousCompleteStage?.ToString()
                },
                Assessments = report.Assessments.Select(ToDto).ToArray()
            };

        private static AssessmentDto ToDto(BrickRoadmapStageAssessment assessment) =>
            new AssessmentDto
            {
                Stage = assessment.Definition.Stage.ToString(),
                DisplayName = assessment.Definition.DisplayName,
                Description = assessment.Definition.Description,
                Status = assessment.Status.ToString(),
                RequiredItemCount = assessment.RequiredItemCount,
                CompletedRequiredItemCount = assessment.CompletedRequiredItemCount,
                MissingRequiredItemCount = assessment.MissingRequiredItemCount,
                CompletionRatio = assessment.CompletionRatio,
                Results = assessment.Results.Select(ToDto).ToArray()
            };

        private static ResultDto ToDto(BrickRoadmapItemResult result) =>
            new ResultDto
            {
                ItemId = result.Item.Id,
                ItemDisplayName = result.Item.DisplayName,
                Required = result.Item.Required,
                Status = result.Status.ToString(),
                Evidence = string.IsNullOrEmpty(result.Evidence) ? null : result.Evidence,
                SatisfiesRequirement = result.SatisfiesRequirement
            };

        private sealed class ReportDto
        {
            public string Schema { get; set; }
            public DateTimeOffset GeneratedAt { get; set; }
            public SummaryDto Summary { get; set; }
            public AssessmentDto[] Assessments { get; set; }
        }

        private sealed class SummaryDto
        {
            public int TotalStages { get; set; }
            public int CompleteStages { get; set; }
            public int PartialStages { get; set; }
            public int NotStartedStages { get; set; }
            public int MissingRequiredItems { get; set; }
            public string HighestContiguousCompleteStage { get; set; }
        }

        private sealed class AssessmentDto
        {
            public string Stage { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
            public int RequiredItemCount { get; set; }
            public int CompletedRequiredItemCount { get; set; }
            public int MissingRequiredItemCount { get; set; }
            public double CompletionRatio { get; set; }
            public ResultDto[] Results { get; set; }
        }

        private sealed class ResultDto
        {
            public string ItemId { get; set; }
            public string ItemDisplayName { get; set; }
            public bool Required { get; set; }
            public string Status { get; set; }
            public string Evidence { get; set; }
            public bool SatisfiesRequirement { get; set; }
        }
    }
}
