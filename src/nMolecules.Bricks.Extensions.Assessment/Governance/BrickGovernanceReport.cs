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
        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public const string CurrentSchema = "NMolecules.Bricks.Governance/1.0";

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickGovernanceReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickGovernanceAreaAssessment> assessments)
            : this(generatedAt, assessments, CurrentSchema)
        {
        }

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
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

        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public string Schema { get; }
        /// <summary>
        /// Gets the timestamp associated with this Bricks model object.
        /// </summary>
        public DateTimeOffset GeneratedAt { get; }
        /// <summary>
        /// Gets the Assessments value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickGovernanceAreaAssessment> Assessments { get; }
        /// <summary>
        /// Gets the Summary value used by Bricks developer tooling.
        /// </summary>
        public BrickGovernanceSummary Summary { get; }
        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }
}
