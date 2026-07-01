using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents roadmap item data used by roadmap stages, readiness checks, and implementation progress.
/// </summary>
public sealed class BrickRoadmapItem
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickRoadmapItem(
            string id,
            string displayName,
            bool required,
            string rationale = null)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Required = required;
            Rationale = rationale ?? string.Empty;
        }

        /// <summary>
        /// Gets the Id value used by Bricks developer tooling.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Gets the Display Name value used by Bricks developer tooling.
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// Gets a value indicating whether Required applies.
        /// </summary>
        public bool Required { get; }
        /// <summary>
        /// Gets the Rationale value used by Bricks developer tooling.
        /// </summary>
        public string Rationale { get; }
    }
}
