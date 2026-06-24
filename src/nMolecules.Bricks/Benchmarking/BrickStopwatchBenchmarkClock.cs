using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Benchmark clock backed by <see cref="Stopwatch"/>.
    /// </summary>
    public sealed class BrickStopwatchBenchmarkClock : IBrickBenchmarkClock
    {
        /// <inheritdoc />
        public TimeSpan Measure(Action operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            var stopwatch = Stopwatch.StartNew();
            operation();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
