using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
public enum BrickConformanceLevel
    {
        Marking = 0,
        StaticValidation = 1,
        Explainability = 2,
        PolicyFiles = 3,
        RuntimeAwareAnalysis = 4,
        IntegrationAndAugmentation = 5
    }
}
