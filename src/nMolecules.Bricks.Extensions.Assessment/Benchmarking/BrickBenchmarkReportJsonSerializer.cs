using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Serializes benchmark reports to the versioned JSON schema.
    /// </summary>
    public static class BrickBenchmarkReportJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes a benchmark report to compact camel-case JSON.
        /// </summary>
        public static string Serialize(BrickBenchmarkReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            return JsonSerializer.Serialize(ToDto(report), Options);
        }

        private static ReportDto ToDto(BrickBenchmarkReport report) =>
            new ReportDto
            {
                Schema = report.Schema,
                GeneratedAt = report.GeneratedAt,
                Summary = new SummaryDto
                {
                    Total = report.Summary.Total,
                    WithinBudget = report.Summary.WithinBudget,
                    OverBudget = report.Summary.OverBudget,
                    NotBudgeted = report.Summary.NotBudgeted
                },
                Results = report.Results.Select(ToDto).ToArray()
            };

        private static ResultDto ToDto(BrickBenchmarkResult result) =>
            new ResultDto
            {
                Id = result.Case.Id,
                DisplayName = result.Case.DisplayName,
                Subject = result.Case.Subject.ToString(),
                Iterations = result.Iterations,
                TotalOperations = result.TotalOperations,
                ElapsedTicks = result.Elapsed.Ticks,
                ElapsedPerOperationTicks = result.ElapsedPerOperation.Ticks,
                OperationsPerSecond = result.OperationsPerSecond,
                Status = result.Status.ToString(),
                BudgetMaxElapsedPerOperationTicks = result.Case.Budget == null || !result.Case.Budget.HasElapsedBudget
                    ? (long?)null
                    : result.Case.Budget.MaxElapsedPerOperation.Ticks,
                BudgetExceededByTicks = result.BudgetExceededBy.Ticks,
                BudgetRationale = result.Case.Budget == null ? null : result.Case.Budget.Rationale
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
            public int WithinBudget { get; set; }
            public int OverBudget { get; set; }
            public int NotBudgeted { get; set; }
        }

        private sealed class ResultDto
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string Subject { get; set; }
            public int Iterations { get; set; }
            public long TotalOperations { get; set; }
            public long ElapsedTicks { get; set; }
            public long ElapsedPerOperationTicks { get; set; }
            public double OperationsPerSecond { get; set; }
            public string Status { get; set; }
            public long? BudgetMaxElapsedPerOperationTicks { get; set; }
            public long BudgetExceededByTicks { get; set; }
            public string BudgetRationale { get; set; }
        }
    }
}
