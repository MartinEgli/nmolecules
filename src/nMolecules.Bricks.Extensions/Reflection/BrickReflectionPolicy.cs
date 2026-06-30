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
        public BrickReflectionPolicy(
            BrickReflectionConfidence minimumConfidence = BrickReflectionConfidence.Medium,
            bool requireJustification = true,
            bool enabled = true)
        {
            MinimumConfidence = minimumConfidence;
            RequireJustification = requireJustification;
            Enabled = enabled;
        }

        public BrickReflectionConfidence MinimumConfidence { get; }
        public bool RequireJustification { get; }
        public bool Enabled { get; }
    }
}
