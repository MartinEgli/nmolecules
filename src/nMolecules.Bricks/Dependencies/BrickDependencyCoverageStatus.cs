using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Describes the outcome status used by dependency coverage targets and coverage reporting.
/// </summary>
public enum BrickDependencyCoverageStatus
    {
        Covered = 0,
        PartiallyObservable = 1,
        NotObservable = 2,
        InsufficientEvidence = 3
    }
}
