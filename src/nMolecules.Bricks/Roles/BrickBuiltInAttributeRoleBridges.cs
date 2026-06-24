using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
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
