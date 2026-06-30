using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Describes the outcome status used by conformance capability checks and level assessments.
/// </summary>
public enum BrickConformanceLevelStatus
    {
        /// <summary>
        /// Gets the Achieved value used by Bricks developer tooling.
        /// </summary>
        Achieved = 0,
        /// <summary>
        /// Gets the Partial value used by Bricks developer tooling.
        /// </summary>
        Partial = 1,
        /// <summary>
        /// Gets the Not Started value used by Bricks developer tooling.
        /// </summary>
        NotStarted = 2
    }
}
