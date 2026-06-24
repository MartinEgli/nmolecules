using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
public sealed class BrickRoadmapReport
    {
        public const string CurrentSchema = "NMolecules.Bricks.Roadmap/1.0";

        public BrickRoadmapReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickRoadmapStageAssessment> assessments)
            : this(generatedAt, assessments, CurrentSchema)
        {
        }

        public BrickRoadmapReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickRoadmapStageAssessment> assessments,
            string schema)
        {
            GeneratedAt = generatedAt;
            Assessments = (assessments ?? Enumerable.Empty<BrickRoadmapStageAssessment>())
                .Where(assessment => assessment != null)
                .OrderBy(assessment => assessment.Definition.Stage)
                .ToArray();
            Schema = schema ?? string.Empty;
            Summary = BrickRoadmapSummary.FromAssessments(Assessments);
        }

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickRoadmapStageAssessment> Assessments { get; }
        public BrickRoadmapSummary Summary { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }
}
