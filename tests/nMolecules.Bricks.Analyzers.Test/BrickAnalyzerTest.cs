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
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002"
                },
                diagnostics.Select(diagnostic => diagnostic.Id).OrderBy(id => id).ToArray());
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
            var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp10));
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
                .Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path))
                .Concat(new[] { MetadataReference.CreateFromFile(typeof(RoleAttribute).Assembly.Location) })
                .ToArray();
            var compilation = CSharpCompilation.Create(
                "AnalyzerFixture",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
                new BrickMetadataAnalyzer(),
                new BrickDependencyRuleAnalyzer(),
                new BrickMemberContractAnalyzer());
            var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);

            return (await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync()).OrderBy(diagnostic => diagnostic.Id).ToArray();
        }
    }
}
