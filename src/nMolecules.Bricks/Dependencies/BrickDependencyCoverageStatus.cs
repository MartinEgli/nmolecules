using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
public enum BrickDependencyCoverageStatus
    {
        Covered = 0,
        PartiallyObservable = 1,
        NotObservable = 2,
        InsufficientEvidence = 3
    }
}
