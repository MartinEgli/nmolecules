using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    public sealed class BrickProfile
    {
        public BrickProfile(
            string id,
            string displayName,
            IEnumerable<BrickRolePack> rolePacks,
            IEnumerable<BrickRule> rules,
            BrickPermissionDefault defaultDecision,
            BrickEnforcementMode enforcement)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            RolePacks = (rolePacks ?? Enumerable.Empty<BrickRolePack>()).ToArray();
            Rules = (rules ?? Enumerable.Empty<BrickRule>()).ToArray();
            DefaultDecision = defaultDecision;
            Enforcement = enforcement;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public IReadOnlyList<BrickRolePack> RolePacks { get; }
        public IReadOnlyList<BrickRule> Rules { get; }
        public BrickPermissionDefault DefaultDecision { get; }
        public BrickEnforcementMode Enforcement { get; }

        public bool ContainsRole(RoleId roleId) =>
            RolePacks.Any(pack => pack.ContainsRole(roleId));

        public BrickPolicy ToPolicy() =>
            ToPolicy(BrickPolicyId.From("Profile." + Id));

        public BrickPolicy ToPolicy(BrickPolicyId policyId) =>
            new BrickPolicy(
                policyId,
                DisplayName,
                Enumerable.Empty<BrickPolicyImport>(),
                Rules,
                RolePacks.SelectMany(pack => pack.CombinationRules),
                Enumerable.Empty<BrickRoleAssignment>(),
                Enumerable.Empty<BrickAlias>(),
                DefaultDecision,
                Enforcement);
    }

    public static class BrickBuiltInProfiles
    {
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
