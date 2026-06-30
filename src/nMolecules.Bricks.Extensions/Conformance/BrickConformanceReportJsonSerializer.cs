using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Serializes and deserializes conformance report serializer documents using the stable Bricks JSON format.
/// </summary>
public static class BrickConformanceReportJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes the Bricks document to its external JSON representation.
        /// </summary>
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
