using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
public sealed class BrickDependencyCoverageReport
    {
        public const string CurrentSchema = "NMolecules.Bricks.DependencyCoverage/1.0";

        public BrickDependencyCoverageReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickDependencyCoverageResult> results)
            : this(generatedAt, results, CurrentSchema)
        {
        }

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

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickDependencyCoverageResult> Results { get; }
        public BrickDependencyCoverageSummary Summary { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }
}
