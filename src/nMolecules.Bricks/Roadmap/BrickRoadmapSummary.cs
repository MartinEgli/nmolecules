using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
public sealed class BrickRoadmapSummary
    {
        private BrickRoadmapSummary(
            int totalStages,
            int completeStages,
            int partialStages,
            int notStartedStages,
            int missingRequiredItems,
            BrickRoadmapStage? highestContiguousCompleteStage)
        {
            TotalStages = totalStages;
            CompleteStages = completeStages;
            PartialStages = partialStages;
            NotStartedStages = notStartedStages;
            MissingRequiredItems = missingRequiredItems;
            HighestContiguousCompleteStage = highestContiguousCompleteStage;
        }

        public int TotalStages { get; }
        public int CompleteStages { get; }
        public int PartialStages { get; }
        public int NotStartedStages { get; }
        public int MissingRequiredItems { get; }
        public BrickRoadmapStage? HighestContiguousCompleteStage { get; }

        public static BrickRoadmapSummary FromAssessments(
            IEnumerable<BrickRoadmapStageAssessment> assessments)
        {
            var items = (assessments ?? Enumerable.Empty<BrickRoadmapStageAssessment>())
                .Where(assessment => assessment != null)
                .OrderBy(assessment => assessment.Definition.Stage)
                .ToArray();

            return new BrickRoadmapSummary(
                items.Length,
                items.Count(assessment => assessment.Status == BrickRoadmapStageStatus.Complete),
                items.Count(assessment => assessment.Status == BrickRoadmapStageStatus.Partial),
                items.Count(assessment => assessment.Status == BrickRoadmapStageStatus.NotStarted),
                items.Sum(assessment => assessment.MissingRequiredItemCount),
                ResolveHighestContiguous(items));
        }

        private static BrickRoadmapStage? ResolveHighestContiguous(
            IReadOnlyList<BrickRoadmapStageAssessment> assessments)
        {
            BrickRoadmapStage? highest = null;
            foreach (var expectedStage in BrickBuiltInRoadmapStages.All.Select(stage => stage.Stage))
            {
                var assessment = assessments.FirstOrDefault(item => item.Definition.Stage == expectedStage);
                if (assessment == null || assessment.Status != BrickRoadmapStageStatus.Complete)
                {
                    break;
                }

                highest = expectedStage;
            }

            return highest;
        }
    }
}
