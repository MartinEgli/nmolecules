using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes whether a benchmark result satisfies its configured budget.
    /// </summary>
    public enum BrickBenchmarkStatus
    {
        /// <summary>The measured elapsed time per operation is within the budget.</summary>
        WithinBudget = 0,
        /// <summary>The measured elapsed time per operation exceeds the budget.</summary>
        OverBudget = 1,
        /// <summary>No elapsed-time budget was configured for the benchmark case.</summary>
        NotBudgeted = 2
    }
}
