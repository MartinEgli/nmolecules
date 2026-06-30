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
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
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

        /// <summary>
        /// Gets the Item value used by Bricks developer tooling.
        /// </summary>
        public BrickRoadmapItem Item { get; }
        /// <summary>
        /// Gets the Status value used by Bricks developer tooling.
        /// </summary>
        public BrickRoadmapItemStatus Status { get; }
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
