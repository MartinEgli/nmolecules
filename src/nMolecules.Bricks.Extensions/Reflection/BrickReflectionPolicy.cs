using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Defines the policy data used to govern reflection-based architecture assessment.
/// </summary>
public sealed class BrickReflectionPolicy
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickReflectionPolicy(
            BrickReflectionConfidence minimumConfidence = BrickReflectionConfidence.Medium,
            bool requireJustification = true,
            bool enabled = true)
        {
            MinimumConfidence = minimumConfidence;
            RequireJustification = requireJustification;
            Enabled = enabled;
        }

        /// <summary>
        /// Gets the Minimum Confidence value used by Bricks developer tooling.
        /// </summary>
        public BrickReflectionConfidence MinimumConfidence { get; }
        /// <summary>
        /// Gets a value indicating whether Require Justification applies.
        /// </summary>
        public bool RequireJustification { get; }
        /// <summary>
        /// Gets a value indicating whether Enabled applies.
        /// </summary>
        public bool Enabled { get; }
    }
}
