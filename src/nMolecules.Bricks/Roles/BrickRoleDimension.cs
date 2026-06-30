using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Describes a dimension that groups related roles and their exclusivity rules.
    /// </summary>
    /// <remarks>
    /// Use dimensions to describe role families such as architecture layer, DDD building block or
    /// deployment role. Dimensions tell the resolver whether multiple roles can coexist.
    /// </remarks>
    public sealed class BrickRoleDimension
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickRoleDimension"/> class.
        /// </summary>
        /// <param name="id">The stable dimension id.</param>
        /// <param name="displayName">The developer-facing dimension name.</param>
        /// <param name="allowsMultipleRoles">Whether one element can carry multiple roles in this dimension.</param>
        /// <param name="isExclusiveByDefault">Whether roles in this dimension are exclusive unless configured otherwise.</param>
        /// <param name="description">The optional dimension description.</param>
        public BrickRoleDimension(BrickDimensionId id, string displayName, bool allowsMultipleRoles, bool isExclusiveByDefault, string description = null)
        {
            Id = id;
            DisplayName = displayName ?? string.Empty;
            AllowsMultipleRoles = allowsMultipleRoles;
            IsExclusiveByDefault = isExclusiveByDefault;
            Description = description;
        }

        /// <summary>
        /// Gets the stable dimension id.
        /// </summary>
        public BrickDimensionId Id { get; }

        /// <summary>
        /// Gets the developer-facing dimension name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets a value indicating whether one element can carry multiple roles in this dimension.
        /// </summary>
        public bool AllowsMultipleRoles { get; }

        /// <summary>
        /// Gets a value indicating whether roles in this dimension are exclusive unless configured otherwise.
        /// </summary>
        public bool IsExclusiveByDefault { get; }

        /// <summary>
        /// Gets the optional dimension description.
        /// </summary>
        public string Description { get; }
    }
}
