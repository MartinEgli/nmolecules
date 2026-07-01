using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Summarizes roadmap summary results so callers can display the important outcome without reading every
/// detail.
/// </summary>
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

        /// <summary>
        /// Gets the Total Stages value used by Bricks developer tooling.
        /// </summary>
        public int TotalStages { get; }
        /// <summary>
        /// Gets the Complete Stages value used by Bricks developer tooling.
        /// </summary>
        public int CompleteStages { get; }
        /// <summary>
        /// Gets the Partial Stages value used by Bricks developer tooling.
        /// </summary>
        public int PartialStages { get; }
        /// <summary>
        /// Gets the Not Started Stages value used by Bricks developer tooling.
        /// </summary>
        public int NotStartedStages { get; }
        /// <summary>
        /// Gets the Missing Required Items value used by Bricks developer tooling.
        /// </summary>
        public int MissingRequiredItems { get; }
        /// <summary>
        /// Gets the Highest Contiguous Complete Stage value used by Bricks developer tooling.
        /// </summary>
        public BrickRoadmapStage? HighestContiguousCompleteStage { get; }

        /// <summary>
        /// Creates a Bricks configuration object from external key-value properties.
        /// </summary>
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
