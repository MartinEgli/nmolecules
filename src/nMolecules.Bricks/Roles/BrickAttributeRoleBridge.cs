using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Bridges known attribute types to Bricks role assignments.
    /// </summary>
    /// <remarks>
    /// Use a bridge when an existing attribute library should participate in Bricks role resolution
    /// without changing those attributes.
    /// </remarks>
    public sealed class BrickAttributeRoleBridge
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickAttributeRoleBridge"/> class.
        /// </summary>
        /// <param name="id">The bridge id.</param>
        /// <param name="mappings">The attribute-to-role mappings.</param>
        public BrickAttributeRoleBridge(string id, IEnumerable<BrickAttributeRoleMapping> mappings)
        {
            Id = id ?? string.Empty;
            Mappings = (mappings ?? Enumerable.Empty<BrickAttributeRoleMapping>()).ToArray();
        }

        /// <summary>
        /// Gets the bridge id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the attribute-to-role mappings.
        /// </summary>
        public IReadOnlyList<BrickAttributeRoleMapping> Mappings { get; }

        /// <summary>
        /// Finds the role id mapped to the supplied attribute type name.
        /// </summary>
        /// <param name="attributeTypeName">The full or short attribute type name.</param>
        /// <returns>The mapped role id, or <c>null</c> when no mapping exists.</returns>
        public RoleId? FindRoleId(string attributeTypeName)
        {
            var key = BrickAttributeRoleMapping.Normalize(attributeTypeName);
            var mapping = Mappings.FirstOrDefault(candidate => string.Equals(candidate.Key, key, StringComparison.Ordinal));
            return mapping == null ? (RoleId?)null : mapping.RoleId;
        }

        /// <summary>
        /// Finds all role ids mapped from the supplied attribute type names.
        /// </summary>
        /// <param name="attributeTypeNames">Attribute type names to map.</param>
        /// <returns>Distinct role ids in deterministic order.</returns>
        public IReadOnlyList<RoleId> FindRoleIds(IEnumerable<string> attributeTypeNames) =>
            (attributeTypeNames ?? Enumerable.Empty<string>())
                .Select(FindRoleId)
                .Where(roleId => roleId.HasValue)
                .Select(roleId => roleId.Value)
                .Distinct()
                .OrderBy(roleId => roleId.Value, StringComparer.Ordinal)
                .ToArray();

        /// <summary>
        /// Creates role assignments for an element from matching attribute type names.
        /// </summary>
        /// <param name="element">The element carrying the attributes.</param>
        /// <param name="attributeTypeNames">The element's attribute type names.</param>
        /// <returns>Role assignments created by this bridge.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="element"/> is null.</exception>
        public IReadOnlyList<BrickRoleAssignment> CreateAssignments(BrickElement element, IEnumerable<string> attributeTypeNames)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return FindRoleIds(attributeTypeNames)
                .Select(roleId => new BrickRoleAssignment(
                    new BrickElementSelector(element.Kind, element.Id.Value, element.AssemblyName),
                    roleId,
                    BrickAssignmentMode.AliasMapping,
                    BrickAssignmentSource.Package,
                    new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Alias),
                    BrickAssignmentBehavior.Apply,
                    $"Mapped by attribute bridge '{Id}'."))
                .ToArray();
        }
    }
}
