using System;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksAttributeRoleBridgeModelTest
    {
        [Theory]
        [InlineData("NMolecules.DDD.EntityAttribute", "DDD.Entity")]
        [InlineData("EntityAttribute", "DDD.Entity")]
        [InlineData("Entity", "DDD.Entity")]
        [InlineData("NMolecules.DDD.ValueObjectAttribute", "DDD.ValueObject")]
        [InlineData("NMolecules.DDD.AggregateRootAttribute", "DDD.AggregateRoot")]
        [InlineData("NMolecules.DDD.RepositoryAttribute", "DDD.Repository")]
        [InlineData("NMolecules.DDD.FactoryAttribute", "DDD.Factory")]
        [InlineData("NMolecules.DDD.ServiceAttribute", "DDD.Service")]
        [InlineData("NMolecules.DDD.DomainServiceAttribute", "DDD.Service")]
        [InlineData("NMolecules.DDD.ApplicationServiceAttribute", "DDD.Service")]
        [InlineData("NMolecules.DDD.Identity", "DDD.Identity")]
        [InlineData("NMolecules.DDD.BoundedContextAttribute", "DDD.BoundedContext")]
        [InlineData("NMolecules.DDD.ModuleAttribute", "DDD.Module")]
        public void BuiltInDddBridgeMapsDddAttributeNamesToRoles(string attributeTypeName, string expectedRoleId)
        {
            var roleId = BrickBuiltInAttributeRoleBridges.Ddd.FindRoleId(attributeTypeName);

            Assert.Equal(RoleId.From(expectedRoleId), roleId);
        }

        [Fact]
        public void AttributeRoleMappingNormalizesNullNames()
        {
            var mapping = new BrickAttributeRoleMapping(null, default, null);

            Assert.Equal(string.Empty, mapping.AttributeTypeName);
            Assert.Equal(default, mapping.RoleId);
            Assert.Null(mapping.Reason);
            Assert.Equal(string.Empty, BrickAttributeRoleMapping.Normalize(null));
        }

        [Fact]
        public void AttributeRoleBridgeNormalizesNullInputsAndUnknownAttributes()
        {
            var bridge = new BrickAttributeRoleBridge(null, null);

            Assert.Equal(string.Empty, bridge.Id);
            Assert.Empty(bridge.Mappings);
            Assert.Null(bridge.FindRoleId("UnknownAttribute"));
            Assert.Empty(bridge.FindRoleIds(null));
        }

        [Fact]
        public void FindRoleIdsFiltersUnknownsDistinctsAndSortsRoles()
        {
            var roles = BrickBuiltInAttributeRoleBridges.Ddd.FindRoleIds(
                new[]
                {
                    "RepositoryAttribute",
                    "UnknownAttribute",
                    "EntityAttribute",
                    "Repository",
                    null,
                    "ValueObjectAttribute"
                });

            Assert.Equal(
                new[]
                {
                    RoleId.From("DDD.Entity"),
                    RoleId.From("DDD.Repository"),
                    RoleId.From("DDD.ValueObject")
                },
                roles.ToArray());
        }

        [Fact]
        public void CreateAssignmentsCreatesElementScopedPackageAssignments()
        {
            var element = new BrickElement(
                BrickElementId.From("type:Billing.Domain.Order"),
                BrickElementKind.Type,
                "Order",
                "Billing.Domain",
                "Billing.Domain",
                "Billing.Domain.Order");

            var assignments = BrickBuiltInAttributeRoleBridges.Ddd.CreateAssignments(
                element,
                new[] { "EntityAttribute", "AggregateRootAttribute" });

            Assert.Equal(new[] { RoleId.From("DDD.AggregateRoot"), RoleId.From("DDD.Entity") }, assignments.Select(assignment => assignment.RoleId).ToArray());
            Assert.All(assignments, assignment =>
            {
                Assert.Equal(new BrickElementSelector(BrickElementKind.Type, "type:Billing.Domain.Order", "Billing.Domain"), assignment.Selector);
                Assert.Equal(BrickAssignmentMode.AliasMapping, assignment.Mode);
                Assert.Equal(BrickAssignmentSource.Package, assignment.Source);
                Assert.Equal(new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Alias), assignment.Precedence);
                Assert.Equal(BrickAssignmentBehavior.Apply, assignment.Behavior);
                Assert.Equal("Mapped by attribute bridge 'DDD'.", assignment.Reason);
            });
        }

        [Fact]
        public void CreateAssignmentsRequiresElement()
        {
            Assert.Throws<ArgumentNullException>(() => BrickBuiltInAttributeRoleBridges.Ddd.CreateAssignments(null, null));
        }
    }
}
