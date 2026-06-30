using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Provides built-in Bricks defaults for built-in Bricks profiles.
/// </summary>
public static class BrickBuiltInProfiles
    {
        /// <summary>
        /// Gets the Layered value used by Bricks developer tooling.
        /// </summary>
        public static BrickProfile Layered => new BrickProfile(
            "Layered",
            "Layered Architecture",
            new[] { BrickBuiltInRolePacks.Architecture },
            new[]
            {
                Rule(
                    "Profile.Layered.DomainMustNotDependOnInfrastructure",
                    "Architecture.Layer.Domain",
                    "Architecture.Layer.Infrastructure"),
                Rule(
                    "Profile.Layered.DomainMustNotDependOnInterface",
                    "Architecture.Layer.Domain",
                    "Architecture.Layer.Interface"),
                Rule(
                    "Profile.Layered.ApplicationMustNotDependOnInterface",
                    "Architecture.Layer.Application",
                    "Architecture.Layer.Interface")
            },
            BrickPermissionDefault.Deny,
            BrickEnforcementMode.Analyze);

        /// <summary>
        /// Gets the Onion value used by Bricks developer tooling.
        /// </summary>
        public static BrickProfile Onion => new BrickProfile(
            "Onion",
            "Onion Architecture",
            new[] { BrickBuiltInRolePacks.Architecture },
            new[]
            {
                Rule(
                    "Profile.Onion.DomainMustNotDependOnApplication",
                    "Architecture.Layer.Domain",
                    "Architecture.Layer.Application"),
                Rule(
                    "Profile.Onion.DomainMustNotDependOnInfrastructure",
                    "Architecture.Layer.Domain",
                    "Architecture.Layer.Infrastructure"),
                Rule(
                    "Profile.Onion.DomainMustNotDependOnInterface",
                    "Architecture.Layer.Domain",
                    "Architecture.Layer.Interface")
            },
            BrickPermissionDefault.Deny,
            BrickEnforcementMode.Analyze);

        /// <summary>
        /// Gets the Hexagonal value used by Bricks developer tooling.
        /// </summary>
        public static BrickProfile Hexagonal => new BrickProfile(
            "Hexagonal",
            "Hexagonal Architecture",
            new[] { BrickBuiltInRolePacks.Architecture },
            new[]
            {
                Rule(
                    "Profile.Hexagonal.DomainMustNotDependOnInterfaceAdapters",
                    "Architecture.Layer.Domain",
                    "Architecture.Layer.Interface"),
                Rule(
                    "Profile.Hexagonal.InterfaceAdaptersMustNotDependOnInfrastructureAdapters",
                    "Architecture.Layer.Interface",
                    "Architecture.Layer.Infrastructure")
            },
            BrickPermissionDefault.Deny,
            BrickEnforcementMode.Analyze);

        /// <summary>
        /// Gets the Cqrs value used by Bricks developer tooling.
        /// </summary>
        public static BrickProfile Cqrs => new BrickProfile(
            "CQRS",
            "Command Query Responsibility Segregation",
            new[] { BrickBuiltInRolePacks.Architecture, BrickBuiltInRolePacks.Cqrs },
            new[]
            {
                Rule(
                    "Profile.CQRS.CommandHandlersMustNotDependOnQueryHandlers",
                    "CQRS.CommandHandler",
                    "CQRS.QueryHandler"),
                Rule(
                    "Profile.CQRS.QueryHandlersMustNotDependOnCommandHandlers",
                    "CQRS.QueryHandler",
                    "CQRS.CommandHandler"),
                Rule(
                    "Profile.CQRS.ReadModelsMustNotDependOnCommandHandlers",
                    "CQRS.ReadModel",
                    "CQRS.CommandHandler")
            },
            BrickPermissionDefault.Deny,
            BrickEnforcementMode.Analyze);

        /// <summary>
        /// Gets the All value used by Bricks developer tooling.
        /// </summary>
        public static IReadOnlyList<BrickProfile> All => new[] { Layered, Onion, Hexagonal, Cqrs };

        private static BrickRule Rule(string id, string sourceRole, string targetRole) =>
            new BrickRule(
                RuleId.From(id),
                id,
                RoleId.From(sourceRole),
                RoleId.From(targetRole),
                BrickDecision.Deny,
                BrickScope.Type,
                BrickSeverity.Error,
                reason: "Built-in architectural profile rule.");
    }
}
