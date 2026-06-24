using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
public static class BrickReportSarifSerializer
    {
        private const string UnknownRuleId = "NMolecules.Bricks.Unknown";

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

        private static SarifLogDto ToDto(BrickReportDocument report)
        {
            var violations = report.Violations
                .OrderBy(violation => violation.Source.Id.Value, StringComparer.Ordinal)
                .ThenBy(violation => violation.Target == null ? string.Empty : violation.Target.Id.Value, StringComparer.Ordinal)
                .ThenBy(RuleId, StringComparer.Ordinal)
                .ToArray();
            var rules = violations
                .GroupBy(RuleId, StringComparer.Ordinal)
                .Select(group => ToRuleDto(group.First()))
                .OrderBy(rule => rule.Id, StringComparer.Ordinal)
                .ToArray();

            return new SarifLogDto
            {
                Version = "2.1.0",
                Schema = "https://json.schemastore.org/sarif-2.1.0.json",
                Runs = new[]
                {
                    new RunDto
                    {
                        Tool = new ToolDto
                        {
                            Driver = new DriverDto
                            {
                                Name = "NMolecules.Bricks",
                                InformationUri = "https://github.com/xmolecules/nmolecules",
                                Rules = rules
                            }
                        },
                        Results = violations.Select(ToResultDto).ToArray()
                    }
                }
            };
        }

        private static ReportingDescriptorDto ToRuleDto(BrickViolation violation)
        {
            var ruleId = RuleId(violation);
            return new ReportingDescriptorDto
            {
                Id = ruleId,
                Name = string.IsNullOrWhiteSpace(violation.RuleName) ? ruleId : violation.RuleName,
                ShortDescription = new MessageDto
                {
                    Text = string.IsNullOrWhiteSpace(violation.RuleName) ? ruleId : violation.RuleName
                }
            };
        }

        private static ResultDto ToResultDto(BrickViolation violation) =>
            new ResultDto
            {
                RuleId = RuleId(violation),
                Level = Level(violation.Severity),
                Message = new MessageDto { Text = violation.Message },
                Locations = new[]
                {
                    new LocationDto
                    {
                        LogicalLocations = new[]
                        {
                            new LogicalLocationDto
                            {
                                FullyQualifiedName = LogicalName(violation.Source),
                                Kind = violation.Source.Kind.ToString()
                            }
                        }
                    }
                },
                Properties = new PropertiesDto
                {
                    Kind = violation.Kind.ToString(),
                    State = violation.State.ToString(),
                    EvidenceLevel = violation.EvidenceLevel.ToString(),
                    SourceId = violation.Source.Id.Value,
                    TargetId = violation.Target == null ? null : violation.Target.Id.Value,
                    DependencyKindId = violation.DependencyKindId.HasValue ? violation.DependencyKindId.Value.Value : null,
                    DependencyLayer = violation.DependencyLayer.HasValue ? violation.DependencyLayer.Value.ToString() : null,
                    StateReason = violation.StateReason
                }
            };

        private static string RuleId(BrickViolation violation) =>
            violation.RuleId.HasValue && !violation.RuleId.Value.IsEmpty
                ? violation.RuleId.Value.Value
                : UnknownRuleId;

        private static string Level(BrickSeverity severity)
        {
            switch (severity)
            {
                case BrickSeverity.Error:
                    return "error";
                case BrickSeverity.Warning:
                    return "warning";
                default:
                    return "note";
            }
        }

        private static string LogicalName(BrickElement element) =>
            !string.IsNullOrWhiteSpace(element.FullName)
                ? element.FullName
                : element.DisplayName;

        private sealed class SarifLogDto
        {
            [JsonPropertyName("$schema")]
            public string Schema { get; set; }

            public string Version { get; set; }
            public RunDto[] Runs { get; set; }
        }

        private sealed class RunDto
        {
            public ToolDto Tool { get; set; }
            public ResultDto[] Results { get; set; }
        }

        private sealed class ToolDto
        {
            public DriverDto Driver { get; set; }
        }

        private sealed class DriverDto
        {
            public string Name { get; set; }
            public string InformationUri { get; set; }
            public ReportingDescriptorDto[] Rules { get; set; }
        }

        private sealed class ReportingDescriptorDto
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public MessageDto ShortDescription { get; set; }
        }

        private sealed class ResultDto
        {
            public string RuleId { get; set; }
            public string Level { get; set; }
            public MessageDto Message { get; set; }
            public LocationDto[] Locations { get; set; }
            public PropertiesDto Properties { get; set; }
        }

        private sealed class MessageDto
        {
            public string Text { get; set; }
        }

        private sealed class LocationDto
        {
            public LogicalLocationDto[] LogicalLocations { get; set; }
        }

        private sealed class LogicalLocationDto
        {
            public string FullyQualifiedName { get; set; }
            public string Kind { get; set; }
        }

        private sealed class PropertiesDto
        {
            public string Kind { get; set; }
            public string State { get; set; }
            public string EvidenceLevel { get; set; }
            public string SourceId { get; set; }
            public string TargetId { get; set; }
            public string DependencyKindId { get; set; }
            public string DependencyLayer { get; set; }
            public string StateReason { get; set; }
        }
    }
}
