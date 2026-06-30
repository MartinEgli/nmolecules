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
public enum BrickConformanceCapabilityStatus
    {
        /// <summary>
        /// Gets the Satisfied value used by Bricks developer tooling.
        /// </summary>
        Satisfied = 0,
        /// <summary>
        /// Gets the Missing value used by Bricks developer tooling.
        /// </summary>
        Missing = 1,
        /// <summary>
        /// Gets the Not Applicable value used by Bricks developer tooling.
        /// </summary>
        NotApplicable = 2
    }
}
