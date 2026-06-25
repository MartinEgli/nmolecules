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

        public BrickGovernanceRequirement Requirement { get; }
        public BrickGovernanceRequirementStatus Status { get; }
        public string Evidence { get; }
        public bool SatisfiesRequirement { get; }
    }
}
