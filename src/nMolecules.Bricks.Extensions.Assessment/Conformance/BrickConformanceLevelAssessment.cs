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
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
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

        /// <summary>
        /// Gets the Definition value used by Bricks developer tooling.
        /// </summary>
        public BrickConformanceLevelDefinition Definition { get; }
        /// <summary>
        /// Gets the Results value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickConformanceCapabilityResult> Results { get; }
        /// <summary>
        /// Gets a value indicating whether Required Capability Count applies.
        /// </summary>
        public int RequiredCapabilityCount { get; }
        /// <summary>
        /// Gets the Satisfied Required Capability Count value used by Bricks developer tooling.
        /// </summary>
        public int SatisfiedRequiredCapabilityCount { get; }
        /// <summary>
        /// Gets the Missing Required Capability Count value used by Bricks developer tooling.
        /// </summary>
        public int MissingRequiredCapabilityCount { get; }
        /// <summary>
        /// Gets the Completion Ratio value used by Bricks developer tooling.
        /// </summary>
        public double CompletionRatio { get; }
        /// <summary>
        /// Gets the Status value used by Bricks developer tooling.
        /// </summary>
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
