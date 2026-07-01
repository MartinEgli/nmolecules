using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Enumerates the supported governance area values used by governance readiness areas, requirements, and
/// summaries.
/// </summary>
public enum BrickGovernanceArea
    {
        /// <summary>
        /// Gets the Policy Ownership value used by Bricks developer tooling.
        /// </summary>
        PolicyOwnership = 0,
        /// <summary>
        /// Gets the Exception Handling value used by Bricks developer tooling.
        /// </summary>
        ExceptionHandling = 1,
        /// <summary>
        /// Gets the Role Pack Evolution value used by Bricks developer tooling.
        /// </summary>
        RolePackEvolution = 2,
        /// <summary>
        /// Gets the Compatibility Expectations value used by Bricks developer tooling.
        /// </summary>
        CompatibilityExpectations = 3
    }
}
