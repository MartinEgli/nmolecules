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

        /// <summary>
        /// Gets the Total Areas value used by Bricks developer tooling.
        /// </summary>
        public int TotalAreas { get; }
        /// <summary>
        /// Gets the Compliant Areas value used by Bricks developer tooling.
        /// </summary>
        public int CompliantAreas { get; }
        /// <summary>
        /// Gets the Partial Areas value used by Bricks developer tooling.
        /// </summary>
        public int PartialAreas { get; }
        /// <summary>
        /// Gets the Non Compliant Areas value used by Bricks developer tooling.
        /// </summary>
        public int NonCompliantAreas { get; }
        /// <summary>
        /// Gets the Missing Required Requirements value used by Bricks developer tooling.
        /// </summary>
        public int MissingRequiredRequirements { get; }
        /// <summary>
        /// Gets a value indicating whether Is Fully Compliant applies.
        /// </summary>
        public bool IsFullyCompliant => TotalAreas > 0 && MissingRequiredRequirements == 0;

        /// <summary>
        /// Creates a Bricks configuration object from external key-value properties.
        /// </summary>
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
