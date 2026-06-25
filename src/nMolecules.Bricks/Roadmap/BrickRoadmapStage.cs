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
        V1 = 0,
        V1_1 = 1,
        V1_2 = 2,
        V2 = 3
    }
}
