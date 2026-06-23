using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
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

    public static class BrickBuiltInAttributeRoleBridges
    {
        public static BrickAttributeRoleBridge Ddd => new BrickAttributeRoleBridge(
            "DDD",
            new[]
            {
                Map("NMolecules.DDD.EntityAttribute", "DDD.Entity"),
                Map("NMolecules.DDD.ValueObjectAttribute", "DDD.ValueObject"),
                Map("NMolecules.DDD.AggregateRootAttribute", "DDD.AggregateRoot"),
                Map("NMolecules.DDD.RepositoryAttribute", "DDD.Repository"),
                Map("NMolecules.DDD.FactoryAttribute", "DDD.Factory"),
                Map("NMolecules.DDD.ServiceAttribute", "DDD.Service"),
                Map("NMolecules.DDD.DomainServiceAttribute", "DDD.Service"),
                Map("NMolecules.DDD.ApplicationServiceAttribute", "DDD.Service"),
                Map("NMolecules.DDD.Identity", "DDD.Identity"),
                Map("NMolecules.DDD.BoundedContextAttribute", "DDD.BoundedContext"),
                Map("NMolecules.DDD.ModuleAttribute", "DDD.Module")
            });

        private static BrickAttributeRoleMapping Map(string attributeTypeName, string roleId) =>
            new BrickAttributeRoleMapping(attributeTypeName, RoleId.From(roleId), "Built-in DDD attribute bridge.");
    }
}
