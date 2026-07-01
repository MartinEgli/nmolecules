using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Describes the outcome status used by roadmap stages, readiness checks, and implementation progress.
/// </summary>
public enum BrickRoadmapItemStatus
    {
        /// <summary>
        /// Gets the Completed value used by Bricks developer tooling.
        /// </summary>
        Completed = 0,
        /// <summary>
        /// Gets the Missing value used by Bricks developer tooling.
        /// </summary>
        Missing = 1,
        /// <summary>
        /// Gets the Not Required value used by Bricks developer tooling.
        /// </summary>
        NotRequired = 2
    }
}
