using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Measures the elapsed time of a benchmark operation.
    /// </summary>
    public interface IBrickBenchmarkClock
    {
        /// <summary>
        /// Measures the elapsed time required to execute <paramref name="operation"/>.
        /// </summary>
        TimeSpan Measure(Action operation);
    }
}
