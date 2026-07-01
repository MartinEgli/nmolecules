using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents conformance level definition data used by conformance capability checks and level assessments.
/// </summary>
public sealed class BrickConformanceLevelDefinition
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickConformanceLevelDefinition(
            BrickConformanceLevel level,
            string displayName,
            string description,
            IEnumerable<BrickConformanceCapability> capabilities)
        {
            Level = level;
            DisplayName = displayName ?? string.Empty;
            Description = description ?? string.Empty;
            Capabilities = (capabilities ?? Enumerable.Empty<BrickConformanceCapability>())
                .Where(capability => capability != null)
                .OrderBy(capability => capability.Id, StringComparer.Ordinal)
                .ToArray();
        }

        /// <summary>
        /// Gets the Level value used by Bricks developer tooling.
        /// </summary>
        public BrickConformanceLevel Level { get; }
        /// <summary>
        /// Gets the Display Name value used by Bricks developer tooling.
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// Gets the Description value used by Bricks developer tooling.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Gets the Capabilities value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickConformanceCapability> Capabilities { get; }
    }
}
