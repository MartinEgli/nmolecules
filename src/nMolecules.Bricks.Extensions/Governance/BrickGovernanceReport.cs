using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Captures a complete governance report report for consumers, tools, and CI pipelines.
/// </summary>
public sealed class BrickGovernanceReport
    {
        public const string CurrentSchema = "NMolecules.Bricks.Governance/1.0";

        public BrickGovernanceReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickGovernanceAreaAssessment> assessments)
            : this(generatedAt, assessments, CurrentSchema)
        {
        }

        public BrickGovernanceReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickGovernanceAreaAssessment> assessments,
            string schema)
        {
            GeneratedAt = generatedAt;
            Assessments = (assessments ?? Enumerable.Empty<BrickGovernanceAreaAssessment>())
                .Where(assessment => assessment != null)
                .OrderBy(assessment => assessment.Definition.Area)
                .ToArray();
            Schema = schema ?? string.Empty;
            Summary = BrickGovernanceSummary.FromAssessments(Assessments);
        }

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickGovernanceAreaAssessment> Assessments { get; }
        public BrickGovernanceSummary Summary { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }
}
