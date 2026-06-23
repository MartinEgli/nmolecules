using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
    public enum BrickConformanceLevel
    {
        Marking = 0,
        StaticValidation = 1,
        Explainability = 2,
        PolicyFiles = 3,
        RuntimeAwareAnalysis = 4,
        IntegrationAndAugmentation = 5
    }

    public enum BrickConformanceCapabilityStatus
    {
        Satisfied = 0,
        Missing = 1,
        NotApplicable = 2
    }

    public enum BrickConformanceLevelStatus
    {
        Achieved = 0,
        Partial = 1,
        NotStarted = 2
    }

    public sealed class BrickConformanceCapability
    {
        public BrickConformanceCapability(
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

    public sealed class BrickConformanceLevelDefinition
    {
        public BrickConformanceLevelDefinition(
            BrickConformanceLevel level,
            string displayName,
            string description,
            IEnumerable<BrickConformanceCapability> capabilities)
        {
            Level = level;
            DisplayName = displayName ?? string.Empty;
            Description = description ?? string.Empty;
            Capabilities = (capabilities ?? Enumerable.Empty<BrickConformanceCapability>())
                .Where(capability => capability != null)
                .OrderBy(capability => capability.Id, StringComparer.Ordinal)
                .ToArray();
        }

        public BrickConformanceLevel Level { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public IReadOnlyList<BrickConformanceCapability> Capabilities { get; }
    }

    public sealed class BrickConformanceCapabilityResult
    {
        public BrickConformanceCapabilityResult(
            BrickConformanceCapability capability,
            BrickConformanceCapabilityStatus status,
            string evidence = null)
        {
            Capability = capability ?? throw new ArgumentNullException(nameof(capability));
            Status = status;
            Evidence = evidence ?? string.Empty;
            SatisfiesRequirement = !Capability.Required || Status == BrickConformanceCapabilityStatus.Satisfied;
        }

        public BrickConformanceCapability Capability { get; }
        public BrickConformanceCapabilityStatus Status { get; }
        public string Evidence { get; }
        public bool SatisfiesRequirement { get; }
    }

    public static class BrickBuiltInConformanceLevels
    {
        public static BrickConformanceLevelDefinition Marking => Level(
            BrickConformanceLevel.Marking,
            "Marking",
            "Structural roles can be marked and named consistently.",
            Capability("role-attributes", "Role attributes", true, "Roles can be expressed in code."),
            Capability("alias-attributes", "Alias attributes", true, "Aliases can adapt existing structural names."),
            Capability("typed-identifiers", "Typed identifiers", true, "Core identifiers avoid stringly typed policy code."));

        public static BrickConformanceLevelDefinition StaticValidation => Level(
            BrickConformanceLevel.StaticValidation,
            "Static validation",
            "Compiler-visible structure can be evaluated deterministically.",
            Capability("type-references", "Type references", true, "Static type dependencies are modelled."),
            Capability("inheritance", "Inheritance", true, "Inheritance dependencies are visible to static validation."),
            Capability("interface-implementation", "Interface implementation", true, "Interface implementation edges are visible to static validation."),
            Capability("object-creation", "Object creation", true, "Object creation dependencies are represented."),
            Capability("basic-rule-validation", "Basic rule validation", true, "Policies can evaluate allowed and forbidden dependencies."),
            Capability("analyzer-diagnostics", "Analyzer diagnostics", true, "Violations can surface as diagnostics."));

        public static BrickConformanceLevelDefinition Explainability => Level(
            BrickConformanceLevel.Explainability,
            "Explainability",
            "Resolution and violation decisions can be explained.",
            Capability("resolution-traces", "Resolution traces", true, "Role resolution keeps explainable trace output."),
            Capability("evidence", "Evidence", true, "Assignments and violations preserve evidence quality."),
            Capability("normalized-violations", "Normalized violations", true, "Policy outcomes are normalized for consumption."),
            Capability("structured-diagnostic-messages", "Structured diagnostic messages", true, "Diagnostics use stable structured message data."));

        public static BrickConformanceLevelDefinition PolicyFiles => Level(
            BrickConformanceLevel.PolicyFiles,
            "Policy files",
            "External policy configuration can be versioned and composed.",
            Capability("schema-versioned-policy-files", "Schema-versioned policy files", true, "Policy files have a machine-readable schema."),
            Capability("baseline-files", "Baseline files", true, "Accepted legacy violations can be represented."),
            Capability("suppression-files", "Suppression files", true, "Suppressions can be represented outside source code."),
            Capability("policy-imports", "Policy imports", true, "Policies can compose other policies."),
            Capability("configuration-precedence", "Configuration precedence", true, "Configuration sources resolve deterministically."));

        public static BrickConformanceLevelDefinition RuntimeAwareAnalysis => Level(
            BrickConformanceLevel.RuntimeAwareAnalysis,
            "Runtime-aware analysis",
            "Runtime-relevant dependency kinds can be represented with evidence quality.",
            Capability("dependency-registrations", "Dependency registrations", true, "Dependency injection registrations are modelled."),
            Capability("friend-assembly", "Friend assembly", true, "InternalsVisibleTo visibility dependencies are modelled."),
            Capability("reflection-access", "Reflection access", true, "Reflection access can be represented explicitly."),
            Capability("runtime-activation", "Runtime activation", true, "Runtime activation edges are modelled."),
            Capability("evidence-confidence-levels", "Evidence confidence levels", true, "Runtime-aware analysis carries evidence confidence."));

        public static BrickConformanceLevelDefinition IntegrationAndAugmentation => Level(
            BrickConformanceLevel.IntegrationAndAugmentation,
            "Integration and augmentation",
            "Bricks output can be consumed by tools and improvement loops.",
            Capability("ide-visualisation", "IDE visualisation", true, "IDE surfaces can consume Bricks status."),
            Capability("report-generation", "Report generation", true, "Machine-readable reports are emitted."),
            Capability("generated-architecture-documentation", "Generated architecture documentation", true, "Architecture documentation can be generated from reports."),
            Capability("technical-integrations", "Technical integrations", true, "External tools can integrate with Bricks outputs."),
            Capability("sarif-output", "SARIF output", true, "Analyzer-compatible SARIF reports can be produced."),
            Capability("benchmarking", "Benchmarking", true, "Central Bricks behaviour can be benchmarked."));

        public static IReadOnlyList<BrickConformanceLevelDefinition> All => new[]
        {
            Marking,
            StaticValidation,
            Explainability,
            PolicyFiles,
            RuntimeAwareAnalysis,
            IntegrationAndAugmentation
        };

        private static BrickConformanceLevelDefinition Level(
            BrickConformanceLevel level,
            string displayName,
            string description,
            params BrickConformanceCapability[] capabilities) =>
            new BrickConformanceLevelDefinition(level, displayName, description, capabilities);

        private static BrickConformanceCapability Capability(
            string id,
            string displayName,
            bool required,
            string rationale) =>
            new BrickConformanceCapability(id, displayName, required, rationale);
    }

    public sealed class BrickConformanceLevelAssessment
    {
        public BrickConformanceLevelAssessment(
            BrickConformanceLevelDefinition definition,
            IEnumerable<BrickConformanceCapabilityResult> results)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Results = (results ?? Enumerable.Empty<BrickConformanceCapabilityResult>())
                .Where(result => result != null)
                .OrderBy(result => result.Capability.Id, StringComparer.Ordinal)
                .ToArray();

            var requiredCapabilities = Definition.Capabilities
                .Where(capability => capability.Required)
                .ToArray();
            RequiredCapabilityCount = requiredCapabilities.Length;
            SatisfiedRequiredCapabilityCount = requiredCapabilities.Count(IsSatisfied);
            MissingRequiredCapabilityCount = RequiredCapabilityCount - SatisfiedRequiredCapabilityCount;
            CompletionRatio = RequiredCapabilityCount == 0
                ? 1d
                : (double)SatisfiedRequiredCapabilityCount / RequiredCapabilityCount;
            Status = ResolveStatus();
        }

        public BrickConformanceLevelDefinition Definition { get; }
        public IReadOnlyList<BrickConformanceCapabilityResult> Results { get; }
        public int RequiredCapabilityCount { get; }
        public int SatisfiedRequiredCapabilityCount { get; }
        public int MissingRequiredCapabilityCount { get; }
        public double CompletionRatio { get; }
        public BrickConformanceLevelStatus Status { get; }

        private bool IsSatisfied(BrickConformanceCapability capability) =>
            Results.Any(result =>
                string.Equals(result.Capability.Id, capability.Id, StringComparison.Ordinal) &&
                result.SatisfiesRequirement);

        private BrickConformanceLevelStatus ResolveStatus()
        {
            if (MissingRequiredCapabilityCount == 0)
            {
                return BrickConformanceLevelStatus.Achieved;
            }

            return SatisfiedRequiredCapabilityCount > 0
                ? BrickConformanceLevelStatus.Partial
                : BrickConformanceLevelStatus.NotStarted;
        }
    }

    public sealed class BrickConformanceSummary
    {
        private BrickConformanceSummary(
            int totalLevels,
            int achievedLevels,
            int partialLevels,
            int notStartedLevels,
            int missingRequiredCapabilities,
            BrickConformanceLevel? highestContiguousAchievedLevel)
        {
            TotalLevels = totalLevels;
            AchievedLevels = achievedLevels;
            PartialLevels = partialLevels;
            NotStartedLevels = notStartedLevels;
            MissingRequiredCapabilities = missingRequiredCapabilities;
            HighestContiguousAchievedLevel = highestContiguousAchievedLevel;
        }

        public int TotalLevels { get; }
        public int AchievedLevels { get; }
        public int PartialLevels { get; }
        public int NotStartedLevels { get; }
        public int MissingRequiredCapabilities { get; }
        public BrickConformanceLevel? HighestContiguousAchievedLevel { get; }

        public static BrickConformanceSummary FromAssessments(
            IEnumerable<BrickConformanceLevelAssessment> assessments)
        {
            var items = (assessments ?? Enumerable.Empty<BrickConformanceLevelAssessment>())
                .Where(assessment => assessment != null)
                .OrderBy(assessment => assessment.Definition.Level)
                .ToArray();

            return new BrickConformanceSummary(
                items.Length,
                items.Count(assessment => assessment.Status == BrickConformanceLevelStatus.Achieved),
                items.Count(assessment => assessment.Status == BrickConformanceLevelStatus.Partial),
                items.Count(assessment => assessment.Status == BrickConformanceLevelStatus.NotStarted),
                items.Sum(assessment => assessment.MissingRequiredCapabilityCount),
                ResolveHighestContiguous(items));
        }

        private static BrickConformanceLevel? ResolveHighestContiguous(
            IReadOnlyList<BrickConformanceLevelAssessment> assessments)
        {
            BrickConformanceLevel? highest = null;
            foreach (var expectedLevel in BrickBuiltInConformanceLevels.All.Select(level => level.Level))
            {
                var assessment = assessments.FirstOrDefault(item => item.Definition.Level == expectedLevel);
                if (assessment == null || assessment.Status != BrickConformanceLevelStatus.Achieved)
                {
                    break;
                }

                highest = expectedLevel;
            }

            return highest;
        }
    }

    public sealed class BrickConformanceReport
    {
        public const string CurrentSchema = "NMolecules.Bricks.Conformance/1.0";

        public BrickConformanceReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickConformanceLevelAssessment> assessments)
            : this(generatedAt, assessments, CurrentSchema)
        {
        }

        public BrickConformanceReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickConformanceLevelAssessment> assessments,
            string schema)
        {
            GeneratedAt = generatedAt;
            Assessments = (assessments ?? Enumerable.Empty<BrickConformanceLevelAssessment>())
                .Where(assessment => assessment != null)
                .OrderBy(assessment => assessment.Definition.Level)
                .ToArray();
            Schema = schema ?? string.Empty;
            Summary = BrickConformanceSummary.FromAssessments(Assessments);
        }

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickConformanceLevelAssessment> Assessments { get; }
        public BrickConformanceSummary Summary { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }

    public static class BrickConformanceReportJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string Serialize(BrickConformanceReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            return JsonSerializer.Serialize(ToDto(report), Options);
        }

        private static ReportDto ToDto(BrickConformanceReport report) =>
            new ReportDto
            {
                Schema = report.Schema,
                GeneratedAt = report.GeneratedAt,
                Summary = new SummaryDto
                {
                    TotalLevels = report.Summary.TotalLevels,
                    AchievedLevels = report.Summary.AchievedLevels,
                    PartialLevels = report.Summary.PartialLevels,
                    NotStartedLevels = report.Summary.NotStartedLevels,
                    MissingRequiredCapabilities = report.Summary.MissingRequiredCapabilities,
                    HighestContiguousAchievedLevel = report.Summary.HighestContiguousAchievedLevel?.ToString()
                },
                Assessments = report.Assessments.Select(ToDto).ToArray()
            };

        private static AssessmentDto ToDto(BrickConformanceLevelAssessment assessment) =>
            new AssessmentDto
            {
                Level = assessment.Definition.Level.ToString(),
                DisplayName = assessment.Definition.DisplayName,
                Description = assessment.Definition.Description,
                Status = assessment.Status.ToString(),
                RequiredCapabilityCount = assessment.RequiredCapabilityCount,
                SatisfiedRequiredCapabilityCount = assessment.SatisfiedRequiredCapabilityCount,
                MissingRequiredCapabilityCount = assessment.MissingRequiredCapabilityCount,
                CompletionRatio = assessment.CompletionRatio,
                Results = assessment.Results.Select(ToDto).ToArray()
            };

        private static ResultDto ToDto(BrickConformanceCapabilityResult result) =>
            new ResultDto
            {
                CapabilityId = result.Capability.Id,
                CapabilityDisplayName = result.Capability.DisplayName,
                Required = result.Capability.Required,
                Status = result.Status.ToString(),
                Evidence = result.Evidence,
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
            public int TotalLevels { get; set; }
            public int AchievedLevels { get; set; }
            public int PartialLevels { get; set; }
            public int NotStartedLevels { get; set; }
            public int MissingRequiredCapabilities { get; set; }
            public string HighestContiguousAchievedLevel { get; set; }
        }

        private sealed class AssessmentDto
        {
            public string Level { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
            public int RequiredCapabilityCount { get; set; }
            public int SatisfiedRequiredCapabilityCount { get; set; }
            public int MissingRequiredCapabilityCount { get; set; }
            public double CompletionRatio { get; set; }
            public ResultDto[] Results { get; set; }
        }

        private sealed class ResultDto
        {
            public string CapabilityId { get; set; }
            public string CapabilityDisplayName { get; set; }
            public bool Required { get; set; }
            public string Status { get; set; }
            public string Evidence { get; set; }
            public bool SatisfiesRequirement { get; set; }
        }
    }
}
