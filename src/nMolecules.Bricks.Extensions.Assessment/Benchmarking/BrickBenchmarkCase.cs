using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes a deterministic benchmark case for a central Bricks capability.
    /// </summary>
    public sealed class BrickBenchmarkCase
    {
        /// <summary>
        /// Creates a benchmark case.
        /// </summary>
        public BrickBenchmarkCase(
            string id,
            string displayName,
            BrickBenchmarkSubject subject,
            int operationCount,
            BrickBenchmarkBudget budget)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Subject = subject;
            OperationCount = operationCount <= 0 ? 1 : operationCount;
            Budget = budget;
        }

        /// <summary>Stable benchmark case identifier.</summary>
        public string Id { get; }
        /// <summary>Human-readable benchmark case name.</summary>
        public string DisplayName { get; }
        /// <summary>Central Bricks capability measured by the case.</summary>
        public BrickBenchmarkSubject Subject { get; }
        /// <summary>Number of logical operations performed by one invocation of the measured action.</summary>
        public int OperationCount { get; }
        /// <summary>Optional elapsed-time budget for the case.</summary>
        public BrickBenchmarkBudget Budget { get; }
    }
}
