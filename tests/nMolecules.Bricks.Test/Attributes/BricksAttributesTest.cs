using System;
using System.Linq;
using System.Reflection;
using Xunit;

[assembly: NMolecules.Bricks.Rule("BRK-001", "Domain", "Infrastructure", NMolecules.Bricks.RuleMode.ForbidDependency, "Domain must not use infrastructure")]

namespace NMolecules.Bricks.Test
{
    public static class BillingRoles
    {
        public const string Domain = "Billing.Domain";
        public const string Infrastructure = "Billing.Infrastructure";
    }

    public static class BillingRules
    {
        public const string DomainMustNotDependOnInfrastructure = "BILL-CUSTOM-001";
    }

    public class BillingDomainRoleAttribute : RoleAttribute
    {
        public BillingDomainRoleAttribute() : base(BillingRoles.Domain)
        {
        }
    }

    public class BillingInfrastructureAliasAttribute : RoleAliasAttribute
    {
        public BillingInfrastructureAliasAttribute()
        {
            Role = BillingRoles.Infrastructure;
        }
    }

    public class BillingRuleAttribute : RuleAttribute
    {
        public BillingRuleAttribute(string sourceRole, string targetRole)
            : base(BillingRules.DomainMustNotDependOnInfrastructure, sourceRole, targetRole)
        {
        }
    }

    [RoleAlias("Domain")]
    public class DomainAliasAttribute : Attribute
    {
    }

    [Role("Domain")]
    [DomainAlias]
    public class DomainType
    {
    }

    [BillingDomainRole]
    public class SpecializedDomainType
    {
    }

    [BillingInfrastructureAlias]
    public class SpecializedInfrastructureAliasMarker : Attribute
    {
    }

    public class AttributesTest
    {
        public static TheoryData<Type, AttributeTargets> AttributeTargetsData => new()
        {
            { typeof(RoleAttribute), AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(RoleAliasAttribute), AttributeTargets.Class },
            { typeof(RuleAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class }
        };

        [Theory]
        [MemberData(nameof(AttributeTargetsData))]
        public void DeclaresExpectedAttributeUsage(Type attributeType, AttributeTargets expectedTargets)
        {
            var usage = attributeType.GetCustomAttribute<AttributeUsageAttribute>();

            Assert.NotNull(usage);
            Assert.Equal(expectedTargets, usage!.ValidOn);
        }

        [Fact]
        public void ExposesExpectedBricksAttributeSet()
        {
            var attributeNames = typeof(RoleAttribute).Assembly
                .GetTypes()
                .Where(type => type.Namespace == typeof(RoleAttribute).Namespace)
                .Where(type => type.Name.EndsWith("Attribute", StringComparison.Ordinal))
                .Where(type => typeof(Attribute).IsAssignableFrom(type))
                .Select(type => type.Name)
                .OrderBy(name => name)
                .ToArray();

            Assert.Equal(new[]
            {
                nameof(RoleAliasAttribute),
                nameof(RoleAttribute),
                nameof(RuleAttribute)
            }, attributeNames);
        }

        [Fact]
        public void RoleAttributeExposesRoleName()
        {
            var role = new RoleAttribute("Application");
            var discovered = typeof(DomainType).GetCustomAttribute<RoleAttribute>();

            Assert.Equal("Application", role.Name);
            Assert.NotNull(discovered);
            Assert.Equal("Domain", discovered!.Name);
        }

        [Fact]
        public void RoleAliasAttributeExposesMappedRole()
        {
            var alias = new RoleAliasAttribute("Infrastructure");
            var discovered = typeof(DomainAliasAttribute).GetCustomAttribute<RoleAliasAttribute>();

            Assert.Equal("Infrastructure", alias.Role);
            Assert.NotNull(discovered);
            Assert.Equal("Domain", discovered!.Role);
        }

        [Fact]
        public void RuleAttributeExposesConfiguration()
        {
            var rule = new RuleAttribute(
                "BRK-100",
                "Domain",
                "Infrastructure",
                RuleMode.RequireDependency,
                "custom {source} {target}",
                excludedSourceNameContains: "Legacy",
                excludedTargetNameContains: "Adapter",
                excludedMemberNameContains: "Allow",
                requiredSourceNameContains: "Billing",
                requiredTargetNameContains: "Port");

            Assert.Equal("BRK-100", rule.Id);
            Assert.Equal("Domain", rule.SourceRole);
            Assert.Equal("Infrastructure", rule.TargetRole);
            Assert.Equal(RuleMode.RequireDependency, rule.Mode);
            Assert.Equal("custom {source} {target}", rule.Message);
            Assert.Equal("Legacy", rule.ExcludedSourceNameContains);
            Assert.Equal("Adapter", rule.ExcludedTargetNameContains);
            Assert.Equal("Allow", rule.ExcludedMemberNameContains);
            Assert.Equal("Billing", rule.RequiredSourceNameContains);
            Assert.Equal("Port", rule.RequiredTargetNameContains);
        }

        [Fact]
        public void BrickAttributesCanBeSpecializedForClearNaming()
        {
            var specializedRole = typeof(SpecializedDomainType).GetCustomAttribute<BillingDomainRoleAttribute>();
            var specializedAlias = typeof(SpecializedInfrastructureAliasMarker).GetCustomAttribute<BillingInfrastructureAliasAttribute>();
            var specializedRule = new BillingRuleAttribute(BillingRoles.Domain, BillingRoles.Infrastructure);

            Assert.NotNull(specializedRole);
            Assert.Equal(BillingRoles.Domain, specializedRole!.Name);
            Assert.NotNull(specializedAlias);
            Assert.Equal(BillingRoles.Infrastructure, specializedAlias!.Role);
            Assert.Equal(BillingRules.DomainMustNotDependOnInfrastructure, specializedRule.Id);
            Assert.Equal(BillingRoles.Domain, specializedRule.SourceRole);
            Assert.Equal(BillingRoles.Infrastructure, specializedRule.TargetRole);
        }

        [Fact]
        public void AssemblyLevelBrickRulesAreDiscoverable()
        {
            var assembly = typeof(AttributesTest).Assembly;
            var rules = assembly.GetCustomAttributes<RuleAttribute>().ToArray();

            Assert.Single(rules);
            Assert.Equal("BRK-001", rules[0].Id);
            Assert.Equal("Domain", rules[0].SourceRole);
            Assert.Equal("Infrastructure", rules[0].TargetRole);
            Assert.Equal(RuleMode.ForbidDependency, rules[0].Mode);
        }
    }
}
