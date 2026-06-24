using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
public enum BrickGovernanceRequirementStatus
    {
        Satisfied = 0,
        Missing = 1,
        NotApplicable = 2
    }
}
