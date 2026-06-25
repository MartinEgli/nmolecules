using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents attribute role mapping data used by role dimensions, assignments, resolution, conflicts, and
/// role packs.
/// </summary>
public sealed class BrickAttributeRoleMapping
    {
        public BrickAttributeRoleMapping(string attributeTypeName, RoleId roleId, string reason = null)
        {
            AttributeTypeName = attributeTypeName ?? string.Empty;
            RoleId = roleId;
            Reason = reason;
            Key = Normalize(attributeTypeName);
        }

        public string AttributeTypeName { get; }
        public RoleId RoleId { get; }
        public string Reason { get; }
        internal string Key { get; }

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
