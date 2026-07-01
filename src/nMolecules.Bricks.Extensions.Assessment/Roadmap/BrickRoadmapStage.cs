using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Enumerates the supported roadmap stage values used by roadmap stages, readiness checks, and
/// implementation progress.
/// </summary>
public enum BrickRoadmapStage
    {
        /// <summary>
        /// Gets the v1 value used by Bricks developer tooling.
        /// </summary>
        V1 = 0,
        /// <summary>
        /// Gets the v1 1 value used by Bricks developer tooling.
        /// </summary>
        V1_1 = 1,
        /// <summary>
        /// Gets the v1 2 value used by Bricks developer tooling.
        /// </summary>
        V1_2 = 2,
        /// <summary>
        /// Gets the v2 value used by Bricks developer tooling.
        /// </summary>
        V2 = 3
    }
}
