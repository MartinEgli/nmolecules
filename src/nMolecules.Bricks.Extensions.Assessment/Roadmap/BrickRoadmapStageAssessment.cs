using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents roadmap stage assessment data used by roadmap stages, readiness checks, and implementation
/// progress.
/// </summary>
public sealed class BrickRoadmapStageAssessment
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickRoadmapStageAssessment(
            BrickRoadmapStageDefinition definition,
            IEnumerable<BrickRoadmapItemResult> results)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Results = (results ?? Enumerable.Empty<BrickRoadmapItemResult>())
                .Where(result => result != null)
                .OrderBy(result => result.Item.Id, StringComparer.Ordinal)
                .ToArray();

            var requiredItems = Definition.IncludedItems.Where(item => item.Required).ToArray();
            RequiredItemCount = requiredItems.Length;
            CompletedRequiredItemCount = requiredItems.Count(IsCompleted);
            MissingRequiredItemCount = RequiredItemCount - CompletedRequiredItemCount;
            CompletionRatio = RequiredItemCount == 0
                ? 1d
                : (double)CompletedRequiredItemCount / RequiredItemCount;
            Status = ResolveStatus();
        }

        /// <summary>
        /// Gets the Definition value used by Bricks developer tooling.
        /// </summary>
        public BrickRoadmapStageDefinition Definition { get; }
        /// <summary>
        /// Gets the Results value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickRoadmapItemResult> Results { get; }
        /// <summary>
        /// Gets a value indicating whether Required Item Count applies.
        /// </summary>
        public int RequiredItemCount { get; }
        /// <summary>
        /// Gets the Completed Required Item Count value used by Bricks developer tooling.
        /// </summary>
        public int CompletedRequiredItemCount { get; }
        /// <summary>
        /// Gets the Missing Required Item Count value used by Bricks developer tooling.
        /// </summary>
        public int MissingRequiredItemCount { get; }
        /// <summary>
        /// Gets the Completion Ratio value used by Bricks developer tooling.
        /// </summary>
        public double CompletionRatio { get; }
        /// <summary>
        /// Gets the Status value used by Bricks developer tooling.
        /// </summary>
        public BrickRoadmapStageStatus Status { get; }

        private bool IsCompleted(BrickRoadmapItem item) =>
            Results.Any(result =>
                string.Equals(result.Item.Id, item.Id, StringComparison.Ordinal) &&
                result.SatisfiesRequirement);

        private BrickRoadmapStageStatus ResolveStatus()
        {
            if (MissingRequiredItemCount == 0)
            {
                return BrickRoadmapStageStatus.Complete;
            }

            return CompletedRequiredItemCount > 0
                ? BrickRoadmapStageStatus.Partial
                : BrickRoadmapStageStatus.NotStarted;
        }
    }
}
