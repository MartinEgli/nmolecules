using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Serializes and deserializes governance report serializer documents using the stable Bricks JSON format.
/// </summary>
public static class BrickGovernanceReportJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes the Bricks document to its external JSON representation.
        /// </summary>
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
