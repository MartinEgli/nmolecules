using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Versioned benchmark report for central Bricks benchmark results.
    /// </summary>
    public sealed class BrickBenchmarkReport
    {
        /// <summary>Current JSON schema identifier for benchmark reports.</summary>
        public const string CurrentSchema = "NMolecules.Bricks.Benchmark/1.0";

        /// <summary>
        /// Creates a benchmark report using the current schema.
        /// </summary>
        public BrickBenchmarkReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickBenchmarkResult> results)
            : this(generatedAt, results, CurrentSchema)
        {
        }

        /// <summary>
        /// Creates a benchmark report with an explicit schema identifier.
        /// </summary>
        public BrickBenchmarkReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickBenchmarkResult> results,
            string schema)
        {
            GeneratedAt = generatedAt;
            Results = (results ?? Enumerable.Empty<BrickBenchmarkResult>())
                .OrderBy(result => result.Case.Id, StringComparer.Ordinal)
                .ToArray();
            Schema = schema ?? string.Empty;
            Summary = BrickBenchmarkSummary.FromResults(Results);
        }

        /// <summary>Schema identifier used to serialize the report.</summary>
        public string Schema { get; }
        /// <summary>Time the report was generated.</summary>
        public DateTimeOffset GeneratedAt { get; }
        /// <summary>Benchmark results sorted by case identifier.</summary>
        public IReadOnlyList<BrickBenchmarkResult> Results { get; }
        /// <summary>Aggregate status counts for the report.</summary>
        public BrickBenchmarkSummary Summary { get; }
        /// <summary>Indicates whether the report uses <see cref="CurrentSchema"/>.</summary>
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }
}
