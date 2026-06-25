using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Enumerates the supported governance area values used by governance readiness areas, requirements, and
/// summaries.
/// </summary>
public enum BrickGovernanceArea
    {
        PolicyOwnership = 0,
        ExceptionHandling = 1,
        RolePackEvolution = 2,
        CompatibilityExpectations = 3
    }
}
