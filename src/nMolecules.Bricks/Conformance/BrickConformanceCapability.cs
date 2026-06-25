using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents conformance capability data used by conformance capability checks and level assessments.
/// </summary>
public sealed class BrickConformanceCapability
    {
        public BrickConformanceCapability(
            string id,
            string displayName,
            bool required,
            string rationale = null)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Required = required;
            Rationale = rationale ?? string.Empty;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public bool Required { get; }
        public string Rationale { get; }
    }
}
