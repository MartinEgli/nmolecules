using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Captures a complete conformance report report for consumers, tools, and CI pipelines.
/// </summary>
public sealed class BrickConformanceReport
    {
        public const string CurrentSchema = "NMolecules.Bricks.Conformance/1.0";

        public BrickConformanceReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickConformanceLevelAssessment> assessments)
            : this(generatedAt, assessments, CurrentSchema)
        {
        }

        public BrickConformanceReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickConformanceLevelAssessment> assessments,
            string schema)
        {
            GeneratedAt = generatedAt;
            Assessments = (assessments ?? Enumerable.Empty<BrickConformanceLevelAssessment>())
                .Where(assessment => assessment != null)
                .OrderBy(assessment => assessment.Definition.Level)
                .ToArray();
            Schema = schema ?? string.Empty;
            Summary = BrickConformanceSummary.FromAssessments(Assessments);
        }

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickConformanceLevelAssessment> Assessments { get; }
        public BrickConformanceSummary Summary { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }
}
