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
        Completed = 0,
        Missing = 1,
        NotRequired = 2
    }
}
