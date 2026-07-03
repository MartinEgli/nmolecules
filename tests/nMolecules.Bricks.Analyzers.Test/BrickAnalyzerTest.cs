using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NMolecules.Bricks.Analyzers;
using Xunit;

namespace NMolecules.Bricks.Analyzers.Test
{
    public class BrickAnalyzerTest
    {
        [Fact]
        public async Task MetadataAnalyzerReportsInvalidAttributeConfiguration()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

[assembly: Policy("""")]
[assembly: Rule("""", """", """")]
[assembly: Dependency("""", """", """", """")]

[Role("""")]
[RequireMemberCount(typeof(IdentityAttribute), -1)]
[RequireMemberRange(typeof(IdentityAttribute), 3, 2)]
[ForbidMember(null)]
[RequireNamedMembers(null)]
[RequireUniqueNamedMember(null, """")]
public class EmptyRole;

public sealed class IdentityAttribute : Attribute;
");

            Assert.Equal(
                new[]
                {
                    "XMoleculesBricks0200",
                    "XMoleculesBricks0201",
                    "XMoleculesBricks0202",
                    "XMoleculesBricks0202",
                    "XMoleculesBricks0202",
                    "XMoleculesBricks0203",
                    "XMoleculesBricks0203",
                    "XMoleculesBricks0203",
                    "XMoleculesBricks0203",
                    "XMoleculesBricks0205",
                    "XMoleculesBricks0205",
                    "XMoleculesBricks0205",
                    "XMoleculesBricks0205",
                    "XMoleculesBricks0205",
                    "XMoleculesBricks0205",
                    "XMoleculesBricks0205"
                },
                diagnostics.Select(diagnostic => diagnostic.Id).OrderBy(id => id).ToArray());

            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.Id == "XMoleculesBricks0205" &&
                    diagnostic.Properties["BrickConfigurationKind"] == "MemberContract");
        }

        [Fact]
        public async Task DependencyRuleAnalyzerReportsForbiddenAndMissingRequiredDependencies()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""DDD001"", ""Domain"", ""Infrastructure"", RuleMode.ForbidDependency)]
[assembly: Rule(""DDD002"", ""Application"", ""Repository"", RuleMode.RequireDependency)]

[Role(""Domain"")]
public sealed class OrderAggregate
{
    private readonly SqlOrderRepository _repository = default!;
}

[Role(""Infrastructure"")]
public sealed class SqlOrderRepository;

[Role(""Application"")]
public sealed class SubmitOrderHandler;

[Role(""Application"")]
public sealed class ApproveOrderHandler
{
    private readonly IOrderRepository _repository = default!;
}

[Role(""Repository"")]
public interface IOrderRepository;
");

            Assert.Equal(
                new[] { "XMoleculesBricks0001", "XMoleculesBricks0001" },
                diagnostics.Select(diagnostic => diagnostic.Id).OrderBy(id => id).ToArray());

            var forbidden = Assert.Single(diagnostics, diagnostic => diagnostic.GetMessage().Contains("forbids dependency"));
            Assert.Equal("ForbiddenDependency", forbidden.Properties["BrickViolationKind"]);
            Assert.Equal("DDD001", forbidden.Properties["RuleId"]);
            Assert.Equal("Domain", forbidden.Properties["SourceRole"]);
            Assert.Equal("Infrastructure", forbidden.Properties["TargetRole"]);

            var required = Assert.Single(diagnostics, diagnostic => diagnostic.GetMessage().Contains("requires 'SubmitOrderHandler'"));
            Assert.Equal("RequiredDependencyMissing", required.Properties["BrickViolationKind"]);
            Assert.Equal("DDD002", required.Properties["RuleId"]);
        }

        [Fact]
        public async Task DependencyRuleAnalyzerReportsSelfDependencies()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[Role(""Node"")]
public sealed class SelfDependentNode
{
    private readonly SelfDependentNode _parent = default!;
}
");

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal("XMoleculesBricks0001", diagnostic.Id);
            Assert.Contains("source and target must not be the same element 'SelfDependentNode'", diagnostic.GetMessage());
            Assert.Equal("SelfDependency", diagnostic.Properties["BrickViolationKind"]);
        }

        [Fact]
        public async Task DependencyRuleAnalyzerReportsDeclaredSelfDependencies()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Dependency(""SELF001"", ""SelfDeclaredNode"", ""SelfDeclaredNode"", BrickDependencyKinds.TypeReference)]

[Role(""Node"")]
public sealed class SelfDeclaredNode;
");

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal("XMoleculesBricks0001", diagnostic.Id);
            Assert.Contains("source and target must not be the same element 'SelfDeclaredNode'", diagnostic.GetMessage());
            Assert.Equal("SelfDependency", diagnostic.Properties["BrickViolationKind"]);
        }

        [Fact]
        public async Task MetadataAnalyzerAcceptsExplicitSinglePolicyImportOwner()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""BRK-POLICY"")]
[assembly: PolicyImport(""BRK-BASE"", ""BRK-POLICY"", BrickPolicyImportMode.Extend)]
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task MetadataAnalyzerAcceptsExplicitPolicyImportOwner()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""BRK-A"")]
[assembly: Policy(""BRK-B"")]
[assembly: PolicyImport(""BRK-BASE"", ""BRK-A"", BrickPolicyImportMode.Extend)]
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task MetadataAnalyzerReportsEmptyPolicyImportId()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""BRK-A"")]
[assembly: PolicyImport("""", ""BRK-A"", BrickPolicyImportMode.Extend)]
");

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal("XMoleculesBricks0200", diagnostic.Id);
            Assert.Contains("imported policy id", diagnostic.GetMessage());
            Assert.Equal("PolicyImport", diagnostic.Properties["BrickConfigurationKind"]);
        }

        [Fact]
        public async Task MetadataAnalyzerReportsExplicitEmptyPolicyImportOwner()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: PolicyImport(""BRK-BASE"", """", BrickPolicyImportMode.Extend)]
");

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal("XMoleculesBricks0200", diagnostic.Id);
            Assert.Equal("PolicyImport", diagnostic.Properties["BrickConfigurationKind"]);
            Assert.Contains("owner policy id", diagnostic.GetMessage());
        }

        [Fact]
        public async Task MetadataAnalyzerReportsDuplicateRulesAndEffectiveRoles()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

[assembly: Rule(""DUP001"", ""Domain"", ""Infrastructure"", RuleMode.ForbidDependency)]
[assembly: Rule(""DUP001"", ""Domain"", ""Infrastructure"", RuleMode.ForbidDependency)]
[assembly: Rule(""DUP002"", ""Application"", ""Repository"", RuleMode.RequireDependency)]
[assembly: Rule(""DUP003"", ""Application"", ""Repository"", RuleMode.RequireDependency)]
[assembly: Rule(""DUP004"", ""ConflictingSource"", ""ConflictingTarget"", RuleMode.ForbidDependency)]
[assembly: Rule(""DUP005"", ""ConflictingSource"", ""ConflictingTarget"", RuleMode.RequireDependency)]

[AttributeUsage(AttributeTargets.Class)]
[RoleAlias(""Domain"")]
public sealed class DomainRoleAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
[RoleAlias(""Domain"")]
public sealed class AlsoDomainRoleAttribute : Attribute;

[DomainRole]
[AlsoDomainRole]
public sealed class DuplicateDomainRoles;
");

            Assert.Equal(
                new[] { "XMoleculesBricks0201", "XMoleculesBricks0202", "XMoleculesBricks0202", "XMoleculesBricks0202" },
                diagnostics.Select(diagnostic => diagnostic.Id).OrderBy(id => id).ToArray());
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "RuleAttribute id 'DUP001' is declared more than once");
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "RuleAttribute for 'Application' to 'Repository' with mode 'RequireDependency' is declared more than once");
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "RuleAttribute for 'ConflictingSource' to 'ConflictingTarget' is declared with conflicting modes 'ForbidDependency' and 'RequireDependency'");
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "Role 'Domain' is assigned more than once to 'DuplicateDomainRoles'");
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "RuleAttribute id 'DUP001' is declared more than once" &&
                    diagnostic.Properties["BrickConfigurationKind"] == "Rule" &&
                    diagnostic.Properties["RuleId"] == "DUP001");
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "Role 'Domain' is assigned more than once to 'DuplicateDomainRoles'" &&
                    diagnostic.Properties["BrickConfigurationKind"] == "Role");
        }

        [Fact]
        public async Task MetadataAnalyzerReportsRoleCombinationAndRuleFilterConflicts()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: RoleCombination(""combo-additive"", ""Domain"", ""Infrastructure"", BrickCombinationKind.Additive)]
[assembly: RoleCombination(""combo-incompatible"", ""Infrastructure"", ""Domain"", BrickCombinationKind.Incompatible)]

[assembly: RequiredSourceNameContains(""RULE-FILTER"", ""Generated"")]
[assembly: ExcludedSourceNameContains(""RULE-FILTER"", ""Generated"")]
[assembly: RequiredTargetNameContains(""RULE-FILTER"", ""Legacy"")]
[assembly: ExcludedTargetNameContains(""RULE-FILTER"", ""Legacy"")]
");

            Assert.Equal(
                new[] { "XMoleculesBricks0201", "XMoleculesBricks0204", "XMoleculesBricks0204" },
                diagnostics.Select(diagnostic => diagnostic.Id).OrderBy(id => id).ToArray());
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "RoleCombinationAttribute for 'Infrastructure' and 'Domain' is declared with conflicting kinds 'Additive' and 'Incompatible'");
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "RuleFilterAttribute for rule 'RULE-FILTER' both requires and excludes source token 'Generated'");
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "RuleFilterAttribute for rule 'RULE-FILTER' both requires and excludes target token 'Legacy'");
        }

        [Fact]
        public async Task MetadataAnalyzerAllowsRoleCombinationPairsAcrossDifferentPolicies()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""BRK-A"")]
[assembly: Policy(""BRK-B"")]
[assembly: RoleCombination(""combo-a"", ""Domain"", ""Infrastructure"", BrickCombinationKind.Additive, policyId: ""BRK-A"")]
[assembly: RoleCombination(""combo-b"", ""Infrastructure"", ""Domain"", BrickCombinationKind.Incompatible, policyId: ""BRK-B"")]
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task MetadataAnalyzerAllowsRoleInSeveralRoleCombinations()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: RoleCombination(""app-domain"", ""Application"", ""Domain"", BrickCombinationKind.Additive)]
[assembly: RoleCombination(""app-infra"", ""Application"", ""Infrastructure"", BrickCombinationKind.Incompatible)]
[assembly: RoleCombination(""app-ui"", ""Application"", ""UI"", BrickCombinationKind.Exclusive)]
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task MetadataAnalyzerAllowsImplicitRoleCombinationPolicyWhenScopeIsUnambiguous()
        {
            var noPolicyDiagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: RoleCombination(""generic-app-domain"", ""Application"", ""Domain"", BrickCombinationKind.Additive)]
");
            var singlePolicyDiagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""BRK-A"")]
[assembly: RoleCombination(""implicit-app-domain"", ""Application"", ""Domain"", BrickCombinationKind.Additive)]
[assembly: RoleCombination(""explicit-app-infra"", ""Application"", ""Infrastructure"", BrickCombinationKind.Incompatible, policyId: ""BRK-A"")]
[assembly: RoleCombination(""self-additive"", ""Domain"", ""Domain"", BrickCombinationKind.Additive, policyId: ""BRK-A"")]
");

            Assert.Empty(noPolicyDiagnostics);
            Assert.Empty(singlePolicyDiagnostics);
        }

        [Fact]
        public async Task MetadataAnalyzerReportsSamePolicyRoleCombinationContradiction()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""BRK-A"")]
[assembly: RoleCombination(""combo-additive"", ""Application"", ""Infrastructure"", BrickCombinationKind.Additive, policyId: ""BRK-A"")]
[assembly: RoleCombination(""combo-incompatible"", ""Infrastructure"", ""Application"", BrickCombinationKind.Incompatible, policyId: ""BRK-A"")]
");

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal("XMoleculesBricks0201", diagnostic.Id);
            Assert.Equal("BRK-A", diagnostic.Properties["PolicyId"]);
            Assert.Equal("RoleCombinationAttribute for 'Infrastructure' and 'Application' is declared with conflicting kinds 'Additive' and 'Incompatible'", diagnostic.GetMessage());
        }

        [Fact]
        public async Task MetadataAnalyzerReportsAmbiguousRoleCombinationPolicyCorrelation()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""BRK-A"")]
[assembly: Policy(""BRK-B"")]
[assembly: RoleCombination(""combo-without-policy"", ""Application"", ""Domain"", BrickCombinationKind.Additive)]
");

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal("XMoleculesBricks0201", diagnostic.Id);
            Assert.Equal("RoleCombination", diagnostic.Properties["BrickConfigurationKind"]);
            Assert.Equal("PolicyId", diagnostic.Properties["CorrelationMode"]);
            Assert.Equal("RoleCombinationAttribute must declare policyId when multiple PolicyAttribute declarations share the same scope", diagnostic.GetMessage());
        }

        [Fact]
        public async Task MetadataAnalyzerAllowsDefaultPolicyForImplicitRoleCombinationPolicy()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""BRK-A"")]
[assembly: Policy(""BRK-B"")]
[assembly: DefaultPolicy(""BRK-A"")]
[assembly: RoleCombination(""combo-with-default-policy"", ""Application"", ""Domain"", BrickCombinationKind.Additive)]
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task MetadataAnalyzerUsesDefaultPolicyForRoleCombinationContradictions()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""BRK-A"")]
[assembly: Policy(""BRK-B"")]
[assembly: DefaultPolicy(""BRK-A"")]
[assembly: RoleCombination(""combo-additive"", ""Application"", ""Infrastructure"", BrickCombinationKind.Additive)]
[assembly: RoleCombination(""combo-incompatible"", ""Infrastructure"", ""Application"", BrickCombinationKind.Incompatible, policyId: ""BRK-A"")]
");

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal("XMoleculesBricks0201", diagnostic.Id);
            Assert.Equal("BRK-A", diagnostic.Properties["PolicyId"]);
            Assert.Equal("RoleCombinationAttribute for 'Infrastructure' and 'Application' is declared with conflicting kinds 'Additive' and 'Incompatible'", diagnostic.GetMessage());
        }

        [Fact]
        public async Task MetadataAnalyzerReportsInvalidDefaultPolicyReference()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""BRK-A"")]
[assembly: DefaultPolicy(""BRK-MISSING"")]
");

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal("XMoleculesBricks0200", diagnostic.Id);
            Assert.Equal("DefaultPolicy", diagnostic.Properties["BrickConfigurationKind"]);
            Assert.Equal("BRK-MISSING", diagnostic.Properties["PolicyId"]);
            Assert.Equal("DefaultPolicyId", diagnostic.Properties["CorrelationMode"]);
            Assert.Equal("DefaultPolicyAttribute policyId 'BRK-MISSING' does not match any PolicyAttribute in the same scope", diagnostic.GetMessage());
        }

        [Fact]
        public async Task MetadataAnalyzerReportsEmptyDefaultPolicyId()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: DefaultPolicy("""")]
");

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal("XMoleculesBricks0200", diagnostic.Id);
            Assert.Equal("DefaultPolicy", diagnostic.Properties["BrickConfigurationKind"]);
            Assert.Equal("DefaultPolicyAttribute must declare a non-empty policy id", diagnostic.GetMessage());
        }

        [Fact]
        public async Task MetadataAnalyzerReportsInvalidRoleCombinationShape()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: RoleCombination("""", """", ""Infrastructure"", BrickCombinationKind.Additive)]
[assembly: RoleCombination(""self-exclusive"", ""Domain"", ""Domain"", BrickCombinationKind.Exclusive)]
[assembly: RoleCombination(""self-incompatible"", ""Application"", ""Application"", BrickCombinationKind.Incompatible)]
");

            Assert.Equal(
                new[] { "XMoleculesBricks0201", "XMoleculesBricks0201", "XMoleculesBricks0201", "XMoleculesBricks0201" },
                diagnostics.Select(diagnostic => diagnostic.Id).OrderBy(id => id).ToArray());
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "RoleCombinationAttribute must declare a non-empty name");
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "RoleCombinationAttribute must declare non-empty left roles");
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "RoleCombinationAttribute 'self-exclusive' cannot declare Exclusive for the same left and right role selector 'Domain'");
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "RoleCombinationAttribute 'self-incompatible' cannot declare Incompatible for the same left and right role selector 'Application'");
        }

        [Fact]
        public async Task MetadataAnalyzerReportsRoleCombinationReferencesAndCycles()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: RoleCombination(""combo-a"", ""combo-b"", ""Domain"", BrickCombinationKind.Additive)]
[assembly: RoleCombination(""combo-b"", ""Application"", ""combo-a"", BrickCombinationKind.Additive)]
");

            Assert.Equal(
                new[] { "XMoleculesBricks0201", "XMoleculesBricks0201" },
                diagnostics.Select(diagnostic => diagnostic.Id).OrderBy(id => id).ToArray());
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "RoleCombinationAttribute 'combo-a' must reference roles in LeftRoles, not another RoleCombinationAttribute 'combo-b'");
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "RoleCombinationAttribute 'combo-b' must reference roles in RightRoles, not another RoleCombinationAttribute 'combo-a'");
        }

        [Fact]
        public async Task MetadataAnalyzerAcceptsPolicyReferencesAcrossMultiplePolicies()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""BRK-A"")]
[assembly: Policy(""BRK-B"")]
[assembly: PolicyImport(""BRK-BASE"", ""BRK-A"", BrickPolicyImportMode.Extend)]
[assembly: Rule(""R-A"", ""Application"", ""Domain"", RuleMode.RequireDependency, policyId: ""BRK-A"")]
[assembly: Dependency(""D-B"", ""ApplicationService"", ""DomainModel"", policyId: ""BRK-B"")]
[assembly: RoleCombination(""combo-b"", ""Application"", ""Domain"", BrickCombinationKind.Additive, policyId: ""BRK-B"")]
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task MetadataAnalyzerReportsContradictoryPolicyReferences()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""BRK-A"")]
[assembly: PolicyImport(""BRK-A"", ""BRK-A"", BrickPolicyImportMode.Extend)]
[assembly: PolicyImport(""BRK-BASE"", ""BRK-MISSING"", BrickPolicyImportMode.Extend)]
[assembly: Rule(""R-MISSING"", ""Application"", ""Domain"", RuleMode.RequireDependency, policyId: ""BRK-MISSING"")]
[assembly: Dependency(""D-MISSING"", ""ApplicationService"", ""DomainModel"", policyId: ""BRK-MISSING"")]
[assembly: RoleCombination(""combo-missing"", ""Application"", ""Domain"", BrickCombinationKind.Additive, policyId: ""BRK-MISSING"")]
");

            Assert.Equal(
                new[] { "XMoleculesBricks0200", "XMoleculesBricks0200", "XMoleculesBricks0201", "XMoleculesBricks0202", "XMoleculesBricks0203" },
                diagnostics.Select(diagnostic => diagnostic.Id).OrderBy(id => id).ToArray());
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "PolicyImportAttribute for owner policy 'BRK-A' must not import itself" &&
                    diagnostic.Properties["BrickConfigurationKind"] == "PolicyImport" &&
                    diagnostic.Properties["PolicyId"] == "BRK-A");
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "PolicyImportAttribute owner policy 'BRK-MISSING' does not match any PolicyAttribute in the same scope");
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "RuleAttribute policyId 'BRK-MISSING' does not match any PolicyAttribute in the same scope" &&
                    diagnostic.Properties["RuleId"] == "R-MISSING");
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "DependencyAttribute policyId 'BRK-MISSING' does not match any PolicyAttribute in the same scope" &&
                    diagnostic.Properties["Source"] == "ApplicationService");
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage() == "RoleCombinationAttribute policyId 'BRK-MISSING' does not match any PolicyAttribute in the same scope" &&
                    diagnostic.Properties["BrickConfigurationKind"] == "RoleCombination");
        }

        [Fact]
        public async Task MetadataAnalyzerReportsMemberContractCombinationConflicts()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

public sealed class IdentityAttribute : Attribute;
public sealed class SecretAttribute : Attribute;
public sealed class ReadRouteAttribute : Attribute;
public sealed class WriteRouteAttribute : Attribute;

[RequireExactlyOneMember(typeof(IdentityAttribute))]
[RequireMemberCount(typeof(IdentityAttribute), 2)]
public sealed class ConflictingIdentityContractAttribute : Attribute;

[RequireAllMembers(typeof(SecretAttribute))]
[ForbidMember(typeof(SecretAttribute))]
public sealed class RequiredAndForbiddenContractAttribute : Attribute;

[RequireExclusiveChoice(typeof(ReadRouteAttribute), typeof(WriteRouteAttribute))]
[RequireAllMembers(typeof(ReadRouteAttribute), typeof(WriteRouteAttribute))]
public sealed class ExclusiveButAllRequiredContractAttribute : Attribute;
");

            Assert.Equal(
                new[] { "XMoleculesBricks0205", "XMoleculesBricks0205", "XMoleculesBricks0205" },
                diagnostics.Select(diagnostic => diagnostic.Id).OrderBy(id => id).ToArray());
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage().Contains("'IdentityAttribute' requires exactly one member and count 2"));
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage().Contains("'SecretAttribute' is both required by RequireAllMembers and forbidden by ForbidMember"));
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.GetMessage().Contains("exclusive choice 'ReadRouteAttribute' or 'WriteRouteAttribute' cannot require both marker types"));
        }

        [Fact]
        public async Task MemberContractAnalyzerReportsCardinalityViolations()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

public sealed class IdentityAttribute : Attribute;
public sealed class CommandHandlerAttribute : Attribute;
public sealed class EventHandlerAttribute : Attribute;
public sealed class ReadRouteAttribute : Attribute;
public sealed class WriteRouteAttribute : Attribute;
public sealed class ParticipantAttribute : Attribute;
public sealed class ForbiddenSecretAttribute : Attribute;
public sealed class NamedIndicatorAttribute : Attribute
{
    public NamedIndicatorAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

[RequireExactlyOneMember(typeof(IdentityAttribute))]
public sealed class MissingIdentity
{
}

[RequireMemberCount(typeof(CommandHandlerAttribute), 2)]
public sealed class TooFewHandlers
{
    [CommandHandler]
    public void HandleSubmit() { }
}

[RequireAllMembers(typeof(ReadRouteAttribute), typeof(WriteRouteAttribute))]
public sealed class MissingWriteRoute
{
    [ReadRoute]
    public void Get() { }
}

[RequireExclusiveChoice(typeof(ReadRouteAttribute), typeof(WriteRouteAttribute))]
public sealed class AmbiguousEndpoint
{
    [ReadRoute]
    public void Get() { }

    [WriteRoute]
    public void Post() { }
}

[RequireMemberRange(typeof(ParticipantAttribute), 2, 4)]
public sealed class TooFewParticipants
{
    [Participant]
    public string First { get; init; } = string.Empty;
}

[ForbidMember(typeof(ForbiddenSecretAttribute))]
public sealed class LeakyContract
{
    [ForbiddenSecret]
    public string Secret { get; init; } = string.Empty;
}

[RequireUniqueNamedMember(typeof(NamedIndicatorAttribute))]
public sealed class DuplicateNamedIndicator
{
    [NamedIndicator(""X"")]
    public string FirstX { get; init; } = string.Empty;

    [NamedIndicator(""X"")]
    public string SecondX { get; init; } = string.Empty;

    [NamedIndicator(""Y"")]
    public string FirstY { get; init; } = string.Empty;
}

[RequireNamedMembers(typeof(NamedIndicatorAttribute), ""X"", ""Y"")]
public sealed class MissingNamedIndicator
{
    [NamedIndicator(""X"")]
    public string FirstX { get; init; } = string.Empty;
}
");

            Assert.Equal(
                new[]
                {
                    "XMoleculesBricks0003",
                    "XMoleculesBricks0004",
                    "XMoleculesBricks0005",
                    "XMoleculesBricks0006",
                    "XMoleculesBricks0007",
                    "XMoleculesBricks0008",
                    "XMoleculesBricks0009",
                    "XMoleculesBricks0010"
                },
                diagnostics.Select(diagnostic => diagnostic.Id).ToArray());
        }

        [Fact]
        public async Task DependencyRuleAnalyzerReportsRoleAliasAndBodyTypeUsage()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

[assembly: Rule(""DDD001"", ""Domain"", ""Infrastructure"", RuleMode.ForbidDependency)]

[AttributeUsage(AttributeTargets.Class)]
[RoleAlias(""Domain"")]
public sealed class DomainComponentAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
[RoleAlias(""Infrastructure"")]
public sealed class InfrastructureComponentAttribute : Attribute;

[DomainComponent]
public sealed class OrderAggregate
{
    public void Rehydrate()
    {
        var repository = new SqlOrderRepository();
    }
}

[InfrastructureComponent]
public sealed class SqlOrderRepository;
");

            Assert.Equal(
                new[] { "XMoleculesBricks0001" },
                diagnostics.Select(diagnostic => diagnostic.Id).ToArray());
        }

        [Fact]
        public async Task DependencyRuleAnalyzerReportsTypeRoleForReferencedType()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: TypeRole(typeof(ThirdParty.Payments.StripeClient), ""PaymentProviderSdk"")]
[assembly: Rule(""PAY001"", ""BusinessWorkflow"", ""PaymentProviderSdk"", RuleMode.ForbidDependency)]

[Role(""BusinessWorkflow"")]
public sealed class CheckoutWorkflow
{
    private readonly ThirdParty.Payments.StripeClient client = default!;
}

namespace ThirdParty.Payments
{
    public sealed class StripeClient
    {
    }
}
");

            Assert.Equal(
                new[] { "XMoleculesBricks0001" },
                diagnostics.Select(diagnostic => diagnostic.Id).ToArray());
        }

        [Fact]
        public async Task DependencyRuleAnalyzerReportsTypeRoleForNamedType()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;
using ThirdParty.Payments;

[assembly: TypeRole(nameof(PayPalClient), ""PaymentProviderSdk"")]
[assembly: Rule(""PAY002"", ""BusinessWorkflow"", ""PaymentProviderSdk"", RuleMode.ForbidDependency)]

[Role(""BusinessWorkflow"")]
public sealed class CheckoutWorkflow
{
    private readonly PayPalClient client = default!;
}

namespace ThirdParty.Payments
{
    public sealed class PayPalClient
    {
    }
}
");

            Assert.Equal(
                new[] { "XMoleculesBricks0001" },
                diagnostics.Select(diagnostic => diagnostic.Id).ToArray());
        }

        [Fact]
        public async Task MetadataAnalyzerReportsInvalidTypeRoleConfiguration()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: TypeRole("""", ""PaymentProviderSdk"")]
[assembly: TypeRole(typeof(StripeClient), """")]

public sealed class StripeClient
{
}
");

            Assert.Equal(
                new[] { "XMoleculesBricks0201", "XMoleculesBricks0201" },
                diagnostics.Select(diagnostic => diagnostic.Id).ToArray());
        }

        [Fact]
        public async Task MemberContractAnalyzerSupportsCustomContractAttributes()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

public sealed class IdentityAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
[RequireExactlyOneMember(typeof(IdentityAttribute))]
public sealed class AggregateContractAttribute : Attribute;

[AggregateContract]
public sealed class MissingIdentityAggregate
{
}

[AggregateContract]
public sealed class OrderAggregate
{
    [Identity]
    public string Id { get; init; } = string.Empty;
}
");

            Assert.Equal(
                new[] { "XMoleculesBricks0003" },
                diagnostics.Select(diagnostic => diagnostic.Id).ToArray());
        }

        [Fact]
        public async Task MemberContractAnalyzerSupportsDddIdentifierContractsOnRoleAttributes()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class DddIdentifierAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
[RoleAlias(""DDD.AggregateRoot"")]
[RequireExactlyOneMember(typeof(DddIdentifierAttribute))]
public sealed class DddAggregateRootAttribute : RoleAttribute
{
    public DddAggregateRootAttribute() : base(""DDD.AggregateRoot"")
    {
    }
}

[AttributeUsage(AttributeTargets.Class)]
[RoleAlias(""DDD.Entity"")]
[RequireExactlyOneMember(typeof(DddIdentifierAttribute))]
public sealed class DddEntityAttribute : RoleAttribute
{
    public DddEntityAttribute() : base(""DDD.Entity"")
    {
    }
}

[DddAggregateRoot]
public sealed class MissingAggregateIdentifier
{
}

[DddAggregateRoot]
public sealed class ContractAggregate
{
    [DddIdentifier]
    public string BusinessNumber { get; init; } = string.Empty;
}

[DddEntity]
public sealed class DuplicateEntityIdentifier
{
    [DddIdentifier]
    private readonly string key = string.Empty;

    [DddIdentifier]
    public string LegacyKey => key;
}

[DddEntity]
public sealed class ContractLine
{
    [DddIdentifier]
    public string LineNumber { get; init; } = string.Empty;
}
");

            Assert.Equal(
                new[] { "XMoleculesBricks0003", "XMoleculesBricks0003" },
                diagnostics.Select(diagnostic => diagnostic.Id).ToArray());
            Assert.Equal(
                new[]
                {
                    "Brick contract 'DddAggregateRoot' requires exactly one member marked with 'DddIdentifierAttribute', but 'MissingAggregateIdentifier' declares 0.",
                    "Brick contract 'DddEntity' requires exactly one member marked with 'DddIdentifierAttribute', but 'DuplicateEntityIdentifier' declares 2."
                },
                diagnostics.Select(diagnostic => diagnostic.GetMessage()).OrderBy(message => message).ToArray());
        }

        [Fact]
        public async Task AcceptsValidSimpleAndMultipleBricksMetadata()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

[assembly: Policy(""orders-policy"", ""Orders Policy"")]
[assembly: Rule(""DDD001"", ""Domain"", ""Infrastructure"", RuleMode.ForbidDependency)]
[assembly: Rule(""DDD002"", ""Application"", ""Repository"", RuleMode.RequireDependency)]

public sealed class IdentityAttribute : Attribute;
public sealed class CommandHandlerAttribute : Attribute;
public sealed class ReviewerAttribute : Attribute;
public sealed class InternalSecretAttribute : Attribute;
public sealed class NamedIndicatorAttribute : Attribute
{
    public NamedIndicatorAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

[Role(""Domain"")]
[RequireExactlyOneMember(typeof(IdentityAttribute))]
public sealed class OrderAggregate
{
    [Identity]
    public string Id { get; init; } = string.Empty;
}

[Role(""Application"")]
[RequireMemberCount(typeof(CommandHandlerAttribute), 2)]
[RequireMemberRange(typeof(ReviewerAttribute), 1, 2)]
[ForbidMember(typeof(InternalSecretAttribute))]
[RequireNamedMembers(typeof(NamedIndicatorAttribute), ""X"", ""Y"")]
[RequireUniqueNamedMember(typeof(NamedIndicatorAttribute))]
public sealed class SubmitOrderHandler
{
    private readonly IOrderRepository _repository = default!;

    [CommandHandler]
    public void Submit() { }

    [CommandHandler]
    public void Retry() { }

    [Reviewer]
    public string ReviewedBy { get; init; } = string.Empty;

    [NamedIndicator(""X"")]
    public string XIndicator { get; init; } = string.Empty;

    [NamedIndicator(""Y"")]
    public string YIndicator { get; init; } = string.Empty;
}

[Role(""Repository"")]
public interface IOrderRepository;

[Role(""Infrastructure"")]
public sealed class SqlOrderRepository;
");

            Assert.Empty(diagnostics);
        }

        private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync(string source)
        {
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
                new BrickMetadataAnalyzer(),
                new BrickDependencyRuleAnalyzer(),
                new BrickMemberContractAnalyzer());
            var diagnostics = await BrickAnalyzerTestFixture.AnalyzeAsync(
                "AnalyzerFixture",
                source,
                analyzers);

            return diagnostics.OrderBy(diagnostic => diagnostic.Id).ToArray();
        }
    }
}
