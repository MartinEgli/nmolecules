using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Describes the outcome status used by governance readiness areas, requirements, and summaries.
/// </summary>
public enum BrickGovernanceAreaStatus
    {
        /// <summary>
        /// Gets the Compliant value used by Bricks developer tooling.
        /// </summary>
        Compliant = 0,
        /// <summary>
        /// Gets the Partial value used by Bricks developer tooling.
        /// </summary>
        Partial = 1,
        /// <summary>
        /// Gets the Non Compliant value used by Bricks developer tooling.
        /// </summary>
        NonCompliant = 2
    }
}
