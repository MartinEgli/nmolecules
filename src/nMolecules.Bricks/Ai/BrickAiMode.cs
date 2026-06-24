using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Configures which AI-assisted Bricks capability is enabled for a run.
    /// </summary>
    public enum BrickAiMode
    {
        /// <summary>Only deterministic Bricks output is produced.</summary>
        Off = 0,
        /// <summary>AI-readable explanations and remediation hints may be produced for deterministic violations.</summary>
        Explain = 1,
        /// <summary>AI may also create advisory rule proposals that require review before enforcement.</summary>
        SuggestRules = 2
    }
}
