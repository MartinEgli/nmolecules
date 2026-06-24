using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
public sealed class BrickGovernanceRequirement
    {
        public BrickGovernanceRequirement(
            string id,
            BrickGovernanceArea area,
            string displayName,
            bool required,
            string rationale = null)
        {
            Id = id ?? string.Empty;
            Area = area;
            DisplayName = displayName ?? string.Empty;
            Required = required;
            Rationale = rationale ?? string.Empty;
        }

        public string Id { get; }
        public BrickGovernanceArea Area { get; }
        public string DisplayName { get; }
        public bool Required { get; }
        public string Rationale { get; }
    }
}
