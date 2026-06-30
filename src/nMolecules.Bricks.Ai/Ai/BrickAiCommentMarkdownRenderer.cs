using System;
using System.Linq;
using System.Text;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Renders AI-ready violation comments as deterministic Markdown for IDE, PR or CI adapters.
    /// </summary>
    public static class BrickAiCommentMarkdownRenderer
    {
        public static string Render(BrickAiCommentDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var builder = new StringBuilder();
            builder.AppendLine("# Bricks AI Review Comments");
            builder.AppendLine();
            builder.AppendLine($"Schema: `{Escape(document.Schema)}`");
            builder.AppendLine($"Generated: `{document.GeneratedAt:O}`");
            builder.AppendLine();

            if (document.Comments.Count == 0)
            {
                builder.AppendLine("No deterministic Bricks violations were provided for AI explanation.");
                return builder.ToString();
            }

            foreach (var comment in document.Comments)
            {
                RenderComment(builder, comment);
            }

            return builder.ToString();
        }

        private static void RenderComment(StringBuilder builder, BrickAiViolationComment comment)
        {
            var ruleId = comment.RuleId?.Value ?? "unassigned-rule";
            builder.AppendLine($"## {Escape(ruleId)} - {Escape(comment.RuleName)}");
            builder.AppendLine();
            builder.AppendLine($"- Decision: `{comment.Decision}`");
            builder.AppendLine($"- Severity: `{comment.Severity}`");
            builder.AppendLine($"- Evidence: `{comment.EvidenceLevel}`");
            builder.AppendLine($"- Source: `{Escape(Display(comment.Violation.Source))}`");
            if (comment.Violation.Target != null)
            {
                builder.AppendLine($"- Target: `{Escape(Display(comment.Violation.Target))}`");
            }

            if (comment.Violation.DependencyKindId.HasValue || comment.Violation.DependencyLayer.HasValue)
            {
                builder.AppendLine($"- Dependency: `{Escape(Dependency(comment.Violation))}`");
            }

            builder.AppendLine();
            builder.AppendLine("### Problem");
            builder.AppendLine();
            builder.AppendLine(Escape(comment.ProblemSummary));
            builder.AppendLine();
            builder.AppendLine("### Why It Matters");
            builder.AppendLine();
            builder.AppendLine(Escape(comment.ArchitecturalReason));
            builder.AppendLine();

            if (comment.Options.Count > 0)
            {
                builder.AppendLine("### Remediation Options");
                builder.AppendLine();
                foreach (var option in comment.Options)
                {
                    var marker = comment.RecommendedOption != null && string.Equals(option.Id, comment.RecommendedOption.Id, StringComparison.Ordinal)
                        ? " recommended"
                        : string.Empty;
                    builder.AppendLine($"- `{Escape(option.Id)}`{marker}: {Escape(option.Description)} Risk: `{option.Risk}`");
                }

                builder.AppendLine();
            }

            if (comment.AiRepairHints.Count > 0)
            {
                builder.AppendLine("### AI Repair Hints");
                builder.AppendLine();
                foreach (var hint in comment.AiRepairHints)
                {
                    builder.AppendLine($"- {Escape(hint)}");
                }

                builder.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(comment.SuppressionGuidance))
            {
                builder.AppendLine("### Suppression Guidance");
                builder.AppendLine();
                builder.AppendLine(Escape(comment.SuppressionGuidance));
                builder.AppendLine();
            }
        }

        private static string Display(BrickElement element) =>
            string.IsNullOrWhiteSpace(element.FullName) ? element.DisplayName : element.FullName;

        private static string Dependency(BrickViolation violation)
        {
            var parts = new[]
            {
                violation.DependencyKindId?.Value,
                violation.DependencyLayer?.ToString()
            };

            return string.Join(" / ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string Escape(string value) =>
            (value ?? string.Empty).Replace("`", "'");
    }
}
