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
public enum BrickGovernanceRequirementStatus
    {
        Satisfied = 0,
        Missing = 1,
        NotApplicable = 2
    }
}
