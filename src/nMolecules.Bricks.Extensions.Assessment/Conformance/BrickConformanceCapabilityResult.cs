using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents the result of conformance capability result processing in the Bricks pipeline.
/// </summary>
public sealed class BrickConformanceCapabilityResult
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickConformanceCapabilityResult(
            BrickConformanceCapability capability,
            BrickConformanceCapabilityStatus status,
            string evidence = null)
        {
            Capability = capability ?? throw new ArgumentNullException(nameof(capability));
            Status = status;
            Evidence = evidence ?? string.Empty;
            SatisfiesRequirement = !Capability.Required || Status == BrickConformanceCapabilityStatus.Satisfied;
        }

        /// <summary>
        /// Gets the Capability value used by Bricks developer tooling.
        /// </summary>
        public BrickConformanceCapability Capability { get; }
        /// <summary>
        /// Gets the Status value used by Bricks developer tooling.
        /// </summary>
        public BrickConformanceCapabilityStatus Status { get; }
        /// <summary>
        /// Gets the Evidence value used by Bricks developer tooling.
        /// </summary>
        public string Evidence { get; }
        /// <summary>
        /// Gets the Satisfies Requirement value used by Bricks developer tooling.
        /// </summary>
        public bool SatisfiesRequirement { get; }
    }
}
