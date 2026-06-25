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
        Satisfied = 0,
        Missing = 1,
        NotApplicable = 2
    }
}
