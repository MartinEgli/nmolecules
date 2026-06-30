using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents the result of governance requirement result processing in the Bricks pipeline.
/// </summary>
public sealed class BrickGovernanceRequirementResult
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickGovernanceRequirementResult(
            BrickGovernanceRequirement requirement,
            BrickGovernanceRequirementStatus status,
            string evidence = null)
        {
            Requirement = requirement ?? throw new ArgumentNullException(nameof(requirement));
            Status = status;
            Evidence = evidence ?? string.Empty;
            SatisfiesRequirement = !Requirement.Required || Status == BrickGovernanceRequirementStatus.Satisfied;
        }

        /// <summary>
        /// Gets a value indicating whether Requirement applies.
        /// </summary>
        public BrickGovernanceRequirement Requirement { get; }
        /// <summary>
        /// Gets the Status value used by Bricks developer tooling.
        /// </summary>
        public BrickGovernanceRequirementStatus Status { get; }
        /// <summary>
        /// Gets the Evidence value used by Bricks developer tooling.
        /// </summary>
        public string Evidence { get; }
        /// <summary>
        /// Gets the Satisfies Requirement value used by Bricks developer tooling.
        /// </summary>
        public bool SatisfiesRequirement { get; }
    }
}
