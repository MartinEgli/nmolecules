using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NMolecules.Bricks.Analyzers;
using Xunit;

namespace NMolecules.Bricks.Analyzers.Test
{
    public class BrickAnalyzerCoverageTest
    {
        [Theory]
        [MemberData(nameof(MetadataConfigurationCases))]
        public async Task MetadataConfigurationCoverageReportsExpectedDiagnostics(string name, string source, int expectedCount)
        {
            Assert.False(string.IsNullOrWhiteSpace(name));

            var diagnostics = await AnalyzeAsync(source);

            Assert.Equal(
                Enumerable.Repeat("XMoleculesBricks0002", expectedCount).ToArray(),
                DiagnosticIds(diagnostics));
        }

        [Theory]
        [MemberData(nameof(ForbiddenDependencyEvidenceCases))]
        public async Task ForbidDependencyCoverageReportsEverySupportedEvidenceShape(
            string roleStyle,
            string evidenceName,
            string sourceBody,
            string extraAssemblyAttribute)
        {
            Assert.False(string.IsNullOrWhiteSpace(evidenceName));

            var diagnostics = await AnalyzeAsync(BuildDependencySource(
                roleStyle,
                "ForbidDependency",
                sourceBody,
                extraAssemblyAttribute));

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Theory]
        [MemberData(nameof(RequiredDependencyEvidenceCases))]
        public async Task RequireDependencyCoverageAcceptsEverySupportedEvidenceShape(
            string roleStyle,
            string evidenceName,
            string sourceBody,
            string extraAssemblyAttribute)
        {
            Assert.False(string.IsNullOrWhiteSpace(evidenceName));

            var diagnostics = await AnalyzeAsync(BuildDependencySource(
                roleStyle,
                "RequireDependency",
                sourceBody,
                extraAssemblyAttribute));

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("direct")]
        [InlineData("alias")]
        public async Task RequireDependencyCoverageReportsMissingDependency(string roleStyle)
        {
            var diagnostics = await AnalyzeAsync(BuildDependencySource(
                roleStyle,
                "RequireDependency",
                string.Empty));

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageAcceptsCompilationWithoutRules()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[Role(""Source"")]
public sealed class SourceType
{
    private readonly TargetType _target = default!;
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DependencyCoverageAcceptsDefaultAllowPolicyWithoutRules()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""P1"", defaultDecision: BrickPermissionDefault.Allow)]

[Role(""Source"")]
public sealed class SourceType
{
    private readonly TargetType _target = default!;
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DependencyCoverageReportsDefaultDenyPolicyWithoutAllowRule()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""P1"", defaultDecision: BrickPermissionDefault.Deny)]

[Role(""Source"")]
public sealed class SourceType
{
    private readonly TargetType _target = default!;
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageReportsModuleLevelDefaultDenyPolicy()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[module: Policy(""P1"", defaultDecision: BrickPermissionDefault.Deny)]

[Role(""Source"")]
public sealed class SourceType
{
    private readonly TargetType _target = default!;
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageReportsTypeLevelDefaultDenyPolicy()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[Policy(""P1"", defaultDecision: BrickPermissionDefault.Deny)]
public static class ProjectArchitecturePolicy
{
}

[Role(""Source"")]
public sealed class SourceType
{
    private readonly TargetType _target = default!;
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageAcceptsDefaultDenyPolicyWithAllowRule()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""P1"", defaultDecision: BrickPermissionDefault.Deny)]
[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.AllowDependency)]

[Role(""Source"")]
public sealed class SourceType
{
    private readonly TargetType _target = default!;
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DependencyCoverageIgnoresDisabledDefaultDenyPolicy()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""P1"", defaultDecision: BrickPermissionDefault.Deny, enforcement: BrickEnforcementMode.Disabled)]

[Role(""Source"")]
public sealed class SourceType
{
    private readonly TargetType _target = default!;
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DependencyCoverageAppliesRequiredSourceNameFilter()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]
[assembly: RequiredSourceNameContains(""R1"", ""Special"")]

[Role(""Source"")]
public sealed class SourceType
{
    private readonly TargetType _target = default!;
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DependencyCoverageReportsWhenRequiredSourceNameFilterMatches()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]
[assembly: RequiredSourceNameContains(""R1"", ""Special"")]

[Role(""Source"")]
public sealed class SpecialSourceType
{
    private readonly TargetType _target = default!;
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageAppliesRequiredTargetNameFilter()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]
[assembly: RequiredTargetNameContains(""R1"", ""Internal"")]

[Role(""Source"")]
public sealed class SourceType
{
    private readonly TargetType _target = default!;
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DependencyCoverageAppliesExcludedSourceNameFilter()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]
[assembly: ExcludedSourceNameContains(""R1"", ""Generated"")]

[Role(""Source"")]
public sealed class GeneratedSourceType
{
    private readonly TargetType _target = default!;
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DependencyCoverageAppliesExcludedTargetNameFilter()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]
[assembly: ExcludedTargetNameContains(""R1"", ""Internal"")]

[Role(""Source"")]
public sealed class SourceType
{
    private readonly InternalTargetType _target = default!;
}

[Role(""Target"")]
public sealed class InternalTargetType
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task RequireDependencyCoverageAppliesRequiredSourceNameFilter()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.RequireDependency)]
[assembly: RequiredSourceNameContains(""R1"", ""Special"")]

[Role(""Source"")]
public sealed class SourceType
{
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task RequireDependencyCoverageReportsWhenRequiredSourceNameFilterMatches()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.RequireDependency)]
[assembly: RequiredSourceNameContains(""R1"", ""Special"")]

[Role(""Source"")]
public sealed class SpecialSourceType
{
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageAcceptsCompilationWithoutRoles()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

public sealed class SourceType
{
    private readonly TargetType _target = default!;
}

public sealed class TargetType
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DependencyCoverageIgnoresDeclaredDependencyWithUnknownEndpoint()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]
[assembly: Dependency(""D1"", ""SourceType"", ""MissingTarget"", BrickDependencyKinds.TypeReference)]

[Role(""Source"")]
public sealed class SourceType
{
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DependencyCoverageIgnoresUnroledTargetTypeUsage()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

[Role(""Source"")]
public sealed class SourceType
{
    public void Use()
    {
        var other = new UnroledType();
        _ = other;
    }
}

public sealed class UnroledType
{
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DependencyCoverageNormalizesNullableValueTypes()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

[Role(""Source"")]
public sealed class SourceType
{
    private readonly TargetType? _target = default;
}

[Role(""Target"")]
public struct TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageExpandsPointerTypes()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

[Role(""Source"")]
public unsafe struct SourceType
{
    public TargetType* Target;
}

[Role(""Target"")]
public struct TargetType
{
    public int Value;
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageReportsBaseTypeDependencies()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

[Role(""Source"")]
public sealed class SourceType : TargetType
{
}

[Role(""Target"")]
public class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageReportsImplementedInterfaceDependencies()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

[Role(""Source"")]
public sealed class SourceType : ITargetType
{
}

[Role(""Target"")]
public interface ITargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageReportsInheritedInterfaceDependencies()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

[Role(""Source"")]
public sealed class SourceType : IIntermediateType
{
}

public interface IIntermediateType : ITargetType
{
}

[Role(""Target"")]
public interface ITargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageReportsGenericConstraintDependencies()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

[Role(""Source"")]
public sealed class SourceType<TTarget>
    where TTarget : TargetType
{
}

[Role(""Target"")]
public class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageReportsGenericMethodConstraintDependencies()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

[Role(""Source"")]
public sealed class SourceType
{
    public void Use<TTarget>(TTarget target)
        where TTarget : TargetType
    {
    }
}

[Role(""Target"")]
public class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageReportsAttributeTypeArgumentDependencies()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

public sealed class UsesTypeAttribute : Attribute
{
    public UsesTypeAttribute(Type type)
    {
    }
}

[Role(""Source"")]
[UsesType(typeof(TargetType))]
public sealed class SourceType
{
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageReportsExtensionMethodContainingTypeDependencies()
        {
            var diagnostics = await AnalyzeAsync(@"
using Helpers;
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

[Role(""Source"")]
public sealed class SourceType
{
    public void Run(string value)
    {
        value.TouchTarget();
    }
}

namespace Helpers
{
    [Role(""Target"")]
    public static class TargetExtensions
    {
        public static void TouchTarget(this string value)
        {
        }
    }
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageReadsDerivedRuleNamedProperties()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

[assembly: ConfiguredRule(Id = ""R1"", SourceRole = ""Source"", TargetRole = ""Target"", Mode = RuleMode.RequireDependency)]

public sealed class ConfiguredRuleAttribute : RuleAttribute
{
    public new string Id { get; set; } = string.Empty;
    public new string SourceRole { get; set; } = string.Empty;
    public new string TargetRole { get; set; } = string.Empty;
    public new RuleMode Mode { get; set; }
}

[Role(""Source"")]
public sealed class SourceType
{
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageIgnoresUnusableDerivedRuleWithoutMetadata()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

[assembly: EmptyRule]

public sealed class EmptyRuleAttribute : RuleAttribute
{
}

[Role(""Source"")]
public sealed class SourceType
{
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DependencyCoverageIgnoresInvalidDeclaredDependencyEndpoint()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]
[assembly: Dependency(""D1"", """", ""TargetType"", BrickDependencyKinds.TypeReference)]

[Role(""Source"")]
public sealed class SourceType
{
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0002" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageIgnoresNullDeclaredDependencyEndpoint()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]
[assembly: Dependency(""D1"", null, ""TargetType"", BrickDependencyKinds.TypeReference)]

[Role(""Source"")]
public sealed class SourceType
{
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0002" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageIgnoresEmptyDirectRole()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

[Role("""")]
public sealed class SourceType
{
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0002" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageAppliesAssemblyLevelRoles()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Role(""Source"")]
[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

public sealed class SourceType
{
    private readonly TargetType _target = default!;
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageAppliesModuleLevelRoles()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[module: Role(""Source"")]
[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

public sealed class SourceType
{
    private readonly TargetType _target = default!;
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageAppliesModuleLevelRules()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[module: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

[Role(""Source"")]
public sealed class SourceType
{
    private readonly TargetType _target = default!;
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageAppliesTypeLevelRules()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]
public static class ProjectArchitectureRules
{
}

[Role(""Source"")]
public sealed class SourceType
{
    private readonly TargetType _target = default!;
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageAppliesTypeLevelRuleFilters()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]
[ExcludedSourceNameContains(""R1"", ""Generated"")]
public static class ProjectArchitectureRules
{
}

[Role(""Source"")]
public sealed class GeneratedSourceType
{
    private readonly TargetType _target = default!;
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DependencyCoverageAppliesExactNamespaceRoles()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: NamespaceRole(""SourceArea"", ""Source"")]
[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

namespace SourceArea
{
    public sealed class SourceType
    {
        private readonly TargetType _target = default!;
    }
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageAppliesNamespaceRolePrefixPatterns()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: NamespaceRole(""SourceArea.*"", ""Source"")]
[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

namespace SourceArea.Feature
{
    public sealed class SourceType
    {
        private readonly TargetType _target = default!;
    }
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageReportsProjectSpecificDddBrickViolation()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

[assembly: Rule(""DDD001"", ""DDD.DomainModel"", ""DDD.InfrastructureAdapter"", RuleMode.ForbidDependency)]

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
[RoleAlias(""DDD.DomainModel"")]
public sealed class DomainModelAttribute : RoleAttribute
{
    public DomainModelAttribute() : base(""DDD.DomainModel"")
    {
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
[RoleAlias(""DDD.InfrastructureAdapter"")]
public sealed class InfrastructureAdapterAttribute : RoleAttribute
{
    public InfrastructureAdapterAttribute() : base(""DDD.InfrastructureAdapter"")
    {
    }
}

namespace Sales.Domain
{
    [DomainModel]
    public sealed class OrderAggregate
    {
        private readonly Sales.Infrastructure.SqlOrderRepository repository = default!;
    }
}

namespace Sales.Infrastructure
{
    [InfrastructureAdapter]
    public sealed class SqlOrderRepository
    {
    }
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageAcceptsProjectSpecificDddNamespaceRoleMove()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

[assembly: NamespaceRole(""Sales.Domain.*"", ""DDD.DomainModel"")]
[assembly: NamespaceRole(""Sales.Infrastructure.*"", ""DDD.InfrastructureAdapter"")]
[assembly: Rule(""DDD001"", ""DDD.DomainModel"", ""DDD.InfrastructureAdapter"", RuleMode.ForbidDependency)]

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
[RoleAlias(""DDD.ApplicationService"")]
public sealed class ApplicationServiceAttribute : RoleAttribute
{
    public ApplicationServiceAttribute() : base(""DDD.ApplicationService"")
    {
    }
}

namespace Sales.Domain.Model
{
    public sealed class OrderAggregate
    {
        public OrderId Id { get; init; } = new OrderId();
    }

    public sealed class OrderId
    {
    }
}

namespace Sales.Application
{
    [ApplicationService]
    public sealed class SubmitOrderHandler
    {
        private readonly Sales.Domain.Model.OrderAggregate order = default!;
    }
}

namespace Sales.Infrastructure.Persistence
{
    public sealed class SqlOrderRepository
    {
    }
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task NamespaceRoleMetadataCoverageReportsEmptyNamespacePattern()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: NamespaceRole("""", ""Source"")]

public sealed class SourceType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0002" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task NamespaceRoleMetadataCoverageReportsEmptyRole()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: NamespaceRole(""SourceArea.*"", """")]

public sealed class SourceType
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0002" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task XmlDocumentationCoverageReportsUndocumentedPublicApi()
        {
            var diagnostics = await AnalyzeDocumentationAsync(@"
using System;

public class UndocumentedType
{
    public UndocumentedType()
    {
    }

    public string Name { get; init; } = string.Empty;

    public string Description;

    public event EventHandler Changed;

    public void Run()
    {
    }
}
");

            Assert.Equal(
                Enumerable.Repeat("XMoleculesBricks0011", 6).ToArray(),
                DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task XmlDocumentationCoverageAcceptsDocumentedPublicApi()
        {
            var diagnostics = await AnalyzeDocumentationAsync(@"
using System;

/// <summary>
/// Documented reusable type.
/// </summary>
public class DocumentedType
{
    /// <summary>
    /// Initializes a new instance of the <see cref=""DocumentedType""/> class.
    /// </summary>
    public DocumentedType()
    {
    }

    /// <summary>
    /// Gets the documented name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Stores a documented description.
    /// </summary>
    public string Description;

    /// <summary>
    /// Raised when the documented type changes.
    /// </summary>
    public event EventHandler Changed;

    /// <summary>
    /// Runs the documented operation.
    /// </summary>
    public void Run()
    {
    }
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task XmlDocumentationCoverageAcceptsDocumentedAttributedPublicApi()
        {
            var diagnostics = await AnalyzeDocumentationAsync(@"
using NMolecules.Bricks;

/// <summary>
/// Documented attributed type.
/// </summary>
[Role(""Domain"")]
public sealed class DocumentedAttributedType
{
    /// <summary>
    /// Gets the documented identifier.
    /// </summary>
    [Role(""Identifier"")]
    public string Id { get; init; } = string.Empty;
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task XmlDocumentationCoverageReportsPublicInterfaceMembersAndEnumMembers()
        {
            var diagnostics = await AnalyzeDocumentationAsync(@"
public interface PublicContract
{
    string Name { get; }

    void Run();
}

public enum PublicState
{
    Started,
    Stopped
}
");

            Assert.Equal(
                Enumerable.Repeat("XMoleculesBricks0011", 6).ToArray(),
                DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task XmlDocumentationCoverageIgnoresNonPublicApi()
        {
            var diagnostics = await AnalyzeDocumentationAsync(@"
internal class InternalType
{
    public string Name { get; init; } = string.Empty;

    public void Run()
    {
    }
}

public class PublicContainer
{
    private string Hidden { get; init; } = string.Empty;

    private void Run()
    {
    }
}
");

            Assert.Equal(new[] { "XMoleculesBricks0011" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyCoverageReportsSyntaxObjectCreationsAndArrayTargets()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

[Role(""Source"")]
public sealed class SourceType
{
    private TargetType field = new TargetType();

    public void Run()
    {
        TargetType[] targets = new TargetType[0];
        var other = string.Empty;
        _ = targets;
        _ = other;
    }
}

[Role(""Target"")]
public sealed class TargetType
{
}
");

            var ids = DiagnosticIds(diagnostics);
            Assert.NotEmpty(ids);
            Assert.All(ids, id => Assert.Equal("XMoleculesBricks0001", id));
        }

        [Fact]
        public async Task AnalyzerCoverageIgnoresUnresolvedAttributes()
        {
            var diagnostics = await AnalyzeAsync(@"
[Missing]
public sealed class Sample
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task AnalyzerCoverageIgnoresMalformedTypeDeclarations()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

public class
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task AnalyzerCoverageIgnoresMalformedTypeWithMissingIdentifier()
        {
            var diagnostics = await AnalyzeAsync(@"
public class
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task MetadataCoverageIgnoresNonConstantStringArguments()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(NonConstantValues.PolicyId)]

public static class NonConstantValues
{
    public static readonly string PolicyId = string.Empty;
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public void AnalyzerFactsCoverageReturnsNullForMissingIntArgument()
        {
            var attribute = CSharpSyntaxTree
                .ParseText("[Sample]\npublic sealed class Sample { }")
                .GetRoot()
                .DescendantNodes()
                .OfType<AttributeSyntax>()
                .Single();

            var result = InvokeAnalyzerFacts(
                "GetIntArgument",
                default(SyntaxNodeAnalysisContext),
                attribute,
                0,
                new[] { "count" });

            Assert.Null(result);
        }

        [Fact]
        public void AnalyzerFactsCoverageHandlesMissingOptionalArguments()
        {
            var attribute = CSharpSyntaxTree
                .ParseText("[Sample(\"value\")]\npublic sealed class Sample { }")
                .GetRoot()
                .DescendantNodes()
                .OfType<AttributeSyntax>()
                .Single();
            var typeAttribute = CSharpSyntaxTree
                .ParseText("[Sample]\npublic sealed class Sample { }")
                .GetRoot()
                .DescendantNodes()
                .OfType<AttributeSyntax>()
                .Single();
            var methodAttribute = CSharpSyntaxTree
                .ParseText("public sealed class Sample { [Sample] public void Run() { } }")
                .GetRoot()
                .DescendantNodes()
                .OfType<AttributeSyntax>()
                .Single();
            var detachedAttribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Sample"));

            var missingString = InvokeAnalyzerFacts(
                "GetStringArgument",
                default(SyntaxNodeAnalysisContext),
                attribute,
                2,
                new[] { "missing" });
            var foundArgument = InvokeAnalyzerFactsPrivate("FindArgument", attribute, 0, Array.Empty<string>());
            var missingArgument = InvokeAnalyzerFactsPrivate("FindArgument", attribute, 2, Array.Empty<string>());
            var missingType = InvokeAnalyzerFacts(
                "GetTypeArgument",
                default(SyntaxNodeAnalysisContext),
                typeAttribute,
                0,
                new[] { "memberAttributeType" });
            var annotatedType = InvokeAnalyzerFacts("FindAnnotatedTypeName", methodAttribute);
            var detachedAnnotatedType = InvokeAnalyzerFacts("FindAnnotatedTypeName", detachedAttribute);
            var missingRequiredNames = ((IEnumerable)InvokePrivateStatic(
                typeof(BrickMetadataAnalyzer),
                "GetStringArgumentsFromOrdinal",
                default(SyntaxNodeAnalysisContext),
                detachedAttribute,
                1)).Cast<object>().ToArray();

            Assert.Null(missingString);
            Assert.NotNull(foundArgument);
            Assert.Null(missingArgument);
            Assert.Null(missingType);
            Assert.Null(annotatedType);
            Assert.Null(detachedAnnotatedType);
            Assert.Empty(missingRequiredNames);
        }

        [Fact]
        public async Task MetadataCoverageReportsAssemblyRoleTargetFallback()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Role("""")]

public sealed class Sample
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0002" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task MetadataCoverageAcceptsValidRoleAndNonNegativeMemberCount()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

public sealed class MarkerAttribute : Attribute
{
    public MarkerAttribute()
    {
    }

    public MarkerAttribute(string name)
    {
        Name = name;
    }

    public MarkerAttribute(int number)
    {
        Number = number;
    }

    public string Name { get; }

    public string Slot { get; set; }

    public int Number { get; }
}

[Role(""Source"")]
[RequireMemberCount(typeof(MarkerAttribute), 1)]
public sealed class Sample
{
    [Marker]
    public string Id { get; init; } = string.Empty;
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task MetadataCoverageIgnoresNonConstantCountArguments()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

public sealed class MarkerAttribute : Attribute
{
}

public static class NonConstantValues
{
    public static readonly int Count = -1;
}

[RequireMemberCount(typeof(MarkerAttribute), NonConstantValues.Count)]
public sealed class Sample
{
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public void DefensiveAnalyzerHelpersHandleNullAttributeData()
        {
            var attribute = new NullAttributeData();

            var isRule = InvokePrivateStatic(typeof(BrickDependencyRuleAnalyzer), "IsRuleAttribute", attribute);
            var isDependency = InvokePrivateStatic(typeof(BrickDependencyRuleAnalyzer), "IsDependencyAttribute", attribute);
            var contractName = InvokePrivateStatic(typeof(BrickMemberContractAnalyzer), "GetContractName", attribute, attribute);
            var untrimmed = InvokePrivateStatic(typeof(BrickMemberContractAnalyzer), "TrimAttributeSuffix", "Marker");
            var shouldAnalyzeNullType = InvokePrivateStatic(
                typeof(BrickMemberContractAnalyzer),
                "ShouldAnalyzeType",
                new object[] { null });

            Assert.Equal(false, isRule);
            Assert.Equal(false, isDependency);
            Assert.Equal("MemberContract", contractName);
            Assert.Equal("Marker", untrimmed);
            Assert.Equal(false, shouldAnalyzeNullType);
        }

        [Fact]
        public void DependencyCoverageUsesFallbackMemberNameWithoutContainingMember()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText("public sealed class Sample { }");
            var compilation = CSharpCompilation.Create(
                "FallbackMemberNameFixture",
                new[] { syntaxTree },
                new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var method = typeof(BrickDependencyRuleAnalyzer).GetMethod(
                "FindContainingMemberName",
                BindingFlags.NonPublic | BindingFlags.Static);

            var result = method.Invoke(null, new object[] { semanticModel, syntaxTree.GetRoot() });

            Assert.Equal("member body", result);
        }

        [Fact]
        public void DefensiveAttributeDataCoverageIgnoresNullAttributeClass()
        {
            var attribute = new NullAttributeData();
            var tryGetRoleName = typeof(BrickDependencyRuleAnalyzer).GetMethod(
                "TryGetRoleName",
                BindingFlags.NonPublic | BindingFlags.Static);
            var getMemberContracts = typeof(BrickMemberContractAnalyzer).GetMethod(
                "GetMemberContracts",
                BindingFlags.NonPublic | BindingFlags.Static);
            var getTypeArray = typeof(BrickMemberContractAnalyzer).GetMethod(
                "GetTypeArray",
                BindingFlags.NonPublic | BindingFlags.Static);
            var getStringArray = typeof(BrickMemberContractAnalyzer).GetMethod(
                "GetStringArray",
                BindingFlags.NonPublic | BindingFlags.Static);
            var getNamedString = typeof(BrickMemberContractAnalyzer).GetMethod(
                "GetNamedString",
                BindingFlags.NonPublic | BindingFlags.Static);
            var hasEmptyNamedString = typeof(BrickMemberContractAnalyzer).GetMethod(
                "HasEmptyNamedString",
                BindingFlags.NonPublic | BindingFlags.Static);
            var unmatchedNamedAttribute = new AttributeDataStub(
                ImmutableArray<TypedConstant>.Empty,
                ImmutableArray.Create(new KeyValuePair<string, TypedConstant>("Other", default(TypedConstant))));
            var nullConstructorArgumentAttribute = new AttributeDataStub(
                ImmutableArray.Create(default(TypedConstant)),
                ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty);
            var primitiveConstructorArgumentAttribute = new AttributeDataStub(
                ImmutableArray.Create(CreatePrimitiveTypedConstant(1)),
                ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty);

            var roleName = tryGetRoleName.Invoke(null, new object[] { attribute });
            var contracts = ((IEnumerable)getMemberContracts.Invoke(null, new object[] { attribute })).Cast<object>().ToArray();
            var typeArray = ((IEnumerable)getTypeArray.Invoke(null, new object[] { attribute })).Cast<object>().ToArray();
            var missingStringArray = ((IEnumerable)getStringArray.Invoke(null, new object[] { attribute, 0 })).Cast<object>().ToArray();
            var nullStringArray = ((IEnumerable)getStringArray.Invoke(null, new object[] { nullConstructorArgumentAttribute, 0 })).Cast<object>().ToArray();
            var primitiveStringArray = ((IEnumerable)getStringArray.Invoke(null, new object[] { primitiveConstructorArgumentAttribute, 0 })).Cast<object>().ToArray();
            var defaultNamedString = (string)getNamedString.Invoke(null, new object[] { unmatchedNamedAttribute, "NameArgument", "Name" });
            var unmatchedHasEmptyNamedString = (bool)hasEmptyNamedString.Invoke(null, new object[] { unmatchedNamedAttribute, "NameArgument" });

            Assert.Null(roleName);
            Assert.Empty(contracts);
            Assert.Empty(typeArray);
            Assert.Empty(missingStringArray);
            Assert.Empty(nullStringArray);
            Assert.Empty(primitiveStringArray);
            Assert.Equal("Name", defaultNamedString);
            Assert.False(unmatchedHasEmptyNamedString);
        }

        [Theory]
        [MemberData(nameof(MemberContractCases))]
        public async Task MemberContractCoverageReportsEveryContractCombination(
            string carrierStyle,
            string contractAttribute,
            string members,
            string[] expectedIds)
        {
            var diagnostics = await AnalyzeAsync(BuildMemberContractSource(carrierStyle, contractAttribute, members));

            Assert.Equal(expectedIds, DiagnosticIds(diagnostics));
        }

        [Theory]
        [MemberData(nameof(MarkedMemberKindCases))]
        public async Task MemberContractCoverageAcceptsEverySupportedMarkedMemberKind(string memberKind, string markedMember)
        {
            Assert.False(string.IsNullOrWhiteSpace(memberKind));

            var diagnostics = await AnalyzeAsync(BuildMemberContractSource(
                "direct",
                "RequireExactlyOneMember(typeof(MarkerAttribute))",
                markedMember));

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task MemberContractCoverageIgnoresMarkedConstructors()
        {
            var diagnostics = await AnalyzeAsync(BuildMemberContractSource(
                "direct",
                "RequireExactlyOneMember(typeof(MarkerAttribute))",
                "    [Marker]\n    public Sample() { }\n"));

            Assert.Equal(new[] { "XMoleculesBricks0003" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task MemberContractCoverageAcceptsDerivedMemberMarker()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

public class MarkerAttribute : Attribute
{
}

public sealed class SpecializedMarkerAttribute : MarkerAttribute
{
}

[RequireExactlyOneMember(typeof(MarkerAttribute))]
public sealed class Sample
{
    [SpecializedMarker]
    public string Id { get; init; } = string.Empty;
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task MemberContractCoverageSupportsRecords()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

public sealed class MarkerAttribute : Attribute
{
}

[RequireExactlyOneMember(typeof(MarkerAttribute))]
public sealed record Sample
{
    [Marker]
    public string Id { get; init; } = string.Empty;
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task MemberContractCoverageSupportsInterfaces()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

public sealed class MarkerAttribute : Attribute
{
}

[RequireExactlyOneMember(typeof(MarkerAttribute))]
public interface ISample
{
    [Marker]
    string Id { get; }
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task MemberContractCoverageSupportsAbstractClasses()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

public sealed class MarkerAttribute : Attribute
{
}

[RequireExactlyOneMember(typeof(MarkerAttribute))]
public abstract class Sample
{
    [Marker]
    public abstract string Id { get; }
}
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task MemberContractCoverageIgnoresInvalidCustomContractMetadata()
        {
            var diagnostics = await AnalyzeAsync(@"
using System;
using NMolecules.Bricks;

[AttributeUsage(AttributeTargets.Class)]
[RequireExactlyOneMember]
[RequireMemberCount]
public sealed class ContractAttribute : Attribute
{
}

[Contract]
public sealed class Sample
{
}
");

            Assert.Empty(diagnostics);
        }

        public static IEnumerable<object[]> MetadataConfigurationCases()
        {
            yield return MetadataCase("empty role", "[Role(\"\")]", 1);
            yield return MetadataCase("null role", "[Role(null)]", 1);
            yield return MetadataCase("named empty role", "[Role(name: \"\")]", 1);
            yield return MetadataCase("[assembly] empty policy", "[assembly: Policy(\"\")]", 1);
            yield return MetadataCase("[assembly] null policy", "[assembly: Policy(null)]", 1);
            yield return MetadataCase("[assembly] named empty policy", "[assembly: Policy(id: \"\")]", 1);
            yield return MetadataCase("[assembly] dependency default kind", "[assembly: Dependency(\"D1\", \"SourceType\", \"TargetType\")]", 0);
            yield return MetadataCase("[assembly] empty rule id", "[assembly: Rule(\"\", \"Source\", \"Target\")]", 1);
            yield return MetadataCase("[assembly] null rule id", "[assembly: Rule(null, \"Source\", \"Target\")]", 1);
            yield return MetadataCase("[assembly] empty rule source", "[assembly: Rule(\"R1\", \"\", \"Target\")]", 1);
            yield return MetadataCase("[assembly] empty rule target", "[assembly: Rule(\"R1\", \"Source\", \"\")]", 1);
            yield return MetadataCase("[assembly] named empty rule parts", "[assembly: Rule(id: \"\", sourceRole: \"\", targetRole: \"\")]", 3);
            yield return MetadataCase("[assembly] empty dependency id", "[assembly: Dependency(\"\", \"SourceType\", \"TargetType\", BrickDependencyKinds.TypeReference)]", 1);
            yield return MetadataCase("[assembly] null dependency id", "[assembly: Dependency(null, \"SourceType\", \"TargetType\", BrickDependencyKinds.TypeReference)]", 1);
            yield return MetadataCase("[assembly] empty dependency source", "[assembly: Dependency(\"D1\", \"\", \"TargetType\", BrickDependencyKinds.TypeReference)]", 1);
            yield return MetadataCase("[assembly] empty dependency target", "[assembly: Dependency(\"D1\", \"SourceType\", \"\", BrickDependencyKinds.TypeReference)]", 1);
            yield return MetadataCase("[assembly] empty dependency kind", "[assembly: Dependency(\"D1\", \"SourceType\", \"TargetType\", \"\")]", 1);
            yield return MetadataCase("missing exactly-one marker", "[RequireExactlyOneMember(null)]", 1);
            yield return MetadataCase("negative count", "[RequireMemberCount(typeof(MarkerAttribute), -1)]", 1);
            yield return MetadataCase("named negative count", "[RequireMemberCount(count: -1, memberAttributeType: typeof(MarkerAttribute))]", 1);
            yield return MetadataCase("named missing marker and negative count", "[RequireMemberCount(count: -1, memberAttributeType: null)]", 2);
            yield return MetadataCase("missing range marker", "[RequireMemberRange(null, 1, 2)]", 1);
            yield return MetadataCase("negative range minimum", "[RequireMemberRange(typeof(MarkerAttribute), -1, 2)]", 1);
            yield return MetadataCase("negative range maximum", "[RequireMemberRange(typeof(MarkerAttribute), 1, -2)]", 2);
            yield return MetadataCase("named inverted range", "[RequireMemberRange(maximumCount: 2, minimumCount: 3, memberAttributeType: typeof(MarkerAttribute))]", 1);
            yield return MetadataCase("non-constant range minimum", "[RequireMemberRange(typeof(MarkerAttribute), NonConstantValues.Count, 2)]", 0);
            yield return MetadataCase("non-constant range maximum", "[RequireMemberRange(typeof(MarkerAttribute), 1, NonConstantValues.Count)]", 0);
            yield return MetadataCase("missing all-members markers without parentheses", "[RequireAllMembers]", 1);
            yield return MetadataCase("missing all-members markers", "[RequireAllMembers()]", 1);
            yield return MetadataCase("null all-members marker", "[RequireAllMembers(null)]", 1);
            yield return MetadataCase("missing exclusive-choice marker", "[RequireExclusiveChoice(null, typeof(MarkerAttribute))]", 1);
            yield return MetadataCase("named missing exclusive-choice marker", "[RequireExclusiveChoice(rightMemberAttributeType: null, leftMemberAttributeType: typeof(MarkerAttribute))]", 1);
            yield return MetadataCase("same exclusive-choice markers", "[RequireExclusiveChoice(typeof(MarkerAttribute), typeof(MarkerAttribute))]", 1);
            yield return MetadataCase("missing forbidden marker", "[ForbidMember(null)]", 1);
            yield return MetadataCase("missing named members marker", "[RequireNamedMembers(null, \"X\")]", 1);
            yield return MetadataCase("missing named members names", "[RequireNamedMembers(typeof(MarkerAttribute))]", 1);
            yield return MetadataCase("null named members names", "[RequireNamedMembers(typeof(MarkerAttribute), requiredNames: null)]", 1);
            yield return MetadataCase("duplicate named members names", "[RequireNamedMembers(typeof(MarkerAttribute), \"X\", \"X\")]", 1);
            yield return MetadataCase("duplicate unnamed named members names", "[RequireNamedMembers(typeof(MarkerAttribute), \"\", \"   \")]", 1);
            yield return MetadataCase("empty named members name argument", "[RequireNamedMembers(typeof(MarkerAttribute), \"X\", NameArgument = \"\")]", 1);
            yield return MetadataCase("missing unique named marker", "[RequireUniqueNamedMember(null)]", 1);
            yield return MetadataCase("empty unique named argument", "[RequireUniqueNamedMember(typeof(MarkerAttribute), \"\")]", 1);
            yield return MetadataCase("named empty unique named argument", "[RequireUniqueNamedMember(nameArgument: \"\", memberAttributeType: typeof(MarkerAttribute))]", 1);
        }

        public static IEnumerable<object[]> ForbiddenDependencyEvidenceCases() =>
            DependencyEvidenceCases();

        public static IEnumerable<object[]> RequiredDependencyEvidenceCases() =>
            DependencyEvidenceCases();

        public static IEnumerable<object[]> MemberContractCases()
        {
            foreach (var carrier in new[] { "direct", "custom" })
            {
                yield return MemberCase(carrier, "RequireExactlyOneMember(typeof(MarkerAttribute))", OneMarkerMember(), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireExactlyOneMember(typeof(MarkerAttribute))", string.Empty, "XMoleculesBricks0003");
                yield return MemberCase(carrier, "RequireExactlyOneMember(typeof(MarkerAttribute))", TwoMarkerMembers(), "XMoleculesBricks0003");

                yield return MemberCase(carrier, "RequireMemberCount(typeof(MarkerAttribute), 2)", TwoMarkerMembers(), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireMemberCount(typeof(MarkerAttribute), 2)", OneMarkerMember(), "XMoleculesBricks0005");
                yield return MemberCase(carrier, "RequireMemberCount(typeof(MarkerAttribute), 2)", ThreeMarkerMembers(), "XMoleculesBricks0005");

                yield return MemberCase(carrier, "RequireMemberRange(typeof(MarkerAttribute), 2, 3)", TwoMarkerMembers(), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireMemberRange(typeof(MarkerAttribute), 2, 3)", ThreeMarkerMembers(), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireMemberRange(typeof(MarkerAttribute), 2, 3)", OneMarkerMember(), "XMoleculesBricks0007");
                yield return MemberCase(carrier, "RequireMemberRange(typeof(MarkerAttribute), 2, 3)", string.Empty, "XMoleculesBricks0007");

                yield return MemberCase(carrier, "RequireAllMembers(typeof(MarkerAttribute), typeof(OtherMarkerAttribute))", OneMarkerMember() + OtherMarkerMember(), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireAllMembers(typeof(MarkerAttribute), typeof(OtherMarkerAttribute))", TwoMarkerMembers() + OtherMarkerMember(), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireAllMembers(typeof(MarkerAttribute), typeof(OtherMarkerAttribute))", OneMarkerMember(), "XMoleculesBricks0004");
                yield return MemberCase(carrier, "RequireAllMembers(typeof(MarkerAttribute), typeof(OtherMarkerAttribute))", string.Empty, "XMoleculesBricks0004", "XMoleculesBricks0004");

                yield return MemberCase(carrier, "RequireExclusiveChoice(typeof(MarkerAttribute), typeof(OtherMarkerAttribute))", OneMarkerMember(), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireExclusiveChoice(typeof(MarkerAttribute), typeof(OtherMarkerAttribute))", OtherMarkerMember(), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireExclusiveChoice(typeof(MarkerAttribute), typeof(OtherMarkerAttribute))", string.Empty, "XMoleculesBricks0006");
                yield return MemberCase(carrier, "RequireExclusiveChoice(typeof(MarkerAttribute), typeof(OtherMarkerAttribute))", OneMarkerMember() + OtherMarkerMember(), "XMoleculesBricks0006");

                yield return MemberCase(carrier, "ForbidMember(typeof(MarkerAttribute))", string.Empty, Array.Empty<string>());
                yield return MemberCase(carrier, "ForbidMember(typeof(MarkerAttribute))", OneMarkerMember(), "XMoleculesBricks0008");

                yield return MemberCase(carrier, "RequireNamedMembers(typeof(MarkerAttribute), \"X\", \"Y\")", NamedMarkerMember("X") + NamedMarkerMember("Y"), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireNamedMembers(typeof(MarkerAttribute), \"X\", \"Y\")", NamedMarkerMember("X"), "XMoleculesBricks0010");
                yield return MemberCase(carrier, "RequireNamedMembers(typeof(MarkerAttribute), new[] { \"X\", \"Y\" })", NamedMarkerMember("X") + NamedMarkerMember("Y"), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireNamedMembers(typeof(MarkerAttribute), new string[] { \"X\", \"Y\" })", NamedMarkerMember("X") + NamedMarkerMember("Y"), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireNamedMembers(typeof(MarkerAttribute), \"X\")", NamedMarkerMember("X") + NamedMarkerMember("X", "DuplicateX"), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireNamedMembers(typeof(MarkerAttribute), \"\")", OneMarkerMember(), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireNamedMembers(typeof(MarkerAttribute), \"\")", NamedMarkerMember("X"), "XMoleculesBricks0010");
                yield return MemberCase(carrier, "RequireNamedMembers(typeof(MarkerAttribute), \"X\", NameArgument = \"Slot\")", SlottedMarkerMember("X"), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireNamedMembers(typeof(MarkerAttribute), \"X\", NameArgument = \"Slot\")", NumberedMarkerMember(1), "XMoleculesBricks0010");
                yield return MemberCase(carrier, "RequireNamedMembers(typeof(MarkerAttribute), \"X\", NameArgument = \"Alias\")", NamedMarkerMember("X"), Array.Empty<string>());

                yield return MemberCase(carrier, "RequireUniqueNamedMember(typeof(MarkerAttribute))", NamedMarkerMember("X") + NamedMarkerMember("Y"), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireUniqueNamedMember(typeof(MarkerAttribute))", NamedMarkerMember("X") + NamedMarkerMember("X", "DuplicateX"), "XMoleculesBricks0009");
                yield return MemberCase(carrier, "RequireUniqueNamedMember(typeof(MarkerAttribute), \"Slot\")", SlottedMarkerMember("X") + SlottedMarkerMember("Y"), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireUniqueNamedMember(typeof(MarkerAttribute), \"Slot\")", SlottedMarkerMember("X") + SlottedMarkerMember("X", "DuplicateSlotX"), "XMoleculesBricks0009");
                yield return MemberCase(carrier, "RequireUniqueNamedMember(typeof(MarkerAttribute), \"Alias\")", NamedMarkerMember("X") + NamedMarkerMember("Y"), Array.Empty<string>());
                yield return MemberCase(carrier, "RequireUniqueNamedMember(typeof(MarkerAttribute))", SlottedMarkerMember("X") + SlottedMarkerMember("Y"), "XMoleculesBricks0009");
                yield return MemberCase(carrier, "RequireUniqueNamedMember(typeof(MarkerAttribute), \"Missing\")", NumberedMarkerMember(1) + NumberedMarkerMember(2, "SecondNumber"), "XMoleculesBricks0009");
                yield return MemberCase(carrier, "RequireUniqueNamedMember(typeof(MarkerAttribute))", NamedMarkerMember("   ", "WhitespaceA") + NamedMarkerMember("   ", "WhitespaceB"), "XMoleculesBricks0009");
                yield return MemberCase(carrier, "RequireUniqueNamedMember(typeof(MarkerAttribute))", OneMarkerMember() + "    [Marker]\n    public string DuplicateUnnamed { get; init; } = string.Empty;\n", "XMoleculesBricks0009");
            }
        }

        public static IEnumerable<object[]> MarkedMemberKindCases()
        {
            yield return new object[] { "field", "    [Marker]\n    private readonly string _id = string.Empty;\n" };
            yield return new object[] { "property", "    [Marker]\n    public string Id { get; init; } = string.Empty;\n" };
            yield return new object[] { "method", "    [Marker]\n    public string GetId() => string.Empty;\n" };
            yield return new object[] { "event", "    [Marker]\n    public event EventHandler Changed { add { } remove { } }\n" };
        }

        private static IEnumerable<object[]> DependencyEvidenceCases()
        {
            var cases = new[]
            {
                new DependencyEvidenceCase("field", "    private readonly TargetType _target = default!;\n", string.Empty),
                new DependencyEvidenceCase("property", "    public TargetType Target { get; } = default!;\n", string.Empty),
                new DependencyEvidenceCase("method return", "    public TargetType Create() => default!;\n", string.Empty),
                new DependencyEvidenceCase("method parameter", "    public void Use(TargetType target) { }\n", string.Empty),
                new DependencyEvidenceCase("local declaration", "    public void Use() { TargetType target = default!; _ = target; }\n", string.Empty),
                new DependencyEvidenceCase("object creation", "    public void Use() { var target = new TargetType(); _ = target; }\n", string.Empty),
                new DependencyEvidenceCase("implicit object creation", "    public void Use() { TargetType target = new(); _ = target; }\n", string.Empty),
                new DependencyEvidenceCase("generic type argument", "    private readonly IReadOnlyList<TargetType> _targets = default!;\n", string.Empty),
                new DependencyEvidenceCase("array element", "    private readonly TargetType[] _targets = default!;\n", string.Empty),
                new DependencyEvidenceCase("declared dependency", string.Empty, "[assembly: Dependency(\"D1\", \"SourceType\", \"TargetType\", BrickDependencyKinds.TypeReference)]\n")
            };

            foreach (var roleStyle in new[] { "direct", "alias" })
            {
                foreach (var dependencyCase in cases)
                {
                    yield return new object[] { roleStyle, dependencyCase.Name, dependencyCase.SourceBody, dependencyCase.ExtraAssemblyAttribute };
                }
            }
        }

        private static object[] MetadataCase(string name, string attribute, int expectedCount) =>
            new object[] { name, BuildMetadataSource(attribute), expectedCount };

        private static object[] MemberCase(string carrier, string contract, string members, params string[] expectedIds) =>
            new object[] { carrier, contract, members, expectedIds };

        private static string BuildMetadataSource(string attribute)
        {
            var typeAttribute = attribute.StartsWith("[assembly:", StringComparison.Ordinal) ? string.Empty : attribute;
            var assemblyAttribute = attribute.StartsWith("[assembly:", StringComparison.Ordinal) ? attribute : string.Empty;

            return @"
using System;
using NMolecules.Bricks;
" + assemblyAttribute + @"

public sealed class MarkerAttribute : Attribute
{
    public MarkerAttribute()
    {
    }

    public MarkerAttribute(string name)
    {
        Name = name;
    }

    public MarkerAttribute(int number)
    {
        Number = number;
    }

    public string Name { get; }

    public string Slot { get; set; }

    public int Number { get; }
}

public static class NonConstantValues
{
    public static int Count => 1;
}

" + typeAttribute + @"
public sealed class Sample
{
}

public sealed class SourceType
{
}

public sealed class TargetType
{
}
";
        }

        private static string BuildDependencySource(
            string roleStyle,
            string mode,
            string sourceBody,
            string extraAssemblyAttribute = "")
        {
            var roleDeclarations = roleStyle == "alias"
                ? @"
[AttributeUsage(AttributeTargets.Class)]
[RoleAlias(""Source"")]
public sealed class SourceRoleAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
[RoleAlias(""Target"")]
public sealed class TargetRoleAttribute : Attribute
{
}
"
                : string.Empty;
            var sourceAttribute = roleStyle == "alias" ? "[SourceRole]" : "[Role(\"Source\")]";
            var targetAttribute = roleStyle == "alias" ? "[TargetRole]" : "[Role(\"Target\")]";

            return @"
using System;
using System.Collections.Generic;
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode." + mode + @")]
" + extraAssemblyAttribute + @"
" + roleDeclarations + @"
" + sourceAttribute + @"
public sealed class SourceType
{
" + sourceBody + @"
}

" + targetAttribute + @"
public sealed class TargetType
{
}
";
        }

        private static string BuildMemberContractSource(string carrierStyle, string contractAttribute, string members)
        {
            var carrierDefinition = carrierStyle == "custom"
                ? @"
[AttributeUsage(AttributeTargets.Class)]
[" + contractAttribute + @"]
public sealed class ContractAttribute : Attribute
{
}
"
                : string.Empty;
            var typeAttribute = carrierStyle == "custom" ? "[Contract]" : "[" + contractAttribute + "]";

            return @"
using System;
using NMolecules.Bricks;

public sealed class MarkerAttribute : Attribute
{
    public MarkerAttribute()
    {
    }

    public MarkerAttribute(string name)
    {
        Name = name;
    }

    public MarkerAttribute(int number)
    {
        Number = number;
    }

    public string Name { get; }

    public string Slot { get; set; }

    public int Number { get; }
}

public sealed class OtherMarkerAttribute : Attribute
{
}
" + carrierDefinition + @"
" + typeAttribute + @"
public sealed class Sample
{
" + members + @"
}
";
        }

        private static string OneMarkerMember() =>
            "    [Marker]\n    public string Id { get; init; } = string.Empty;\n";

        private static string TwoMarkerMembers() =>
            OneMarkerMember() + "    [Marker]\n    public string SecondaryId { get; init; } = string.Empty;\n";

        private static string ThreeMarkerMembers() =>
            TwoMarkerMembers() + "    [Marker]\n    public string ThirdId { get; init; } = string.Empty;\n";

        private static string OtherMarkerMember() =>
            "    [OtherMarker]\n    public string Code { get; init; } = string.Empty;\n";

        private static string NamedMarkerMember(string name, string memberName = null) =>
            "    [Marker(\"" + name + "\")]\n    public string " + (memberName ?? "Marker" + name) + " { get; init; } = string.Empty;\n";

        private static string SlottedMarkerMember(string name, string memberName = null) =>
            "    [Marker(Slot = \"" + name + "\")]\n    public string " + (memberName ?? "Slot" + name) + " { get; init; } = string.Empty;\n";

        private static string NumberedMarkerMember(int number, string memberName = null) =>
            "    [Marker(" + number + ")]\n    public string " + (memberName ?? "Number" + number) + " { get; init; } = string.Empty;\n";

        private static string[] DiagnosticIds(IEnumerable<Diagnostic> diagnostics) =>
            diagnostics.Select(diagnostic => diagnostic.Id).OrderBy(id => id).ToArray();

        private static object InvokeAnalyzerFacts(string methodName, params object[] parameters)
        {
            var factsType = typeof(BrickMetadataAnalyzer).Assembly.GetType("NMolecules.Bricks.Analyzers.BrickAnalyzerFacts");
            var method = factsType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            return method.Invoke(null, parameters);
        }

        private static object InvokeAnalyzerFactsPrivate(string methodName, params object[] parameters)
        {
            var factsType = typeof(BrickMetadataAnalyzer).Assembly.GetType("NMolecules.Bricks.Analyzers.BrickAnalyzerFacts");
            var method = factsType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            return method.Invoke(null, parameters);
        }

        private static object InvokePrivateStatic(Type type, string methodName, params object[] parameters)
        {
            var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            return method.Invoke(null, parameters);
        }

        private static TypedConstant CreatePrimitiveTypedConstant(object value)
        {
            var constructor = typeof(TypedConstant)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(candidate => candidate.GetParameters().Length == 3);

            return (TypedConstant)constructor.Invoke(new[] { null, TypedConstantKind.Primitive, value });
        }

        private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp10));
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
                .Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path))
                .Concat(new[] { MetadataReference.CreateFromFile(typeof(RoleAttribute).Assembly.Location) })
                .ToArray();
            var compilation = CSharpCompilation.Create(
                "AnalyzerCoverageFixture",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithAllowUnsafe(true));
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
                new BrickMetadataAnalyzer(),
                new BrickNamespaceRoleMetadataAnalyzer(),
                new BrickDependencyRuleAnalyzer(),
                new BrickMemberContractAnalyzer());
            var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);

            return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
        }

        private static async Task<IReadOnlyList<Diagnostic>> AnalyzeDocumentationAsync(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp10));
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
                .Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path))
                .ToArray();
            var compilation = CSharpCompilation.Create(
                "DocumentationAnalyzerCoverageFixture",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new BrickXmlDocumentationAnalyzer());
            var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);

            return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
        }

        private readonly struct DependencyEvidenceCase
        {
            public DependencyEvidenceCase(string name, string sourceBody, string extraAssemblyAttribute)
            {
                Name = name;
                SourceBody = sourceBody;
                ExtraAssemblyAttribute = extraAssemblyAttribute;
            }

            public string Name { get; }

            public string SourceBody { get; }

            public string ExtraAssemblyAttribute { get; }
        }

        private sealed class NullAttributeData : AttributeData
        {
            protected override INamedTypeSymbol CommonAttributeClass => null;

            protected override IMethodSymbol CommonAttributeConstructor => null;

            protected override SyntaxReference CommonApplicationSyntaxReference => null;

            protected override ImmutableArray<TypedConstant> CommonConstructorArguments =>
                ImmutableArray<TypedConstant>.Empty;

            protected override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments =>
                ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty;
        }

        private sealed class AttributeDataStub : AttributeData
        {
            private readonly ImmutableArray<TypedConstant> constructorArguments;
            private readonly ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments;

            public AttributeDataStub(
                ImmutableArray<TypedConstant> constructorArguments,
                ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments)
            {
                this.constructorArguments = constructorArguments;
                this.namedArguments = namedArguments;
            }

            protected override INamedTypeSymbol CommonAttributeClass => null;

            protected override IMethodSymbol CommonAttributeConstructor => null;

            protected override SyntaxReference CommonApplicationSyntaxReference => null;

            protected override ImmutableArray<TypedConstant> CommonConstructorArguments => constructorArguments;

            protected override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments => namedArguments;
        }
    }
}
