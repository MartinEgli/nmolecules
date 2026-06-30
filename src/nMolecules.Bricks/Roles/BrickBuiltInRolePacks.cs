using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Provides built-in role packs for common Bricks scenarios.
    /// </summary>
    public static class BrickBuiltInRolePacks
    {
        /// <summary>
        /// Gets the structural core role pack.
        /// </summary>
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

        /// <summary>
        /// Gets the Domain-Driven Design role pack.
        /// </summary>
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

        /// <summary>
        /// Gets the events role pack.
        /// </summary>
        public static BrickRolePack Events => new BrickRolePack(
            "Events",
            "Events",
            Roles(
                "Events",
                "Events.DomainEvent",
                "Events.DomainEventHandler",
                "Events.DomainEventPublisher"),
            null);

        /// <summary>
        /// Gets the architecture layer role pack.
        /// </summary>
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

        /// <summary>
        /// Gets the CQRS role pack.
        /// </summary>
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

        /// <summary>
        /// Gets all built-in role packs in deterministic order.
        /// </summary>
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
