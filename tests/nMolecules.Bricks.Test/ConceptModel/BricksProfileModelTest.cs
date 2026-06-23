using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksProfileModelTest
    {
        [Fact]
        public void BrickProfileCopiesPacksAndRules()
        {
            var pack = BrickBuiltInRolePacks.Architecture;
            var rule = Rule("Profile.Layered.DomainIsolation", "Architecture.Layer.Domain", "Architecture.Layer.Infrastructure");
            var packs = new[] { pack };
            var rules = new[] { rule };

            var profile = new BrickProfile(
                "Layered",
                "Layered",
                packs,
                rules,
                BrickPermissionDefault.Deny,
                BrickEnforcementMode.Enforce);
            packs[0] = BrickBuiltInRolePacks.StructuralCore;
            rules[0] = Rule("Other", "A", "B");

            Assert.Equal("Layered", profile.Id);
            Assert.Equal("Layered", profile.DisplayName);
            Assert.Same(pack, profile.RolePacks.Single());
            Assert.Equal(rule, profile.Rules.Single());
            Assert.Equal(BrickPermissionDefault.Deny, profile.DefaultDecision);
            Assert.Equal(BrickEnforcementMode.Enforce, profile.Enforcement);
            Assert.True(profile.ContainsRole(RoleId.From("Architecture.Layer.Domain")));
            Assert.False(profile.ContainsRole(RoleId.From("Unknown")));
        }

        [Fact]
        public void BrickProfileNormalizesNullValues()
        {
            var profile = new BrickProfile(null, null, null, null, BrickPermissionDefault.Allow, BrickEnforcementMode.Document);

            Assert.Equal(string.Empty, profile.Id);
            Assert.Equal(string.Empty, profile.DisplayName);
            Assert.Empty(profile.RolePacks);
            Assert.Empty(profile.Rules);
            Assert.Equal(BrickPermissionDefault.Allow, profile.DefaultDecision);
            Assert.Equal(BrickEnforcementMode.Document, profile.Enforcement);
            Assert.False(profile.ContainsRole(default));
        }

        [Fact]
        public void BrickProfileCreatesPolicyWithProfileRulesAndPackCombinationRules()
        {
            var profile = new BrickProfile(
                "Custom",
                "Custom",
                new[] { BrickBuiltInRolePacks.StructuralCore },
                new[] { Rule("Profile.Custom.NoPlatform", "Business.*", "Platform") },
                BrickPermissionDefault.Deny,
                BrickEnforcementMode.Analyze);

            var policy = profile.ToPolicy();

            Assert.Equal(BrickPolicyId.From("Profile.Custom"), policy.Id);
            Assert.Equal("Custom", policy.Name);
            Assert.Empty(policy.Imports);
            Assert.Equal(RuleId.From("Profile.Custom.NoPlatform"), policy.Rules.Single().RuleId);
            Assert.Equal(BrickBuiltInRolePacks.StructuralCore.CombinationRules.Count, policy.CombinationRules.Count);
            Assert.Equal(BrickPermissionDefault.Deny, policy.DefaultDecision);
            Assert.Equal(BrickEnforcementMode.Analyze, policy.Enforcement);
        }

        [Fact]
        public void BrickProfileCanCreatePolicyWithExplicitId()
        {
            var profile = new BrickProfile("Custom", "Custom", null, null, BrickPermissionDefault.Allow, BrickEnforcementMode.Document);

            var policy = profile.ToPolicy(BrickPolicyId.From("ProjectPolicy"));

            Assert.Equal(BrickPolicyId.From("ProjectPolicy"), policy.Id);
            Assert.Equal(BrickPermissionDefault.Allow, policy.DefaultDecision);
            Assert.Equal(BrickEnforcementMode.Document, policy.Enforcement);
        }

        [Fact]
        public void BuiltInProfilesExposeStableArchitecturalProfiles()
        {
            Assert.Equal(new[] { "Layered", "Onion", "Hexagonal", "CQRS" }, BrickBuiltInProfiles.All.Select(profile => profile.Id).ToArray());
            Assert.All(BrickBuiltInProfiles.All, profile => Assert.Contains(profile.RolePacks, pack => pack.Id == "Architecture"));
            Assert.Contains(BrickBuiltInProfiles.Cqrs.RolePacks, pack => pack.Id == "CQRS");
        }

        [Fact]
        public void BuiltInLayeredProfileDefinesLayerDirectionRules()
        {
            var profile = BrickBuiltInProfiles.Layered;

            Assert.Equal("Layered", profile.Id);
            Assert.Equal(BrickPermissionDefault.Deny, profile.DefaultDecision);
            Assert.Equal(BrickEnforcementMode.Analyze, profile.Enforcement);
            Assert.Contains(profile.Rules, rule => rule.RuleId == RuleId.From("Profile.Layered.DomainMustNotDependOnInfrastructure"));
            Assert.Contains(profile.Rules, rule => rule.SourceRole == RoleId.From("Architecture.Layer.Application") && rule.TargetRole == RoleId.From("Architecture.Layer.Interface"));
        }

        [Fact]
        public void BuiltInOnionProfileKeepsDomainIndependent()
        {
            var profile = BrickBuiltInProfiles.Onion;

            Assert.Contains(profile.Rules, rule => rule.SourceRole == RoleId.From("Architecture.Layer.Domain") && rule.TargetRole == RoleId.From("Architecture.Layer.Application"));
            Assert.Contains(profile.Rules, rule => rule.SourceRole == RoleId.From("Architecture.Layer.Domain") && rule.TargetRole == RoleId.From("Architecture.Layer.Infrastructure"));
        }

        [Fact]
        public void BuiltInHexagonalProfileSeparatesInterfaceAndInfrastructureAdapters()
        {
            var profile = BrickBuiltInProfiles.Hexagonal;

            Assert.Contains(profile.Rules, rule => rule.SourceRole == RoleId.From("Architecture.Layer.Domain") && rule.TargetRole == RoleId.From("Architecture.Layer.Interface"));
            Assert.Contains(profile.Rules, rule => rule.SourceRole == RoleId.From("Architecture.Layer.Interface") && rule.TargetRole == RoleId.From("Architecture.Layer.Infrastructure"));
        }

        [Fact]
        public void BuiltInCqrsProfileIncludesCqrsPackAndReadWriteSeparation()
        {
            var profile = BrickBuiltInProfiles.Cqrs;

            Assert.Contains(profile.RolePacks, pack => pack.Id == "CQRS");
            Assert.True(profile.ContainsRole(RoleId.From("CQRS.CommandHandler")));
            Assert.True(profile.ContainsRole(RoleId.From("CQRS.QueryHandler")));
            Assert.Contains(profile.Rules, rule => rule.SourceRole == RoleId.From("CQRS.CommandHandler") && rule.TargetRole == RoleId.From("CQRS.QueryHandler"));
            Assert.Contains(profile.Rules, rule => rule.SourceRole == RoleId.From("CQRS.QueryHandler") && rule.TargetRole == RoleId.From("CQRS.CommandHandler"));
        }

        private static BrickRule Rule(string id, string sourceRole, string targetRole) =>
            new BrickRule(
                RuleId.From(id),
                id,
                RoleId.From(sourceRole),
                RoleId.From(targetRole),
                BrickDecision.Deny,
                BrickScope.Type,
                BrickSeverity.Error);
    }
}
