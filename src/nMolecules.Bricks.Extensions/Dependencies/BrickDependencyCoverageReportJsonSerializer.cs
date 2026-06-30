using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Serializes and deserializes dependency coverage report serializer documents using the stable Bricks JSON
/// format.
/// </summary>
public static class BrickDependencyCoverageReportJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string Serialize(BrickDependencyCoverageReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            return JsonSerializer.Serialize(ToDto(report), Options);
        }

        private static ReportDto ToDto(BrickDependencyCoverageReport report) =>
            new ReportDto
            {
                Schema = report.Schema,
                GeneratedAt = report.GeneratedAt,
                Summary = new SummaryDto
                {
                    Total = report.Summary.Total,
                    Covered = report.Summary.Covered,
                    PartiallyObservable = report.Summary.PartiallyObservable,
                    NotObservable = report.Summary.NotObservable,
                    InsufficientEvidence = report.Summary.InsufficientEvidence,
                    AverageCoverageRatio = report.Summary.AverageCoverageRatio
                },
                Results = report.Results.Select(ToDto).ToArray()
            };

        private static ResultDto ToDto(BrickDependencyCoverageResult result) =>
            new ResultDto
            {
                KindId = result.Target.KindId.Value,
                Layer = result.Target.Layer.ToString(),
                MinimumEvidenceLevel = result.Target.MinimumEvidenceLevel.ToString(),
                Required = result.Target.Required,
                Rationale = result.Target.Rationale,
                AnalyzedDependencies = result.AnalyzedDependencies,
                ObservableDependencies = result.ObservableDependencies,
                UnobservableDependencies = result.UnobservableDependencies,
                CoverageRatio = result.CoverageRatio,
                ObservedEvidenceLevel = result.ObservedEvidenceLevel.ToString(),
                MeetsEvidenceRequirement = result.MeetsEvidenceRequirement,
                Status = result.Status.ToString(),
                Notes = string.IsNullOrEmpty(result.Notes) ? null : result.Notes
            };

        private sealed class ReportDto
        {
            public string Schema { get; set; }
            public DateTimeOffset GeneratedAt { get; set; }
            public SummaryDto Summary { get; set; }
            public ResultDto[] Results { get; set; }
        }

        private sealed class SummaryDto
        {
            public int Total { get; set; }
            public int Covered { get; set; }
            public int PartiallyObservable { get; set; }
            public int NotObservable { get; set; }
            public int InsufficientEvidence { get; set; }
            public double AverageCoverageRatio { get; set; }
        }

        private sealed class ResultDto
        {
            public string KindId { get; set; }
            public string Layer { get; set; }
            public string MinimumEvidenceLevel { get; set; }
            public bool Required { get; set; }
            public string Rationale { get; set; }
            public int AnalyzedDependencies { get; set; }
            public int ObservableDependencies { get; set; }
            public int UnobservableDependencies { get; set; }
            public double CoverageRatio { get; set; }
            public string ObservedEvidenceLevel { get; set; }
            public bool MeetsEvidenceRequirement { get; set; }
            public string Status { get; set; }
            public string Notes { get; set; }
        }
    }
}
