using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickRole
    {
        public BrickRole(RoleId id, BrickDimensionId dimensionId, string displayName, string category = null, string description = null, bool isBuiltin = false)
        {
            Id = id;
            DimensionId = dimensionId;
            DisplayName = displayName ?? string.Empty;
            Category = category;
            Description = description;
            IsBuiltin = isBuiltin;
        }

        public RoleId Id { get; }
        public BrickDimensionId DimensionId { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public string Description { get; }
        public bool IsBuiltin { get; }
    }
}
