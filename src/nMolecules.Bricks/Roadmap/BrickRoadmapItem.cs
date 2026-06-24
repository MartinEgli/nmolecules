using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
public sealed class BrickRoadmapItem
    {
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

        public string Id { get; }
        public string DisplayName { get; }
        public bool Required { get; }
        public string Rationale { get; }
    }
}
