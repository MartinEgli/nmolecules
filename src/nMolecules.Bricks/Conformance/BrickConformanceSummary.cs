using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
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

        public int TotalLevels { get; }
        public int AchievedLevels { get; }
        public int PartialLevels { get; }
        public int NotStartedLevels { get; }
        public int MissingRequiredCapabilities { get; }
        public BrickConformanceLevel? HighestContiguousAchievedLevel { get; }

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
