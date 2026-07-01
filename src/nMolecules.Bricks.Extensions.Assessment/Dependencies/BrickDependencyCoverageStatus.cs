using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Describes the outcome status used by dependency coverage targets and coverage reporting.
/// </summary>
public enum BrickDependencyCoverageStatus
    {
        /// <summary>
        /// Gets the Covered value used by Bricks developer tooling.
        /// </summary>
        Covered = 0,
        /// <summary>
        /// Gets the Partially Observable value used by Bricks developer tooling.
        /// </summary>
        PartiallyObservable = 1,
        /// <summary>
        /// Gets the Not Observable value used by Bricks developer tooling.
        /// </summary>
        NotObservable = 2,
        /// <summary>
        /// Gets the Insufficient Evidence value used by Bricks developer tooling.
        /// </summary>
        InsufficientEvidence = 3
    }
}
