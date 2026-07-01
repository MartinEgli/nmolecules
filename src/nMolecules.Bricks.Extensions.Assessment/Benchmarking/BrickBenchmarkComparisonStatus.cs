using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes how a current benchmark result compares with a previous baseline.
    /// </summary>
    public enum BrickBenchmarkComparisonStatus
    {
        /// <summary>The current result remains within the configured comparison threshold.</summary>
        Stable = 0,
        /// <summary>The current result is meaningfully faster than the baseline.</summary>
        Improved = 1,
        /// <summary>The current result is slower than the baseline by more than the allowed threshold.</summary>
        Regressed = 2,
        /// <summary>No matching baseline result exists for the current benchmark case.</summary>
        NoBaseline = 3
    }
}
