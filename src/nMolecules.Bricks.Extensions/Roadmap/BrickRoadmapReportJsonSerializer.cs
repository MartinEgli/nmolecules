using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Serializes and deserializes roadmap report serializer documents using the stable Bricks JSON format.
/// </summary>
public static class BrickRoadmapReportJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes the Bricks document to its external JSON representation.
        /// </summary>
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
