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
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
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

        /// <summary>
        /// Gets the Area value used by Bricks developer tooling.
        /// </summary>
        public BrickGovernanceArea Area { get; }
        /// <summary>
        /// Gets the Display Name value used by Bricks developer tooling.
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// Gets the Description value used by Bricks developer tooling.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Gets a value indicating whether Requirements applies.
        /// </summary>
        public IReadOnlyList<BrickGovernanceRequirement> Requirements { get; }
    }
}
