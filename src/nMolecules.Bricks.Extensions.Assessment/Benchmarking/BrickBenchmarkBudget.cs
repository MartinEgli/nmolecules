using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Defines the expected maximum elapsed time per operation for a benchmark case.
    /// </summary>
    public sealed class BrickBenchmarkBudget
    {
        /// <summary>
        /// Creates a benchmark budget.
        /// </summary>
        /// <param name="maxElapsedPerOperation">Maximum allowed elapsed time per measured operation.</param>
        /// <param name="rationale">Optional explanation for the budget.</param>
        public BrickBenchmarkBudget(TimeSpan maxElapsedPerOperation, string rationale = null)
        {
            MaxElapsedPerOperation = maxElapsedPerOperation < TimeSpan.Zero ? TimeSpan.Zero : maxElapsedPerOperation;
            Rationale = rationale ?? string.Empty;
        }

        /// <summary>Maximum allowed elapsed time per measured operation.</summary>
        public TimeSpan MaxElapsedPerOperation { get; }
        /// <summary>Explanation for why this budget exists.</summary>
        public string Rationale { get; }
        /// <summary>Indicates whether this budget contains a positive elapsed-time limit.</summary>
        public bool HasElapsedBudget => MaxElapsedPerOperation > TimeSpan.Zero;
    }
}
