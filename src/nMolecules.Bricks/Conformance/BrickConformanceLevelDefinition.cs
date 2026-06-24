using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
public sealed class BrickConformanceLevelDefinition
    {
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

        public BrickConformanceLevel Level { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public IReadOnlyList<BrickConformanceCapability> Capabilities { get; }
    }
}
