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
        Complete = 0,
        Partial = 1,
        NotStarted = 2
    }
}
