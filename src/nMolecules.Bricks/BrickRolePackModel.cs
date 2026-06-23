using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    public sealed class BrickRolePack
    {
        public BrickRolePack(
            string id,
            string displayName,
            IEnumerable<BrickRole> roles,
            IEnumerable<BrickRoleCombinationRule> combinationRules)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Roles = (roles ?? Enumerable.Empty<BrickRole>()).ToArray();
            CombinationRules = (combinationRules ?? Enumerable.Empty<BrickRoleCombinationRule>()).ToArray();
        }

        public string Id { get; }
        public string DisplayName { get; }
        public IReadOnlyList<BrickRole> Roles { get; }
        public IReadOnlyList<BrickRoleCombinationRule> CombinationRules { get; }

        public bool ContainsRole(RoleId roleId) => Roles.Any(role => role.Id == roleId);
    }

    public static class BrickBuiltInRolePacks
    {
        public static BrickRolePack StructuralCore => new BrickRolePack(
            "StructuralCore",
            "Structural Core",
            Roles(
                "StructuralCore",
                "Business.*",
                "Contracts",
                "Shared",
                "Infrastructure",
                "Platform",
                "Legacy",
                "Generated",
                "TestOnly",
                "FriendConsumer"),
            new[]
            {
                new BrickRoleCombinationRule(
                    "Contracts+Shared",
                    BrickRoleSelector.From("Contracts"),
                    BrickRoleSelector.From("Shared"),
                    BrickCombinationKind.Additive,
                    "Contract surfaces may also be shared."),
                new BrickRoleCombinationRule(
                    "Generated+Business",
                    BrickRoleSelector.From("Generated"),
                    BrickRoleSelector.From("Business.*"),
                    BrickCombinationKind.Incompatible,
                    "Generated code must not be treated as business-owned code without explicit review."),
                new BrickRoleCombinationRule(
                    "TestOnly+Business",
                    BrickRoleSelector.From("TestOnly"),
                    BrickRoleSelector.From("Business.*"),
                    BrickCombinationKind.Incompatible,
                    "Test-only code must not be part of production business partitions.")
            });

        public static BrickRolePack Ddd => new BrickRolePack(
            "DDD",
            "Domain-Driven Design",
            Roles(
                "DomainDrivenDesign",
                "DDD.Entity",
                "DDD.ValueObject",
                "DDD.AggregateRoot",
                "DDD.Repository",
                "DDD.Factory",
                "DDD.Service",
                "DDD.Identity",
                "DDD.BoundedContext",
                "DDD.Module"),
            null);

        public static BrickRolePack Events => new BrickRolePack(
            "Events",
            "Events",
            Roles(
                "Events",
                "Events.DomainEvent",
                "Events.DomainEventHandler",
                "Events.DomainEventPublisher"),
            null);

        public static BrickRolePack Architecture => new BrickRolePack(
            "Architecture",
            "Architecture",
            Roles(
                "ArchitectureLayer",
                "Architecture.Layer.Domain",
                "Architecture.Layer.Application",
                "Architecture.Layer.Infrastructure",
                "Architecture.Layer.Interface"),
            null);

        public static BrickRolePack Cqrs => new BrickRolePack(
            "CQRS",
            "Command Query Responsibility Segregation",
            Roles(
                "CQRS",
                "CQRS.Command",
                "CQRS.CommandHandler",
                "CQRS.Query",
                "CQRS.QueryHandler",
                "CQRS.ReadModel",
                "CQRS.Projector"),
            null);

        public static IReadOnlyList<BrickRolePack> All => new[] { StructuralCore, Ddd, Events, Architecture, Cqrs };

        private static IReadOnlyList<BrickRole> Roles(string dimensionId, params string[] roleIds) =>
            roleIds
                .Select(roleId => new BrickRole(
                    RoleId.From(roleId),
                    BrickDimensionId.From(dimensionId),
                    roleId,
                    category: dimensionId,
                    isBuiltin: true))
                .ToArray();
    }
}
