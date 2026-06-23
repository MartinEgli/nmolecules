using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
    public enum BrickGovernanceArea
    {
        PolicyOwnership = 0,
        ExceptionHandling = 1,
        RolePackEvolution = 2,
        CompatibilityExpectations = 3
    }

    public enum BrickGovernanceRequirementStatus
    {
        Satisfied = 0,
        Missing = 1,
        NotApplicable = 2
    }

    public enum BrickGovernanceAreaStatus
    {
        Compliant = 0,
        Partial = 1,
        NonCompliant = 2
    }

    public sealed class BrickGovernanceRequirement
    {
        public BrickGovernanceRequirement(
            string id,
            BrickGovernanceArea area,
            string displayName,
            bool required,
            string rationale = null)
        {
            Id = id ?? string.Empty;
            Area = area;
            DisplayName = displayName ?? string.Empty;
            Required = required;
            Rationale = rationale ?? string.Empty;
        }

        public string Id { get; }
        public BrickGovernanceArea Area { get; }
        public string DisplayName { get; }
        public bool Required { get; }
        public string Rationale { get; }
    }

    public sealed class BrickGovernanceAreaDefinition
    {
        public BrickGovernanceAreaDefinition(
            BrickGovernanceArea area,
            string displayName,
            string description,
            IEnumerable<BrickGovernanceRequirement> requirements)
        {
            Area = area;
            DisplayName = displayName ?? string.Empty;
            Description = description ?? string.Empty;
            Requirements = (requirements ?? Enumerable.Empty<BrickGovernanceRequirement>())
                .Where(requirement => requirement != null)
                .OrderBy(requirement => requirement.Id, StringComparer.Ordinal)
                .ToArray();
        }

        public BrickGovernanceArea Area { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public IReadOnlyList<BrickGovernanceRequirement> Requirements { get; }
    }

    public sealed class BrickGovernanceRequirementResult
    {
        public BrickGovernanceRequirementResult(
            BrickGovernanceRequirement requirement,
            BrickGovernanceRequirementStatus status,
            string evidence = null)
        {
            Requirement = requirement ?? throw new ArgumentNullException(nameof(requirement));
            Status = status;
            Evidence = evidence ?? string.Empty;
            SatisfiesRequirement = !Requirement.Required || Status == BrickGovernanceRequirementStatus.Satisfied;
        }

        public BrickGovernanceRequirement Requirement { get; }
        public BrickGovernanceRequirementStatus Status { get; }
        public string Evidence { get; }
        public bool SatisfiesRequirement { get; }
    }

    public static class BrickBuiltInGovernanceAreas
    {
        public static BrickGovernanceAreaDefinition PolicyOwnership => Area(
            BrickGovernanceArea.PolicyOwnership,
            "Policy ownership",
            "Policies have explicit accountability and review ownership.",
            Requirement("policy-owner", BrickGovernanceArea.PolicyOwnership, "Policy owner", true, "Every activated policy has an accountable owner."),
            Requirement("review-process", BrickGovernanceArea.PolicyOwnership, "Review process", true, "Policy changes are reviewed like code."),
            Requirement("change-history", BrickGovernanceArea.PolicyOwnership, "Change history", true, "Policy changes remain auditable."));

        public static BrickGovernanceAreaDefinition ExceptionHandling => Area(
            BrickGovernanceArea.ExceptionHandling,
            "Exception handling",
            "Exceptions are deliberate, separate from baselines, and time-aware.",
            Requirement("suppression-justification", BrickGovernanceArea.ExceptionHandling, "Suppression justification", true, "Suppressions carry an intentional reason."),
            Requirement("suppression-owner", BrickGovernanceArea.ExceptionHandling, "Suppression owner", true, "Suppressions carry an accountable owner."),
            Requirement("expiration-policy", BrickGovernanceArea.ExceptionHandling, "Expiration policy", true, "Exceptions can expire and be revisited."),
            Requirement("baseline-separation", BrickGovernanceArea.ExceptionHandling, "Baseline separation", true, "Baselines and suppressions stay separate."));

        public static BrickGovernanceAreaDefinition RolePackEvolution => Area(
            BrickGovernanceArea.RolePackEvolution,
            "Role-pack evolution",
            "Role packs can evolve without hidden compatibility drift.",
            Requirement("role-pack-owner", BrickGovernanceArea.RolePackEvolution, "Role-pack owner", true, "Role packs have ownership."),
            Requirement("role-pack-versioning", BrickGovernanceArea.RolePackEvolution, "Role-pack versioning", true, "Role-pack changes are versioned."),
            Requirement("role-pack-deprecation-path", BrickGovernanceArea.RolePackEvolution, "Role-pack deprecation path", true, "Role-pack changes have a migration path."));

        public static BrickGovernanceAreaDefinition CompatibilityExpectations => Area(
            BrickGovernanceArea.CompatibilityExpectations,
            "Compatibility expectations",
            "Public contracts and machine-readable outputs stay compatible.",
            Requirement("schema-versioning", BrickGovernanceArea.CompatibilityExpectations, "Schema versioning", true, "Export contracts carry schema versions."),
            Requirement("stable-diagnostic-ids", BrickGovernanceArea.CompatibilityExpectations, "Stable diagnostic IDs", true, "Diagnostic IDs remain stable."),
            Requirement("breaking-change-review", BrickGovernanceArea.CompatibilityExpectations, "Breaking-change review", true, "Breaking changes are intentional and reviewed."));

        public static IReadOnlyList<BrickGovernanceAreaDefinition> All => new[]
        {
            PolicyOwnership,
            ExceptionHandling,
            RolePackEvolution,
            CompatibilityExpectations
        };

        private static BrickGovernanceAreaDefinition Area(
            BrickGovernanceArea area,
            string displayName,
            string description,
            params BrickGovernanceRequirement[] requirements) =>
            new BrickGovernanceAreaDefinition(area, displayName, description, requirements);

        private static BrickGovernanceRequirement Requirement(
            string id,
            BrickGovernanceArea area,
            string displayName,
            bool required,
            string rationale) =>
            new BrickGovernanceRequirement(id, area, displayName, required, rationale);
    }

    public sealed class BrickGovernanceAreaAssessment
    {
        public BrickGovernanceAreaAssessment(
            BrickGovernanceAreaDefinition definition,
            IEnumerable<BrickGovernanceRequirementResult> results)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Results = (results ?? Enumerable.Empty<BrickGovernanceRequirementResult>())
                .Where(result => result != null)
                .OrderBy(result => result.Requirement.Id, StringComparer.Ordinal)
                .ToArray();
            var requiredRequirements = Definition.Requirements.Where(requirement => requirement.Required).ToArray();
            RequiredRequirementCount = requiredRequirements.Length;
            SatisfiedRequiredRequirementCount = requiredRequirements.Count(IsSatisfied);
            MissingRequiredRequirementCount = RequiredRequirementCount - SatisfiedRequiredRequirementCount;
            ComplianceRatio = RequiredRequirementCount == 0
                ? 1d
                : (double)SatisfiedRequiredRequirementCount / RequiredRequirementCount;
            Status = ResolveStatus();
        }

        public BrickGovernanceAreaDefinition Definition { get; }
        public IReadOnlyList<BrickGovernanceRequirementResult> Results { get; }
        public int RequiredRequirementCount { get; }
        public int SatisfiedRequiredRequirementCount { get; }
        public int MissingRequiredRequirementCount { get; }
        public double ComplianceRatio { get; }
        public BrickGovernanceAreaStatus Status { get; }

        private bool IsSatisfied(BrickGovernanceRequirement requirement) =>
            Results.Any(result =>
                string.Equals(result.Requirement.Id, requirement.Id, StringComparison.Ordinal) &&
                result.SatisfiesRequirement);

        private BrickGovernanceAreaStatus ResolveStatus()
        {
            if (MissingRequiredRequirementCount == 0)
            {
                return BrickGovernanceAreaStatus.Compliant;
            }

            return SatisfiedRequiredRequirementCount > 0
                ? BrickGovernanceAreaStatus.Partial
                : BrickGovernanceAreaStatus.NonCompliant;
        }
    }

    public sealed class BrickGovernanceSummary
    {
        private BrickGovernanceSummary(
            int totalAreas,
            int compliantAreas,
            int partialAreas,
            int nonCompliantAreas,
            int missingRequiredRequirements)
        {
            TotalAreas = totalAreas;
            CompliantAreas = compliantAreas;
            PartialAreas = partialAreas;
            NonCompliantAreas = nonCompliantAreas;
            MissingRequiredRequirements = missingRequiredRequirements;
        }

        public int TotalAreas { get; }
        public int CompliantAreas { get; }
        public int PartialAreas { get; }
        public int NonCompliantAreas { get; }
        public int MissingRequiredRequirements { get; }
        public bool IsFullyCompliant => TotalAreas > 0 && MissingRequiredRequirements == 0;

        public static BrickGovernanceSummary FromAssessments(
            IEnumerable<BrickGovernanceAreaAssessment> assessments)
        {
            var items = (assessments ?? Enumerable.Empty<BrickGovernanceAreaAssessment>())
                .Where(assessment => assessment != null)
                .OrderBy(assessment => assessment.Definition.Area)
                .ToArray();
            return new BrickGovernanceSummary(
                items.Length,
                items.Count(assessment => assessment.Status == BrickGovernanceAreaStatus.Compliant),
                items.Count(assessment => assessment.Status == BrickGovernanceAreaStatus.Partial),
                items.Count(assessment => assessment.Status == BrickGovernanceAreaStatus.NonCompliant),
                items.Sum(assessment => assessment.MissingRequiredRequirementCount));
        }
    }

    public sealed class BrickGovernanceReport
    {
        public const string CurrentSchema = "NMolecules.Bricks.Governance/1.0";

        public BrickGovernanceReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickGovernanceAreaAssessment> assessments)
            : this(generatedAt, assessments, CurrentSchema)
        {
        }

        public BrickGovernanceReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickGovernanceAreaAssessment> assessments,
            string schema)
        {
            GeneratedAt = generatedAt;
            Assessments = (assessments ?? Enumerable.Empty<BrickGovernanceAreaAssessment>())
                .Where(assessment => assessment != null)
                .OrderBy(assessment => assessment.Definition.Area)
                .ToArray();
            Schema = schema ?? string.Empty;
            Summary = BrickGovernanceSummary.FromAssessments(Assessments);
        }

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickGovernanceAreaAssessment> Assessments { get; }
        public BrickGovernanceSummary Summary { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }

    public static class BrickGovernanceReportJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string Serialize(BrickGovernanceReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            return JsonSerializer.Serialize(ToDto(report), Options);
        }

        private static ReportDto ToDto(BrickGovernanceReport report) =>
            new ReportDto
            {
                Schema = report.Schema,
                GeneratedAt = report.GeneratedAt,
                Summary = new SummaryDto
                {
                    TotalAreas = report.Summary.TotalAreas,
                    CompliantAreas = report.Summary.CompliantAreas,
                    PartialAreas = report.Summary.PartialAreas,
                    NonCompliantAreas = report.Summary.NonCompliantAreas,
                    MissingRequiredRequirements = report.Summary.MissingRequiredRequirements,
                    IsFullyCompliant = report.Summary.IsFullyCompliant
                },
                Assessments = report.Assessments.Select(ToDto).ToArray()
            };

        private static AssessmentDto ToDto(BrickGovernanceAreaAssessment assessment) =>
            new AssessmentDto
            {
                Area = assessment.Definition.Area.ToString(),
                DisplayName = assessment.Definition.DisplayName,
                Description = assessment.Definition.Description,
                Status = assessment.Status.ToString(),
                RequiredRequirementCount = assessment.RequiredRequirementCount,
                SatisfiedRequiredRequirementCount = assessment.SatisfiedRequiredRequirementCount,
                MissingRequiredRequirementCount = assessment.MissingRequiredRequirementCount,
                ComplianceRatio = assessment.ComplianceRatio,
                Results = assessment.Results.Select(ToDto).ToArray()
            };

        private static ResultDto ToDto(BrickGovernanceRequirementResult result) =>
            new ResultDto
            {
                RequirementId = result.Requirement.Id,
                DisplayName = result.Requirement.DisplayName,
                Required = result.Requirement.Required,
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
            public int TotalAreas { get; set; }
            public int CompliantAreas { get; set; }
            public int PartialAreas { get; set; }
            public int NonCompliantAreas { get; set; }
            public int MissingRequiredRequirements { get; set; }
            public bool IsFullyCompliant { get; set; }
        }

        private sealed class AssessmentDto
        {
            public string Area { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
            public int RequiredRequirementCount { get; set; }
            public int SatisfiedRequiredRequirementCount { get; set; }
            public int MissingRequiredRequirementCount { get; set; }
            public double ComplianceRatio { get; set; }
            public ResultDto[] Results { get; set; }
        }

        private sealed class ResultDto
        {
            public string RequirementId { get; set; }
            public string DisplayName { get; set; }
            public bool Required { get; set; }
            public string Status { get; set; }
            public string Evidence { get; set; }
            public bool SatisfiesRequirement { get; set; }
        }
    }
}
