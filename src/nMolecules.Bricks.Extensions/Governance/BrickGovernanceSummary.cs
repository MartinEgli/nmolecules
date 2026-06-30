using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Summarizes governance summary results so callers can display the important outcome without reading every
/// detail.
/// </summary>
public sealed class BrickGovernanceSummary
    {
        private BrickGovernanceSummary(
            int totalAreas,
            int compliantAreas,
            int partialAreas,
            int nonCompliantAreas,
            int missingRequiredRequirements)
        {
            TotalAreas = totalAreas;
            CompliantAreas = compliantAreas;
            PartialAreas = partialAreas;
            NonCompliantAreas = nonCompliantAreas;
            MissingRequiredRequirements = missingRequiredRequirements;
        }

        public int TotalAreas { get; }
        public int CompliantAreas { get; }
        public int PartialAreas { get; }
        public int NonCompliantAreas { get; }
        public int MissingRequiredRequirements { get; }
        public bool IsFullyCompliant => TotalAreas > 0 && MissingRequiredRequirements == 0;

        public static BrickGovernanceSummary FromAssessments(
            IEnumerable<BrickGovernanceAreaAssessment> assessments)
        {
            var items = (assessments ?? Enumerable.Empty<BrickGovernanceAreaAssessment>())
                .Where(assessment => assessment != null)
                .OrderBy(assessment => assessment.Definition.Area)
                .ToArray();
            return new BrickGovernanceSummary(
                items.Length,
                items.Count(assessment => assessment.Status == BrickGovernanceAreaStatus.Compliant),
                items.Count(assessment => assessment.Status == BrickGovernanceAreaStatus.Partial),
                items.Count(assessment => assessment.Status == BrickGovernanceAreaStatus.NonCompliant),
                items.Sum(assessment => assessment.MissingRequiredRequirementCount));
        }
    }
}
