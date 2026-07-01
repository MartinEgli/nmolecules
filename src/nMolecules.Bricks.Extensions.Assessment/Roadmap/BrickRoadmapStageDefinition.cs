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
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
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

        /// <summary>
        /// Gets the Stage value used by Bricks developer tooling.
        /// </summary>
        public BrickRoadmapStage Stage { get; }
        /// <summary>
        /// Gets the Display Name value used by Bricks developer tooling.
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// Gets the Description value used by Bricks developer tooling.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Gets the Included Items value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickRoadmapItem> IncludedItems { get; }
        /// <summary>
        /// Gets the Excluded Items value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickRoadmapItem> ExcludedItems { get; }

        private static IReadOnlyList<BrickRoadmapItem> NormalizeItems(
            IEnumerable<BrickRoadmapItem> items) =>
            (items ?? Enumerable.Empty<BrickRoadmapItem>())
                .Where(item => item != null)
                .OrderBy(item => item.Id, StringComparer.Ordinal)
                .ToArray();
    }
}
