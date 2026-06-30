using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents governance area assessment data used by governance readiness areas, requirements, and
/// summaries.
/// </summary>
public sealed class BrickGovernanceAreaAssessment
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickGovernanceAreaAssessment(
            BrickGovernanceAreaDefinition definition,
            IEnumerable<BrickGovernanceRequirementResult> results)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Results = (results ?? Enumerable.Empty<BrickGovernanceRequirementResult>())
                .Where(result => result != null)
                .OrderBy(result => result.Requirement.Id, StringComparer.Ordinal)
                .ToArray();
            var requiredRequirements = Definition.Requirements.Where(requirement => requirement.Required).ToArray();
            RequiredRequirementCount = requiredRequirements.Length;
            SatisfiedRequiredRequirementCount = requiredRequirements.Count(IsSatisfied);
            MissingRequiredRequirementCount = RequiredRequirementCount - SatisfiedRequiredRequirementCount;
            ComplianceRatio = RequiredRequirementCount == 0
                ? 1d
                : (double)SatisfiedRequiredRequirementCount / RequiredRequirementCount;
            Status = ResolveStatus();
        }

        /// <summary>
        /// Gets the Definition value used by Bricks developer tooling.
        /// </summary>
        public BrickGovernanceAreaDefinition Definition { get; }
        /// <summary>
        /// Gets the Results value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickGovernanceRequirementResult> Results { get; }
        /// <summary>
        /// Gets a value indicating whether Required Requirement Count applies.
        /// </summary>
        public int RequiredRequirementCount { get; }
        /// <summary>
        /// Gets the Satisfied Required Requirement Count value used by Bricks developer tooling.
        /// </summary>
        public int SatisfiedRequiredRequirementCount { get; }
        /// <summary>
        /// Gets the Missing Required Requirement Count value used by Bricks developer tooling.
        /// </summary>
        public int MissingRequiredRequirementCount { get; }
        /// <summary>
        /// Gets the Compliance Ratio value used by Bricks developer tooling.
        /// </summary>
        public double ComplianceRatio { get; }
        /// <summary>
        /// Gets the Status value used by Bricks developer tooling.
        /// </summary>
        public BrickGovernanceAreaStatus Status { get; }

        private bool IsSatisfied(BrickGovernanceRequirement requirement) =>
            Results.Any(result =>
                string.Equals(result.Requirement.Id, requirement.Id, StringComparison.Ordinal) &&
                result.SatisfiesRequirement);

        private BrickGovernanceAreaStatus ResolveStatus()
        {
            if (MissingRequiredRequirementCount == 0)
            {
                return BrickGovernanceAreaStatus.Compliant;
            }

            return SatisfiedRequiredRequirementCount > 0
                ? BrickGovernanceAreaStatus.Partial
                : BrickGovernanceAreaStatus.NonCompliant;
        }
    }
}
