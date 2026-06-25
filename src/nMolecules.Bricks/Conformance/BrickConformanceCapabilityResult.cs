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

        public BrickConformanceCapability Capability { get; }
        public BrickConformanceCapabilityStatus Status { get; }
        public string Evidence { get; }
        public bool SatisfiesRequirement { get; }
    }
}
