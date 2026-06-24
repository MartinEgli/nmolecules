using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
public sealed class BrickGovernanceAreaAssessment
    {
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

        public BrickGovernanceAreaDefinition Definition { get; }
        public IReadOnlyList<BrickGovernanceRequirementResult> Results { get; }
        public int RequiredRequirementCount { get; }
        public int SatisfiedRequiredRequirementCount { get; }
        public int MissingRequiredRequirementCount { get; }
        public double ComplianceRatio { get; }
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
