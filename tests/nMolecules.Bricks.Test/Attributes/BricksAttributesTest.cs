using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

[assembly: NMolecules.Bricks.Rule("BRK-001", "Domain", "Infrastructure", NMolecules.Bricks.RuleMode.ForbidDependency, "Domain must not use infrastructure")]

namespace NMolecules.Bricks.Test
{
    /// <summary>
    /// Central role names used throughout the brick attribute tests so that the
    /// assertions exercise reusable, domain-specific constants instead of literals.
    /// </summary>
    public static class BillingRoles
    {
        public const string Domain = "Billing.Domain";
        public const string Infrastructure = "Billing.Infrastructure";
        public static RoleId DomainId => RoleId.From(Domain);
        public static RoleId InfrastructureId => RoleId.From(Infrastructure);
    }

    /// <summary>
    /// Custom rule identifiers used to demonstrate specialization of the generic
    /// brick rule model into domain-specific naming.
    /// </summary>
    public static class BillingRules
    {
        public const string DomainMustNotDependOnInfrastructure = "BILL-CUSTOM-001";
        public static RuleId DomainMustNotDependOnInfrastructureId =>
            RuleId.From(DomainMustNotDependOnInfrastructure);
    }

    /// <summary>
    /// Example of a specialized role attribute that derives from the generic
    /// brick role abstraction but exposes a clearer domain-specific name.
    /// </summary>
    public class BillingDomainRoleAttribute : RoleAttribute
    {
        public BillingDomainRoleAttribute() : base(BillingRoles.DomainId)
        {
        }
    }

    /// <summary>
    /// Example of a specialized role-alias attribute that maps a custom marker
    /// type to an existing infrastructure role.
    /// </summary>
    public class BillingInfrastructureAliasAttribute : RoleAliasAttribute
    {
        public BillingInfrastructureAliasAttribute() : base(BillingRoles.InfrastructureId)
        {
        }
    }

    /// <summary>
    /// Example of a specialized rule attribute that turns a reusable generic
    /// rule primitive into a project-specific semantic name.
    /// </summary>
    public class BillingRuleAttribute : RuleAttribute
    {
        public BillingRuleAttribute(RoleId sourceRole, RoleId targetRole)
            : base(BillingRules.DomainMustNotDependOnInfrastructureId, sourceRole, targetRole)
        {
        }
    }

    /// <summary>
    /// Example of a specialized rule attribute that groups optional filter fields
    /// into a dedicated value object.
    /// </summary>
    public class BillingFilteredRuleAttribute : RuleAttribute
    {
        public BillingFilteredRuleAttribute(RoleId sourceRole, RoleId targetRole, params RuleFilter[] filters)
            : base(BillingRules.DomainMustNotDependOnInfrastructureId, sourceRole, targetRole, filters)
        {
        }
    }

    /// <summary>
    /// Example of a specialized rule attribute that groups the custom message
    /// template into a dedicated value object.
    /// </summary>
    public class BillingMessageRuleAttribute : RuleAttribute
    {
        public BillingMessageRuleAttribute(RoleId sourceRole, RoleId targetRole, RuleMessage message)
            : base(BillingRules.DomainMustNotDependOnInfrastructureId, sourceRole, targetRole, message)
        {
        }
    }

    public class EmptyRoleAttribute : RoleAttribute
    {
        public EmptyRoleAttribute() : base()
        {
        }
    }

    public class EmptyRoleAliasAttribute : RoleAliasAttribute
    {
        public EmptyRoleAliasAttribute() : base()
        {
        }
    }

    public class EmptyRuleAttribute : RuleAttribute
    {
        public EmptyRuleAttribute() : base()
        {
        }
    }

    public class BillingMessageFilteredRuleAttribute : RuleAttribute
    {
        public BillingMessageFilteredRuleAttribute(RoleId sourceRole, RoleId targetRole, RuleMessage message, params RuleFilter[] filters)
            : base(BillingRules.DomainMustNotDependOnInfrastructureId, sourceRole, targetRole, message, filters)
        {
        }
    }

    /// <summary>
    /// Simple alias marker used to prove that role aliases can be declared on
    /// custom attribute types and then consumed by other annotated types.
    /// </summary>
    [RoleAlias("Domain")]
    public class DomainAliasAttribute : Attribute
    {
    }

    /// <summary>
    /// Sample type that carries both a direct role assignment and a role alias,
    /// allowing the tests to validate discovery of both metadata forms.
    /// </summary>
    [Role("Domain")]
    [DomainAlias]
    public class DomainType
    {
    }

    /// <summary>
    /// Sample type used to verify that specialized role attributes remain
    /// discoverable exactly like the generic base attribute.
    /// </summary>
    [BillingDomainRole]
    public class SpecializedDomainType
    {
    }

    /// <summary>
    /// Sample alias marker used to verify that a specialized role alias can be
    /// discovered and still exposes the configured target role.
    /// </summary>
    [BillingInfrastructureAlias]
    public class SpecializedInfrastructureAliasMarker : Attribute
    {
    }

    /// <summary>
    /// Verifies the generic brick attribute surface, the supported attribute
    /// targets, and the specialization model used for custom rule families.
    /// </summary>
    public class AttributesTest
    {
        /// <summary>
        /// Provides the expected usage mask for the public brick attributes so
        /// contract changes are detected immediately.
        /// </summary>
        public static TheoryData<Type, AttributeTargets> AttributeTargetsData => new()
        {
            { typeof(RoleAttribute), AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(RoleAliasAttribute), AttributeTargets.Class },
            { typeof(RuleAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class },
            { typeof(RuleFilterAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class }
        };

        /// <summary>
        /// Ensures that the declared target set of each brick attribute matches
        /// the public contract assumed by analyzers and documentation.
        /// </summary>
        [Theory]
        [MemberData(nameof(AttributeTargetsData))]
        public void DeclaresExpectedAttributeUsage(Type attributeType, AttributeTargets expectedTargets)
        {
            var usage = attributeType.GetCustomAttribute<AttributeUsageAttribute>();

            Assert.NotNull(usage);
            Assert.Equal(expectedTargets, usage!.ValidOn);
        }

        /// <summary>
        /// Guards the exported brick attribute set against accidental public API
        /// drift such as missing, renamed, or newly added attributes.
        /// </summary>
        [Fact]
        public void ExposesExpectedBricksAttributeSet()
        {
            var attributeNames = typeof(RoleAttribute).Assembly
                .GetTypes()
                .Where(type => type.Namespace == typeof(RoleAttribute).Namespace)
                .Where(type => type.Name.EndsWith("Attribute", StringComparison.Ordinal))
                .Where(type => typeof(Attribute).IsAssignableFrom(type))
                .Select(type => type.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(new[]
            {
                nameof(ExcludedMemberNameContainsAttribute),
                nameof(ExcludedSourceNameContainsAttribute),
                nameof(ExcludedTargetNameContainsAttribute),
                nameof(RequireAllMembersAttribute),
                nameof(RequireExactlyOneMemberAttribute),
                nameof(RequireExclusiveChoiceAttribute),
                nameof(RequireMemberCountAttribute),
                nameof(RequiredSourceNameContainsAttribute),
                nameof(RequiredTargetNameContainsAttribute),
                nameof(RoleAliasAttribute),
                nameof(RoleAttribute),
                nameof(RuleAttribute),
                nameof(RuleFilterAttribute)
            }, attributeNames);
        }

        /// <summary>
        /// Verifies both direct construction and reflection-based discovery of
        /// role assignments declared through <see cref="RoleAttribute"/>.
        /// </summary>
        [Fact]
        public void RoleAttributeExposesRoleName()
        {
            var role = new RoleAttribute("Application");
            var discovered = typeof(DomainType).GetCustomAttribute<RoleAttribute>();

            Assert.Equal("Application", role.Name);
            Assert.Equal(RoleId.From("Application"), role.Id);
            Assert.NotNull(discovered);
            Assert.Equal("Domain", discovered!.Name);
            Assert.Equal(RoleId.From("Domain"), discovered.Id);
        }

        /// <summary>
        /// Verifies both direct construction and reflection-based discovery of
        /// role aliases declared through <see cref="RoleAliasAttribute"/>.
        /// </summary>
        [Fact]
        public void RoleAliasAttributeExposesMappedRole()
        {
            var alias = new RoleAliasAttribute("Infrastructure");
            var discovered = typeof(DomainAliasAttribute).GetCustomAttribute<RoleAliasAttribute>();

            Assert.Equal("Infrastructure", alias.Role);
            Assert.Equal(RoleId.From("Infrastructure"), alias.RoleId);
            Assert.NotNull(discovered);
            Assert.Equal("Domain", discovered!.Role);
            Assert.Equal(RoleId.From("Domain"), discovered.RoleId);
        }

        /// <summary>
        /// Verifies that role names can also be modeled through a typed wrapper
        /// for regular code, catalogs, and helper APIs outside attribute syntax.
        /// </summary>
        [Fact]
        public void RoleIdProvidesTypedWrapperForRoleNames()
        {
            var direct = new RoleId("Billing.Domain");
            var factory = RoleId.From("Billing.Domain");

            Assert.Equal(direct, factory);
            Assert.Equal("Billing.Domain", direct.Value);
            Assert.False(direct.IsEmpty);
            Assert.Equal(BillingRoles.Domain, BillingRoles.DomainId);
            Assert.Equal(BillingRoles.Infrastructure, BillingRoles.InfrastructureId);
        }

        /// <summary>
        /// Verifies that the generic rule attribute preserves its runtime
        /// configuration, including mode, message template, and typed ids.
        /// </summary>
        [Fact]
        public void RuleAttributeExposesConfiguration()
        {
            var rule = new RuleAttribute(
                "BRK-100",
                "Domain",
                "Infrastructure",
                RuleMode.RequireDependency,
                "custom {source} {target}");

            Assert.Equal("BRK-100", rule.Id);
            Assert.Equal(RuleId.From("BRK-100"), rule.RuleId);
            Assert.Equal("Domain", rule.SourceRole);
            Assert.Equal("Infrastructure", rule.TargetRole);
            Assert.Equal(RoleId.From("Domain"), rule.SourceRoleId);
            Assert.Equal(RoleId.From("Infrastructure"), rule.TargetRoleId);
            Assert.Equal(RuleMode.RequireDependency, rule.Mode);
            Assert.Equal("custom {source} {target}", rule.Message);
            Assert.Equal(RuleMessage.From("custom {source} {target}"), rule.MessageTemplate);
            Assert.Empty(rule.Filters);
        }

        /// <summary>
        /// Verifies that specialized rule filters can be passed through a params
        /// list when deriving a more specific brick rule attribute.
        /// </summary>
        [Fact]
        public void RuleFiltersCanBeSpecializedAndPassedAsParams()
        {
            var excludedSource = new ExcludedSourceNameContainsRuleFilter("Legacy", "Generated");
            var excludedTarget = new ExcludedTargetNameContainsRuleFilter("Facade");
            var excludedMember = new ExcludedMemberNameContainsRuleFilter("Allow");
            var requiredSource = new RequiredSourceNameContainsRuleFilter("Contract");
            var requiredTarget = new RequiredTargetNameContainsRuleFilter("Repository");
            var rule = new BillingFilteredRuleAttribute(
                BillingRoles.DomainId,
                BillingRoles.InfrastructureId,
                excludedSource,
                excludedTarget,
                excludedMember,
                requiredSource,
                requiredTarget);

            Assert.False(excludedSource.IsEmpty);
            Assert.Equal(new[] { "Legacy", "Generated" }, excludedSource.Tokens);
            Assert.Equal(new[] { "Facade" }, excludedTarget.Tokens);
            Assert.Equal(new[] { "Allow" }, excludedMember.Tokens);
            Assert.Equal(new[] { "Contract" }, requiredSource.Tokens);
            Assert.Equal(new[] { "Repository" }, requiredTarget.Tokens);
            Assert.Collection(
                rule.Filters,
                filter => Assert.Equal(excludedSource, filter),
                filter => Assert.Equal(excludedTarget, filter),
                filter => Assert.Equal(excludedMember, filter),
                filter => Assert.Equal(requiredSource, filter),
                filter => Assert.Equal(requiredTarget, filter));
        }

        /// <summary>
        /// Verifies that the dedicated rule-filter attributes bind to a concrete
        /// rule identifier and expose the same typed filter objects as runtime code.
        /// </summary>
        [Fact]
        public void RuleFilterAttributesExposeRuleBindingAndTypedFilters()
        {
            RuleFilterAttribute[] attributes =
            {
                new ExcludedSourceNameContainsAttribute("BILL-200", "Legacy", "Generated"),
                new ExcludedTargetNameContainsAttribute("BILL-200", "Facade"),
                new ExcludedMemberNameContainsAttribute("BILL-200", "Allow"),
                new RequiredSourceNameContainsAttribute("BILL-200", "Contract"),
                new RequiredTargetNameContainsAttribute("BILL-200", "Repository")
            };

            Assert.All(attributes, attribute => Assert.Equal(RuleId.From("BILL-200"), attribute.RuleId));
            Assert.Collection(
                attributes.Select(attribute => attribute.ToFilter()).ToArray(),
                filter => Assert.Equal(new ExcludedSourceNameContainsRuleFilter("Legacy", "Generated"), filter),
                filter => Assert.Equal(new ExcludedTargetNameContainsRuleFilter("Facade"), filter),
                filter => Assert.Equal(new ExcludedMemberNameContainsRuleFilter("Allow"), filter),
                filter => Assert.Equal(new RequiredSourceNameContainsRuleFilter("Contract"), filter),
                filter => Assert.Equal(new RequiredTargetNameContainsRuleFilter("Repository"), filter));
        }

        /// <summary>
        /// Verifies that grouped rule messages can be assembled through a fluent
        /// builder and passed through a typed value object.
        /// </summary>
        [Fact]
        public void RuleMessageBuilderCreatesTypedMessageTemplates()
        {
            var message = RuleMessage.Builder()
                .Text("Rule ")
                .Rule()
                .Text(": ")
                .Source()
                .Text(" must not depend on ")
                .Target()
                .Text(" via ")
                .Member()
                .Text(".")
                .Build();
            var rule = new BillingMessageRuleAttribute(BillingRoles.DomainId, BillingRoles.InfrastructureId, message);

            Assert.False(message.IsEmpty);
            Assert.True(message.UsesRulePlaceholder);
            Assert.True(message.UsesSourcePlaceholder);
            Assert.True(message.UsesTargetPlaceholder);
            Assert.True(message.UsesMemberPlaceholder);
            Assert.Equal("Rule {rule}: {source} must not depend on {target} via {member}.", message.Value);
            Assert.Equal(message, rule.MessageTemplate);
            Assert.Equal(message.Value, rule.Message);
        }

        /// <summary>
        /// Verifies the intended extension model where domain-specific marker
        /// names derive from the generic brick attributes without losing metadata.
        /// </summary>
        [Fact]
        public void BrickAttributesCanBeSpecializedForClearNaming()
        {
            var specializedRole = typeof(SpecializedDomainType).GetCustomAttribute<BillingDomainRoleAttribute>();
            var specializedAlias = typeof(SpecializedInfrastructureAliasMarker).GetCustomAttribute<BillingInfrastructureAliasAttribute>();
            var specializedRule = new BillingRuleAttribute(BillingRoles.DomainId, BillingRoles.InfrastructureId);

            Assert.NotNull(specializedRole);
            Assert.Equal(BillingRoles.Domain, specializedRole!.Name);
            Assert.NotNull(specializedAlias);
            Assert.Equal(BillingRoles.Infrastructure, specializedAlias!.Role);
            Assert.Equal(BillingRules.DomainMustNotDependOnInfrastructure, specializedRule.Id);
            Assert.Equal(BillingRules.DomainMustNotDependOnInfrastructureId, specializedRule.RuleId);
            Assert.Equal(BillingRoles.Domain, specializedRule.SourceRole);
            Assert.Equal(BillingRoles.Infrastructure, specializedRule.TargetRole);
            Assert.Equal(BillingRoles.DomainId, specializedRole.Id);
            Assert.Equal(BillingRoles.InfrastructureId, specializedAlias.RoleId);
            Assert.Equal(BillingRoles.DomainId, specializedRule.SourceRoleId);
            Assert.Equal(BillingRoles.InfrastructureId, specializedRule.TargetRoleId);
        }

        /// <summary>
        /// Verifies that assembly-level brick rules are emitted into metadata and
        /// can be discovered by reflection-based tooling.
        /// </summary>
        [Fact]
        public void AssemblyLevelBrickRulesAreDiscoverable()
        {
            var assembly = typeof(AttributesTest).Assembly;
            var rules = assembly.GetCustomAttributes<RuleAttribute>().ToArray();

            Assert.Single(rules);
            Assert.Equal("BRK-001", rules[0].Id);
            Assert.Equal(RuleId.From("BRK-001"), rules[0].RuleId);
            Assert.Equal("Domain", rules[0].SourceRole);
            Assert.Equal("Infrastructure", rules[0].TargetRole);
            Assert.Equal(RoleId.From("Domain"), rules[0].SourceRoleId);
            Assert.Equal(RoleId.From("Infrastructure"), rules[0].TargetRoleId);
            Assert.Equal(RuleMode.ForbidDependency, rules[0].Mode);
        }

        [Fact]
        public void ProtectedAttributeConstructorsNormalizeToEmptyValues()
        {
            var role = new EmptyRoleAttribute();
            var alias = new EmptyRoleAliasAttribute();
            var rule = new EmptyRuleAttribute();

            Assert.Equal(string.Empty, role.Name);
            Assert.True(role.Id.IsEmpty);
            Assert.Equal(string.Empty, alias.Role);
            Assert.True(alias.RoleId.IsEmpty);
            Assert.Equal(string.Empty, rule.Id);
            Assert.Equal(string.Empty, rule.SourceRole);
            Assert.Equal(string.Empty, rule.TargetRole);
            Assert.Equal(RuleMode.ForbidDependency, rule.Mode);
            Assert.Equal(string.Empty, rule.Message);
            Assert.Empty(rule.Filters);
        }

        [Fact]
        public void NullAttributeInputsNormalizeToEmptyValues()
        {
            var role = new RoleAttribute(null);
            var alias = new RoleAliasAttribute(null);
            var rule = new RuleAttribute(null, null, null, RuleMode.ForbidDependency, null);
            var filterAttribute = new ExcludedSourceNameContainsAttribute(null, null, "  ", "Legacy");

            Assert.True(role.Id.IsEmpty);
            Assert.True(alias.RoleId.IsEmpty);
            Assert.True(rule.RuleId.IsEmpty);
            Assert.True(rule.SourceRoleId.IsEmpty);
            Assert.True(rule.TargetRoleId.IsEmpty);
            Assert.True(rule.MessageTemplate.IsEmpty);
            Assert.Equal(string.Empty, filterAttribute.Rule);
            Assert.Equal(new[] { "Legacy" }, filterAttribute.Tokens);
        }

        [Fact]
        public void MemberCardinalityAttributesNormalizeNullTypes()
        {
            var exactlyOne = new RequireExactlyOneMemberAttribute(null);
            var all = new RequireAllMembersAttribute(null);
            var count = new RequireMemberCountAttribute(null, 2);
            var exclusive = new RequireExclusiveChoiceAttribute(null, null);

            Assert.Equal(typeof(Attribute), exactlyOne.MemberAttributeType);
            Assert.Empty(all.MemberAttributeTypes);
            Assert.Equal(typeof(Attribute), count.MemberAttributeType);
            Assert.Equal(2, count.Count);
            Assert.Equal(typeof(Attribute), exclusive.LeftMemberAttributeType);
            Assert.Equal(typeof(Attribute), exclusive.RightMemberAttributeType);
        }

        [Fact]
        public void RuleIdRoleIdAndRuleMessageExposeFullValueSemantics()
        {
            RuleId ruleId = "BRK-SEM";
            RoleId roleId = "Domain";
            RuleMessage message = "Rule {rule} uses {source}";
            string rawRule = ruleId;
            string rawRole = roleId;
            string rawMessage = message;

            Assert.Equal("BRK-SEM", RuleId.From("BRK-SEM").ToString());
            Assert.Equal("Domain", RoleId.From("Domain").ToString());
            Assert.Equal("Rule {rule} uses {source}", RuleMessage.From("Rule {rule} uses {source}").ToString());
            Assert.Equal("BRK-SEM", rawRule);
            Assert.Equal("Domain", rawRole);
            Assert.Equal("Rule {rule} uses {source}", rawMessage);
            Assert.True(ruleId.Equals((object)RuleId.From("BRK-SEM")));
            Assert.True(roleId.Equals((object)RoleId.From("Domain")));
            Assert.True(message.Equals((object)RuleMessage.From("Rule {rule} uses {source}")));
            Assert.False(ruleId.Equals((object)"BRK-SEM"));
            Assert.False(roleId.Equals((object)"Domain"));
            Assert.False(message.Equals((object)"Rule {rule} uses {source}"));
            Assert.Equal(RuleId.From("BRK-SEM").GetHashCode(), ruleId.GetHashCode());
            Assert.Equal(RoleId.From("Domain").GetHashCode(), roleId.GetHashCode());
            Assert.Equal(RuleMessage.From("Rule {rule} uses {source}").GetHashCode(), message.GetHashCode());
            Assert.Equal(0, default(RuleId).GetHashCode());
            Assert.Equal(0, default(RoleId).GetHashCode());
            Assert.Equal(0, default(RuleMessage).GetHashCode());
            Assert.True(ruleId == RuleId.From("BRK-SEM"));
            Assert.True(roleId == RoleId.From("Domain"));
            Assert.True(message == RuleMessage.From("Rule {rule} uses {source}"));
            Assert.True(ruleId != RuleId.From("BRK-OTHER"));
            Assert.True(roleId != RoleId.From("Infrastructure"));
            Assert.True(message != RuleMessage.From("Other"));
            Assert.True(message.UsesRulePlaceholder);
            Assert.True(message.UsesSourcePlaceholder);
            Assert.False(message.UsesTargetPlaceholder);
            Assert.False(message.UsesMemberPlaceholder);
            Assert.False(RuleMessage.Empty.UsesRulePlaceholder);
        }

        [Fact]
        public void RuleFiltersExposeEqualityOperatorsAndEmptyNormalization()
        {
            var empty = new ExcludedSourceNameContainsRuleFilter(null, " ", "");
            var nullParams = new ExcludedTargetNameContainsRuleFilter((string[])null);
            var left = new ExcludedSourceNameContainsRuleFilter(" Legacy ");
            var same = new ExcludedSourceNameContainsRuleFilter("Legacy");
            var differentType = new ExcludedTargetNameContainsRuleFilter("Legacy");

            Assert.True(empty.IsEmpty);
            Assert.True(nullParams.IsEmpty);
            Assert.Equal(string.Empty, empty.Value);
            Assert.True(left == same);
            Assert.False(left != same);
            Assert.False(left == differentType);
            Assert.True(left != differentType);
            Assert.True(left != null);
            Assert.False(left == null);
            Assert.False((RuleFilter)null == left);
            Assert.True((RuleFilter)null == (RuleFilter)null);
            Assert.False((RuleFilter)null != (RuleFilter)null);
            Assert.True(left.Equals((object)same));
            Assert.False(left.Equals((object)"Legacy"));
            Assert.Equal(left.GetHashCode(), same.GetHashCode());
            var uninitialized = (ExcludedSourceNameContainsRuleFilter)RuntimeHelpers.GetUninitializedObject(typeof(ExcludedSourceNameContainsRuleFilter));
            Assert.NotEqual(0, uninitialized.GetHashCode());
        }

        [Fact]
        public void SpecializedRuleConstructorsCloneFiltersAndTypedMessages()
        {
            var filter = new ExcludedSourceNameContainsRuleFilter("Legacy");
            var message = RuleMessage.From("Typed message");
            var rule = new BillingMessageFilteredRuleAttribute(BillingRoles.DomainId, BillingRoles.InfrastructureId, message, filter);
            var nullFiltersRule = new BillingMessageFilteredRuleAttribute(BillingRoles.DomainId, BillingRoles.InfrastructureId, message, (RuleFilter[])null);
            var filters = rule.Filters;

            filters[0] = new ExcludedTargetNameContainsRuleFilter("Other");

            Assert.Equal(message, rule.MessageTemplate);
            Assert.Equal(RuleMode.ForbidDependency, rule.Mode);
            Assert.Equal(filter, rule.Filters.Single());
            Assert.Empty(nullFiltersRule.Filters);
        }
    }
}
