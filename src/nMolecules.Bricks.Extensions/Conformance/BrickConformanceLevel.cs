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
        /// <summary>
        /// Gets the Marking value used by Bricks developer tooling.
        /// </summary>
        Marking = 0,
        /// <summary>
        /// Gets the Static Validation value used by Bricks developer tooling.
        /// </summary>
        StaticValidation = 1,
        /// <summary>
        /// Gets the Explainability value used by Bricks developer tooling.
        /// </summary>
        Explainability = 2,
        /// <summary>
        /// Gets the Policy Files value used by Bricks developer tooling.
        /// </summary>
        PolicyFiles = 3,
        /// <summary>
        /// Gets the Runtime Aware Analysis value used by Bricks developer tooling.
        /// </summary>
        RuntimeAwareAnalysis = 4,
        /// <summary>
        /// Gets the Integration And Augmentation value used by Bricks developer tooling.
        /// </summary>
        IntegrationAndAugmentation = 5
    }
}
