using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Maps an attribute type name to a Bricks role id.
    /// </summary>
    public sealed class BrickAttributeRoleMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickAttributeRoleMapping"/> class.
        /// </summary>
        /// <param name="attributeTypeName">The attribute type name to map.</param>
        /// <param name="roleId">The role id represented by the attribute.</param>
        /// <param name="reason">The optional mapping reason.</param>
        public BrickAttributeRoleMapping(string attributeTypeName, RoleId roleId, string reason = null)
        {
            AttributeTypeName = attributeTypeName ?? string.Empty;
            RoleId = roleId;
            Reason = reason;
            Key = Normalize(attributeTypeName);
        }

        /// <summary>
        /// Gets the attribute type name to map.
        /// </summary>
        public string AttributeTypeName { get; }

        /// <summary>
        /// Gets the role id represented by the attribute.
        /// </summary>
        public RoleId RoleId { get; }

        /// <summary>
        /// Gets the optional mapping reason.
        /// </summary>
        public string Reason { get; }
        internal string Key { get; }

        /// <summary>
        /// Normalizes an attribute type name for matching.
        /// </summary>
        /// <param name="attributeTypeName">The full or short attribute type name.</param>
        /// <returns>The short attribute name without namespace or <c>Attribute</c> suffix.</returns>
        public static string Normalize(string attributeTypeName)
        {
            var value = attributeTypeName ?? string.Empty;
            var lastDot = value.LastIndexOf('.');
            if (lastDot >= 0)
            {
                value = value.Substring(lastDot + 1);
            }

            return value.EndsWith("Attribute", StringComparison.Ordinal)
                ? value.Substring(0, value.Length - "Attribute".Length)
                : value;
        }
    }
}
