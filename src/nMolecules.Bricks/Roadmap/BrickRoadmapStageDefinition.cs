using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents roadmap stage definition data used by roadmap stages, readiness checks, and implementation
/// progress.
/// </summary>
public sealed class BrickRoadmapStageDefinition
    {
        public BrickRoadmapStageDefinition(
            BrickRoadmapStage stage,
            string displayName,
            string description,
            IEnumerable<BrickRoadmapItem> includedItems,
            IEnumerable<BrickRoadmapItem> excludedItems)
        {
            Stage = stage;
            DisplayName = displayName ?? string.Empty;
            Description = description ?? string.Empty;
            IncludedItems = NormalizeItems(includedItems);
            ExcludedItems = NormalizeItems(excludedItems);
        }

        public BrickRoadmapStage Stage { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public IReadOnlyList<BrickRoadmapItem> IncludedItems { get; }
        public IReadOnlyList<BrickRoadmapItem> ExcludedItems { get; }

        private static IReadOnlyList<BrickRoadmapItem> NormalizeItems(
            IEnumerable<BrickRoadmapItem> items) =>
            (items ?? Enumerable.Empty<BrickRoadmapItem>())
                .Where(item => item != null)
                .OrderBy(item => item.Id, StringComparer.Ordinal)
                .ToArray();
    }
}
