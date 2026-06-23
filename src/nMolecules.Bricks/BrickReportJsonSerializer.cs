using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
    public static class BrickReportJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        public static string Serialize(BrickReportDocument report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            return JsonSerializer.Serialize(ToDto(report), Options);
        }

        private static ReportDto ToDto(BrickReportDocument report) =>
            new ReportDto
            {
                Schema = report.Schema,
                GeneratedAt = report.GeneratedAt,
                Summary = new SummaryDto
                {
                    Total = report.Summary.Total,
                    Active = report.Summary.Active,
                    Suppressed = report.Summary.Suppressed,
                    Baselined = report.Summary.Baselined,
                    ExpiredSuppressions = report.Summary.ExpiredSuppressions,
                    ExpiredBaselineEntries = report.Summary.ExpiredBaselineEntries
                },
                Violations = report.Violations
                    .Select(ToDto)
                    .OrderBy(violation => violation.Source.Id, StringComparer.Ordinal)
                    .ThenBy(violation => violation.Target == null ? string.Empty : violation.Target.Id, StringComparer.Ordinal)
                    .ThenBy(violation => violation.RuleId ?? string.Empty, StringComparer.Ordinal)
                    .ToArray()
            };

        private static ViolationDto ToDto(BrickViolation violation) =>
            new ViolationDto
            {
                Kind = violation.Kind.ToString(),
                RuleId = violation.RuleId.HasValue ? violation.RuleId.Value.Value : null,
                RuleName = violation.RuleName,
                Source = ToDto(violation.Source),
                Target = violation.Target == null ? null : ToDto(violation.Target),
                ResolvedSourceRoles = violation.ResolvedSourceRoles.Select(role => role.Value).OrderBy(role => role, StringComparer.Ordinal).ToArray(),
                ResolvedTargetRoles = violation.ResolvedTargetRoles.Select(role => role.Value).OrderBy(role => role, StringComparer.Ordinal).ToArray(),
                DependencyKindId = violation.DependencyKindId.HasValue ? violation.DependencyKindId.Value.Value : null,
                Scope = violation.Scope.ToString(),
                DependencyLayer = violation.DependencyLayer.HasValue ? violation.DependencyLayer.Value.ToString() : null,
                Severity = violation.Severity.ToString(),
                Message = violation.Message,
                EvidenceLevel = violation.EvidenceLevel.ToString(),
                State = violation.State.ToString(),
                StateReason = violation.StateReason
            };

        private static ElementDto ToDto(BrickElement element) =>
            new ElementDto
            {
                Id = element.Id.Value,
                Kind = element.Kind.ToString(),
                DisplayName = element.DisplayName,
                AssemblyName = element.AssemblyName,
                NamespaceName = element.NamespaceName,
                FullName = element.FullName,
                Origin = element.Origin.ToString(),
                Source = element.Source.ToString()
            };

        private sealed class ReportDto
        {
            public string Schema { get; set; }
            public DateTimeOffset GeneratedAt { get; set; }
            public SummaryDto Summary { get; set; }
            public ViolationDto[] Violations { get; set; }
        }

        private sealed class SummaryDto
        {
            public int Total { get; set; }
            public int Active { get; set; }
            public int Suppressed { get; set; }
            public int Baselined { get; set; }
            public int ExpiredSuppressions { get; set; }
            public int ExpiredBaselineEntries { get; set; }
        }

        private sealed class ViolationDto
        {
            public string Kind { get; set; }
            public string RuleId { get; set; }
            public string RuleName { get; set; }
            public ElementDto Source { get; set; }
            public ElementDto Target { get; set; }
            public string[] ResolvedSourceRoles { get; set; }
            public string[] ResolvedTargetRoles { get; set; }
            public string DependencyKindId { get; set; }
            public string Scope { get; set; }
            public string DependencyLayer { get; set; }
            public string Severity { get; set; }
            public string Message { get; set; }
            public string EvidenceLevel { get; set; }
            public string State { get; set; }
            public string StateReason { get; set; }
        }

        private sealed class ElementDto
        {
            public string Id { get; set; }
            public string Kind { get; set; }
            public string DisplayName { get; set; }
            public string AssemblyName { get; set; }
            public string NamespaceName { get; set; }
            public string FullName { get; set; }
            public string Origin { get; set; }
            public string Source { get; set; }
        }
    }
}
