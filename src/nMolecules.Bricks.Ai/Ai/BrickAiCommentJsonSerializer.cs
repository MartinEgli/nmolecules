using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Serializes AI-ready Bricks violation comments to the versioned JSON schema.
    /// </summary>
    public static class BrickAiCommentJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes a comment document to compact camel-case JSON.
        /// </summary>
        public static string Serialize(BrickAiCommentDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return JsonSerializer.Serialize(ToDto(document), Options);
        }

        private static DocumentDto ToDto(BrickAiCommentDocument document) =>
            new DocumentDto
            {
                Schema = document.Schema,
                GeneratedAt = document.GeneratedAt,
                Comments = document.Comments.Select(ToDto).ToArray()
            };

        private static CommentDto ToDto(BrickAiViolationComment comment) =>
            new CommentDto
            {
                RuleId = comment.RuleId?.Value,
                RuleName = comment.RuleName,
                Decision = comment.Decision.ToString(),
                Severity = comment.Severity.ToString(),
                EvidenceLevel = comment.EvidenceLevel.ToString(),
                Source = ToElementDto(comment.Violation.Source, comment.Violation.ResolvedSourceRoles),
                Target = comment.Violation.Target == null ? null : ToElementDto(comment.Violation.Target, comment.Violation.ResolvedTargetRoles),
                Dependency = ToDependencyDto(comment.Violation),
                Problem = comment.ProblemSummary,
                WhyItMatters = comment.ArchitecturalReason,
                RecommendedOption = comment.RecommendedOption == null ? null : comment.RecommendedOption.Id,
                RepairOptions = comment.Options.Select(ToDto).ToArray(),
                AiRepairHints = comment.AiRepairHints.ToArray(),
                SuppressionGuidance = comment.SuppressionGuidance
            };

        private static ElementDto ToElementDto(BrickElement element, IEnumerable<RoleId> roles) =>
            new ElementDto
            {
                Name = string.IsNullOrWhiteSpace(element.FullName) ? element.DisplayName : element.FullName,
                Roles = roles.Select(role => role.Value).ToArray()
            };

        private static DependencyDto ToDependencyDto(BrickViolation violation) =>
            violation.DependencyKindId.HasValue || violation.DependencyLayer.HasValue
                ? new DependencyDto
                {
                    Kind = violation.DependencyKindId?.Value,
                    Layer = violation.DependencyLayer?.ToString()
                }
                : null;

        private static RepairOptionDto ToDto(BrickRemediationOption option) =>
            new RepairOptionDto
            {
                Id = option.Id,
                Kind = option.Kind.ToString(),
                Description = option.Description,
                WhenToUse = option.WhenToUse,
                Risk = option.Risk.ToString(),
                Preferred = option.IsPreferred
            };

        private sealed class DocumentDto
        {
            public string Schema { get; set; }
            public DateTimeOffset GeneratedAt { get; set; }
            public CommentDto[] Comments { get; set; }
        }

        private sealed class CommentDto
        {
            public string RuleId { get; set; }
            public string RuleName { get; set; }
            public string Decision { get; set; }
            public string Severity { get; set; }
            public string EvidenceLevel { get; set; }
            public ElementDto Source { get; set; }
            public ElementDto Target { get; set; }
            public DependencyDto Dependency { get; set; }
            public string Problem { get; set; }
            public string WhyItMatters { get; set; }
            public string RecommendedOption { get; set; }
            public RepairOptionDto[] RepairOptions { get; set; }
            public string[] AiRepairHints { get; set; }
            public string SuppressionGuidance { get; set; }
        }

        private sealed class ElementDto
        {
            public string Name { get; set; }
            public string[] Roles { get; set; }
        }

        private sealed class DependencyDto
        {
            public string Kind { get; set; }
            public string Layer { get; set; }
        }

        private sealed class RepairOptionDto
        {
            public string Id { get; set; }
            public string Kind { get; set; }
            public string Description { get; set; }
            public string WhenToUse { get; set; }
            public string Risk { get; set; }
            public bool Preferred { get; set; }
        }
    }
}
