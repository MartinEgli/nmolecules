using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickRoleDimension
    {
        public BrickRoleDimension(BrickDimensionId id, string displayName, bool allowsMultipleRoles, bool isExclusiveByDefault, string description = null)
        {
            Id = id;
            DisplayName = displayName ?? string.Empty;
            AllowsMultipleRoles = allowsMultipleRoles;
            IsExclusiveByDefault = isExclusiveByDefault;
            Description = description;
        }

        public BrickDimensionId Id { get; }
        public string DisplayName { get; }
        public bool AllowsMultipleRoles { get; }
        public bool IsExclusiveByDefault { get; }
        public string Description { get; }
    }
}
