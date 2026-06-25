using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents governance area definition data used by governance readiness areas, requirements, and
/// summaries.
/// </summary>
public sealed class BrickGovernanceAreaDefinition
    {
        public BrickGovernanceAreaDefinition(
            BrickGovernanceArea area,
            string displayName,
            string description,
            IEnumerable<BrickGovernanceRequirement> requirements)
        {
            Area = area;
            DisplayName = displayName ?? string.Empty;
            Description = description ?? string.Empty;
            Requirements = (requirements ?? Enumerable.Empty<BrickGovernanceRequirement>())
                .Where(requirement => requirement != null)
                .OrderBy(requirement => requirement.Id, StringComparer.Ordinal)
                .ToArray();
        }

        public BrickGovernanceArea Area { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public IReadOnlyList<BrickGovernanceRequirement> Requirements { get; }
    }
}
