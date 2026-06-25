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
        Achieved = 0,
        Partial = 1,
        NotStarted = 2
    }
}
