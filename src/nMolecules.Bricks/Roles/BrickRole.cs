using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Describes a role that can be assigned to a Bricks element.
    /// </summary>
    /// <remarks>
    /// Role definitions are catalog metadata. Use them in role packs and documentation so tools can
    /// display a stable id, readable name and optional category for each architectural role.
    /// </remarks>
    public sealed class BrickRole
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickRole"/> class.
        /// </summary>
        /// <param name="id">The stable role id.</param>
        /// <param name="dimensionId">The dimension this role belongs to.</param>
        /// <param name="displayName">The developer-facing role name.</param>
        /// <param name="category">The optional category used for grouping in reports or documentation.</param>
        /// <param name="description">The optional role description.</param>
        /// <param name="isBuiltin">Whether the role is supplied by a built-in Bricks role pack.</param>
        public BrickRole(RoleId id, BrickDimensionId dimensionId, string displayName, string category = null, string description = null, bool isBuiltin = false)
        {
            Id = id;
            DimensionId = dimensionId;
            DisplayName = displayName ?? string.Empty;
            Category = category;
            Description = description;
            IsBuiltin = isBuiltin;
        }

        /// <summary>
        /// Gets the stable role id.
        /// </summary>
        public RoleId Id { get; }

        /// <summary>
        /// Gets the dimension this role belongs to.
        /// </summary>
        public BrickDimensionId DimensionId { get; }

        /// <summary>
        /// Gets the developer-facing role name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the optional category used for grouping in reports or documentation.
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// Gets the optional role description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets a value indicating whether the role is supplied by a built-in Bricks role pack.
        /// </summary>
        public bool IsBuiltin { get; }
    }
}
