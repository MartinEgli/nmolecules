using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes one possible remediation for a deterministic Bricks violation.
    /// </summary>
    public sealed class BrickRemediationOption
    {
        /// <summary>
        /// Creates a remediation option with its category, decision guidance, risk, and preference flag.
        /// </summary>
        public BrickRemediationOption(
            string id,
            BrickRemediationKind kind,
            string description,
            string whenToUse,
            BrickRemediationRisk risk,
            bool isPreferred)
        {
            Id = id ?? string.Empty;
            Kind = kind;
            Description = description ?? string.Empty;
            WhenToUse = whenToUse ?? string.Empty;
            Risk = risk;
            IsPreferred = isPreferred;
        }

        /// <summary>Stable option identifier used by JSON output and recommendations.</summary>
        public string Id { get; }
        /// <summary>The structural remediation category.</summary>
        public BrickRemediationKind Kind { get; }
        /// <summary>Human-readable description of the change to make.</summary>
        public string Description { get; }
        /// <summary>Guidance for when this option is appropriate.</summary>
        public string WhenToUse { get; }
        /// <summary>Expected architectural risk of applying this option.</summary>
        public BrickRemediationRisk Risk { get; }
        /// <summary>Whether this option is preferred for the represented violation.</summary>
        public bool IsPreferred { get; }
    }
}
