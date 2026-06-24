using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
public sealed class BrickRoadmapStageAssessment
    {
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

        public BrickRoadmapStageDefinition Definition { get; }
        public IReadOnlyList<BrickRoadmapItemResult> Results { get; }
        public int RequiredItemCount { get; }
        public int CompletedRequiredItemCount { get; }
        public int MissingRequiredItemCount { get; }
        public double CompletionRatio { get; }
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
