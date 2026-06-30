using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents conformance level assessment data used by conformance capability checks and level assessments.
/// </summary>
public sealed class BrickConformanceLevelAssessment
    {
        public BrickConformanceLevelAssessment(
            BrickConformanceLevelDefinition definition,
            IEnumerable<BrickConformanceCapabilityResult> results)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Results = (results ?? Enumerable.Empty<BrickConformanceCapabilityResult>())
                .Where(result => result != null)
                .OrderBy(result => result.Capability.Id, StringComparer.Ordinal)
                .ToArray();

            var requiredCapabilities = Definition.Capabilities
                .Where(capability => capability.Required)
                .ToArray();
            RequiredCapabilityCount = requiredCapabilities.Length;
            SatisfiedRequiredCapabilityCount = requiredCapabilities.Count(IsSatisfied);
            MissingRequiredCapabilityCount = RequiredCapabilityCount - SatisfiedRequiredCapabilityCount;
            CompletionRatio = RequiredCapabilityCount == 0
                ? 1d
                : (double)SatisfiedRequiredCapabilityCount / RequiredCapabilityCount;
            Status = ResolveStatus();
        }

        public BrickConformanceLevelDefinition Definition { get; }
        public IReadOnlyList<BrickConformanceCapabilityResult> Results { get; }
        public int RequiredCapabilityCount { get; }
        public int SatisfiedRequiredCapabilityCount { get; }
        public int MissingRequiredCapabilityCount { get; }
        public double CompletionRatio { get; }
        public BrickConformanceLevelStatus Status { get; }

        private bool IsSatisfied(BrickConformanceCapability capability) =>
            Results.Any(result =>
                string.Equals(result.Capability.Id, capability.Id, StringComparison.Ordinal) &&
                result.SatisfiesRequirement);

        private BrickConformanceLevelStatus ResolveStatus()
        {
            if (MissingRequiredCapabilityCount == 0)
            {
                return BrickConformanceLevelStatus.Achieved;
            }

            return SatisfiedRequiredCapabilityCount > 0
                ? BrickConformanceLevelStatus.Partial
                : BrickConformanceLevelStatus.NotStarted;
        }
    }
}
