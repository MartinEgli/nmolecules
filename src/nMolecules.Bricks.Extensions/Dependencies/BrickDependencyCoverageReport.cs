using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Captures a complete dependency coverage report report for consumers, tools, and CI pipelines.
/// </summary>
public sealed class BrickDependencyCoverageReport
    {
        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public const string CurrentSchema = "NMolecules.Bricks.DependencyCoverage/1.0";

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickDependencyCoverageReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickDependencyCoverageResult> results)
            : this(generatedAt, results, CurrentSchema)
        {
        }

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickDependencyCoverageReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickDependencyCoverageResult> results,
            string schema)
        {
            GeneratedAt = generatedAt;
            Results = (results ?? Enumerable.Empty<BrickDependencyCoverageResult>())
                .OrderBy(result => result.Target.KindId.Value, StringComparer.Ordinal)
                .ToArray();
            Schema = schema ?? string.Empty;
            Summary = BrickDependencyCoverageSummary.FromResults(Results);
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
        /// Gets the Results value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickDependencyCoverageResult> Results { get; }
        /// <summary>
        /// Gets the Summary value used by Bricks developer tooling.
        /// </summary>
        public BrickDependencyCoverageSummary Summary { get; }
        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }
}
