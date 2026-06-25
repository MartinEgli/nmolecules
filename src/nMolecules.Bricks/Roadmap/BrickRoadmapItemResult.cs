using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents the result of roadmap item result processing in the Bricks pipeline.
/// </summary>
public sealed class BrickRoadmapItemResult
    {
        public BrickRoadmapItemResult(
            BrickRoadmapItem item,
            BrickRoadmapItemStatus status,
            string evidence = null)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Status = status;
            Evidence = evidence ?? string.Empty;
            SatisfiesRequirement = !Item.Required || Status == BrickRoadmapItemStatus.Completed;
        }

        public BrickRoadmapItem Item { get; }
        public BrickRoadmapItemStatus Status { get; }
        public string Evidence { get; }
        public bool SatisfiesRequirement { get; }
    }
}
