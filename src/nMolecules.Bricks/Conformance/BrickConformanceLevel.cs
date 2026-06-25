using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents the conformance level level used to communicate Bricks maturity or confidence.
/// </summary>
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
