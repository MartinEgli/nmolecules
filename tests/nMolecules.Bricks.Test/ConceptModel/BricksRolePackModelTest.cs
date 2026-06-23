using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksRolePackModelTest
    {
        [Fact]
        public void BrickRolePackCopiesRolesAndCombinationRules()
        {
            var role = Role("Contracts", "StructuralCore");
            var combination = new BrickRoleCombinationRule(
                "Contracts+Shared",
                BrickRoleSelector.From("Contracts"),
                BrickRoleSelector.From("Shared"),
                BrickCombinationKind.Additive);
            var roles = new[] { role };
            var combinations = new[] { combination };

            var pack = new BrickRolePack("StructuralCore", "Structural Core", roles, combinations);
            roles[0] = Role("Other", "StructuralCore");
            combinations[0] = new BrickRoleCombinationRule("Other", null, null, BrickCombinationKind.Incompatible);

            Assert.Equal("StructuralCore", pack.Id);
            Assert.Equal("Structural Core", pack.DisplayName);
            Assert.Equal(role, pack.Roles.Single());
            Assert.Same(combination, pack.CombinationRules.Single());
            Assert.True(pack.ContainsRole(RoleId.From("Contracts")));
            Assert.False(pack.ContainsRole(RoleId.From("Other")));
        }

        [Fact]
        public void BrickRolePackNormalizesNullValues()
        {
            var pack = new BrickRolePack(null, null, null, null);

            Assert.Equal(string.Empty, pack.Id);
            Assert.Equal(string.Empty, pack.DisplayName);
            Assert.Empty(pack.Roles);
            Assert.Empty(pack.CombinationRules);
            Assert.False(pack.ContainsRole(default));
        }

        [Fact]
        public void BuiltInStructuralCorePackContainsV22RolesAndCombinationRules()
        {
            var pack = BrickBuiltInRolePacks.StructuralCore;

            Assert.Equal("StructuralCore", pack.Id);
            Assert.Equal(new[]
            {
                "Business.*",
                "Contracts",
                "Shared",
                "Infrastructure",
                "Platform",
                "Legacy",
                "Generated",
                "TestOnly",
                "FriendConsumer"
            }, pack.Roles.Select(role => role.Id.Value).ToArray());
            Assert.Contains(pack.CombinationRules, rule => rule.Kind == BrickCombinationKind.Additive && rule.Matches(RoleId.From("Contracts"), RoleId.From("Shared")));
            Assert.Contains(pack.CombinationRules, rule => rule.Kind == BrickCombinationKind.Incompatible && rule.Matches(RoleId.From("Generated"), RoleId.From("Business.Sales")));
            Assert.Contains(pack.CombinationRules, rule => rule.Kind == BrickCombinationKind.Incompatible && rule.Matches(RoleId.From("TestOnly"), RoleId.From("Business.Support")));
        }

        [Fact]
        public void BuiltInDddPackContainsV22Roles()
        {
            Assert.Equal(new[]
            {
                "DDD.Entity",
                "DDD.ValueObject",
                "DDD.AggregateRoot",
                "DDD.Repository",
                "DDD.Factory",
                "DDD.Service",
                "DDD.Identity",
                "DDD.BoundedContext",
                "DDD.Module"
            }, BrickBuiltInRolePacks.Ddd.Roles.Select(role => role.Id.Value).ToArray());
        }

        [Fact]
        public void BuiltInEventsPackContainsV22Roles()
        {
            Assert.Equal(new[]
            {
                "Events.DomainEvent",
                "Events.DomainEventHandler",
                "Events.DomainEventPublisher"
            }, BrickBuiltInRolePacks.Events.Roles.Select(role => role.Id.Value).ToArray());
        }

        [Fact]
        public void BuiltInArchitecturePackContainsV22Roles()
        {
            Assert.Equal(new[]
            {
                "Architecture.Layer.Domain",
                "Architecture.Layer.Application",
                "Architecture.Layer.Infrastructure",
                "Architecture.Layer.Interface"
            }, BrickBuiltInRolePacks.Architecture.Roles.Select(role => role.Id.Value).ToArray());
        }

        [Fact]
        public void AllBuiltInPacksAreReturnedInStableOrderAndCopied()
        {
            var packs = BrickBuiltInRolePacks.All.ToArray();
            packs[0] = new BrickRolePack("Other", "Other", null, null);

            Assert.Equal(new[] { "StructuralCore", "DDD", "Events", "Architecture" }, BrickBuiltInRolePacks.All.Select(pack => pack.Id).ToArray());
        }

        private static BrickRole Role(string id, string dimension) =>
            new BrickRole(RoleId.From(id), BrickDimensionId.From(dimension), id);
    }
}
