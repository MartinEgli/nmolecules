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
public enum BrickRoadmapStageStatus
    {
        /// <summary>
        /// Gets the Complete value used by Bricks developer tooling.
        /// </summary>
        Complete = 0,
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
