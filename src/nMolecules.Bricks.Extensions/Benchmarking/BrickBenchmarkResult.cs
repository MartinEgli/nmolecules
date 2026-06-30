using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Captures the measured result of one benchmark case.
    /// </summary>
    public sealed class BrickBenchmarkResult
    {
        /// <summary>
        /// Creates a benchmark result from elapsed time and operation counts.
        /// </summary>
        public BrickBenchmarkResult(
            BrickBenchmarkCase benchmarkCase,
            int iterations,
            long totalOperations,
            TimeSpan elapsed)
        {
            Case = benchmarkCase ?? throw new ArgumentNullException(nameof(benchmarkCase));
            Iterations = iterations <= 0 ? 1 : iterations;
            TotalOperations = totalOperations <= 0 ? 1 : totalOperations;
            Elapsed = elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
            ElapsedPerOperation = TimeSpan.FromTicks(Elapsed.Ticks / TotalOperations);
            OperationsPerSecond = Elapsed.TotalSeconds > 0
                ? TotalOperations / Elapsed.TotalSeconds
                : double.PositiveInfinity;
            Status = ResolveStatus(Case.Budget, ElapsedPerOperation);
            BudgetExceededBy = Status == BrickBenchmarkStatus.OverBudget
                ? ElapsedPerOperation - Case.Budget.MaxElapsedPerOperation
                : TimeSpan.Zero;
        }

        /// <summary>Benchmark case that produced the result.</summary>
        public BrickBenchmarkCase Case { get; }
        /// <summary>Number of times the measured action was executed.</summary>
        public int Iterations { get; }
        /// <summary>Total number of logical operations measured across all iterations.</summary>
        public long TotalOperations { get; }
        /// <summary>Total elapsed time across all iterations.</summary>
        public TimeSpan Elapsed { get; }
        /// <summary>Average elapsed time per logical operation.</summary>
        public TimeSpan ElapsedPerOperation { get; }
        /// <summary>Calculated throughput in logical operations per second.</summary>
        public double OperationsPerSecond { get; }
        /// <summary>Budget status calculated from elapsed time per operation.</summary>
        public BrickBenchmarkStatus Status { get; }
        /// <summary>Indicates whether the result is within its configured budget.</summary>
        public bool IsWithinBudget => Status == BrickBenchmarkStatus.WithinBudget;
        /// <summary>Amount by which the elapsed time per operation exceeds the budget, or zero.</summary>
        public TimeSpan BudgetExceededBy { get; }

        private static BrickBenchmarkStatus ResolveStatus(
            BrickBenchmarkBudget budget,
            TimeSpan elapsedPerOperation)
        {
            if (budget == null || !budget.HasElapsedBudget)
            {
                return BrickBenchmarkStatus.NotBudgeted;
            }

            return elapsedPerOperation <= budget.MaxElapsedPerOperation
                ? BrickBenchmarkStatus.WithinBudget
                : BrickBenchmarkStatus.OverBudget;
        }
    }
}
