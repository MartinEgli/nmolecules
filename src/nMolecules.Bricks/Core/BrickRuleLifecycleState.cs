using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Represents the review lifecycle of an AI-generated rule proposal.
    /// </summary>
    public enum BrickRuleLifecycleState
    {
        /// <summary>AI observed a possible rule, but no human has accepted it for evaluation.</summary>
        Candidate = 0,
        /// <summary>A human marked the proposal as worth shaping and validating.</summary>
        Draft = 1,
        /// <summary>The proposed rule may run in reporting mode without affecting builds.</summary>
        Observing = 2,
        /// <summary>The proposed rule may emit warnings after review.</summary>
        Warning = 3,
        /// <summary>The rule is active enforcement and can break builds after explicit promotion.</summary>
        Enforced = 4,
        /// <summary>The proposal was reviewed and discarded.</summary>
        Rejected = 5,
        /// <summary>The proposal or promoted rule is no longer recommended.</summary>
        Deprecated = 6
    }
}
