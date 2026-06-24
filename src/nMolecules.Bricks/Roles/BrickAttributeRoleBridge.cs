using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickAttributeRoleBridge
    {
        public BrickAttributeRoleBridge(string id, IEnumerable<BrickAttributeRoleMapping> mappings)
        {
            Id = id ?? string.Empty;
            Mappings = (mappings ?? Enumerable.Empty<BrickAttributeRoleMapping>()).ToArray();
        }

        public string Id { get; }
        public IReadOnlyList<BrickAttributeRoleMapping> Mappings { get; }

        public RoleId? FindRoleId(string attributeTypeName)
        {
            var key = BrickAttributeRoleMapping.Normalize(attributeTypeName);
            var mapping = Mappings.FirstOrDefault(candidate => string.Equals(candidate.Key, key, StringComparison.Ordinal));
            return mapping == null ? (RoleId?)null : mapping.RoleId;
        }

        public IReadOnlyList<RoleId> FindRoleIds(IEnumerable<string> attributeTypeNames) =>
            (attributeTypeNames ?? Enumerable.Empty<string>())
                .Select(FindRoleId)
                .Where(roleId => roleId.HasValue)
                .Select(roleId => roleId.Value)
                .Distinct()
                .OrderBy(roleId => roleId.Value, StringComparer.Ordinal)
                .ToArray();

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
