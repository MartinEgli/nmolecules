using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Summarizes conformance summary results so callers can display the important outcome without reading every
/// detail.
/// </summary>
public sealed class BrickConformanceSummary
    {
        private BrickConformanceSummary(
            int totalLevels,
            int achievedLevels,
            int partialLevels,
            int notStartedLevels,
            int missingRequiredCapabilities,
            BrickConformanceLevel? highestContiguousAchievedLevel)
        {
            TotalLevels = totalLevels;
            AchievedLevels = achievedLevels;
            PartialLevels = partialLevels;
            NotStartedLevels = notStartedLevels;
            MissingRequiredCapabilities = missingRequiredCapabilities;
            HighestContiguousAchievedLevel = highestContiguousAchievedLevel;
        }

        /// <summary>
        /// Gets the Total Levels value used by Bricks developer tooling.
        /// </summary>
        public int TotalLevels { get; }
        /// <summary>
        /// Gets the Achieved Levels value used by Bricks developer tooling.
        /// </summary>
        public int AchievedLevels { get; }
        /// <summary>
        /// Gets the Partial Levels value used by Bricks developer tooling.
        /// </summary>
        public int PartialLevels { get; }
        /// <summary>
        /// Gets the Not Started Levels value used by Bricks developer tooling.
        /// </summary>
        public int NotStartedLevels { get; }
        /// <summary>
        /// Gets the Missing Required Capabilities value used by Bricks developer tooling.
        /// </summary>
        public int MissingRequiredCapabilities { get; }
        /// <summary>
        /// Gets the Highest Contiguous Achieved Level value used by Bricks developer tooling.
        /// </summary>
        public BrickConformanceLevel? HighestContiguousAchievedLevel { get; }

        /// <summary>
        /// Creates a Bricks configuration object from external key-value properties.
        /// </summary>
        public static BrickConformanceSummary FromAssessments(
            IEnumerable<BrickConformanceLevelAssessment> assessments)
        {
            var items = (assessments ?? Enumerable.Empty<BrickConformanceLevelAssessment>())
                .Where(assessment => assessment != null)
                .OrderBy(assessment => assessment.Definition.Level)
                .ToArray();

            return new BrickConformanceSummary(
                items.Length,
                items.Count(assessment => assessment.Status == BrickConformanceLevelStatus.Achieved),
                items.Count(assessment => assessment.Status == BrickConformanceLevelStatus.Partial),
                items.Count(assessment => assessment.Status == BrickConformanceLevelStatus.NotStarted),
                items.Sum(assessment => assessment.MissingRequiredCapabilityCount),
                ResolveHighestContiguous(items));
        }

        private static BrickConformanceLevel? ResolveHighestContiguous(
            IReadOnlyList<BrickConformanceLevelAssessment> assessments)
        {
            BrickConformanceLevel? highest = null;
            foreach (var expectedLevel in BrickBuiltInConformanceLevels.All.Select(level => level.Level))
            {
                var assessment = assessments.FirstOrDefault(item => item.Definition.Level == expectedLevel);
                if (assessment == null || assessment.Status != BrickConformanceLevelStatus.Achieved)
                {
                    break;
                }

                highest = expectedLevel;
            }

            return highest;
        }
    }
}
