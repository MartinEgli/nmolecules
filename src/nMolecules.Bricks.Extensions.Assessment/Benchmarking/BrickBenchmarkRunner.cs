using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Executes benchmark cases against measured operations.
    /// </summary>
    public static class BrickBenchmarkRunner
    {
        /// <summary>
        /// Runs a benchmark case for the requested number of iterations.
        /// </summary>
        public static BrickBenchmarkResult Run(
            BrickBenchmarkCase benchmarkCase,
            Action operation,
            int iterations = 1,
            IBrickBenchmarkClock clock = null)
        {
            if (benchmarkCase == null)
            {
                throw new ArgumentNullException(nameof(benchmarkCase));
            }

            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (iterations <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations));
            }

            var activeClock = clock ?? new BrickStopwatchBenchmarkClock();
            var elapsed = TimeSpan.Zero;
            for (var i = 0; i < iterations; i++)
            {
                elapsed += activeClock.Measure(operation);
            }

            return new BrickBenchmarkResult(
                benchmarkCase,
                iterations,
                (long)benchmarkCase.OperationCount * iterations,
                elapsed);
        }
    }
}
