using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using NMolecules.Bricks.Analyzers;
using Xunit;

namespace NMolecules.Bricks.Analyzers.Test
{
    public class BrickRequestedAnalyzerSetTest
    {
        [Fact]
        public void RequestedAnalyzerPrioritySetIsAvailable()
        {
            var analyzerTypes = typeof(BrickDependencyRuleAnalyzer).Assembly
                .GetTypes()
                .Where(type => typeof(DiagnosticAnalyzer).IsAssignableFrom(type) && !type.IsAbstract)
                .Select(type => type.Name)
                .OrderBy(name => name)
                .ToArray();

            Assert.Contains("BrickInheritanceDependencyAnalyzer", analyzerTypes);
            Assert.Contains("BrickDefaultPolicyAnalyzer", analyzerTypes);
            Assert.Contains("BrickRuleFilterAnalyzer", analyzerTypes);
            Assert.Contains("BrickNamespaceRoleAnalyzer", analyzerTypes);
            Assert.Contains("BrickProjectEvidenceAnalyzer", analyzerTypes);
            Assert.Contains("BrickXmlDocumentationAnalyzer", analyzerTypes);
            Assert.Contains("BrickSampleConsistencyAnalyzer", analyzerTypes);
            Assert.Contains("BrickPackageBoundaryAnalyzer", analyzerTypes);
            Assert.Contains("BrickFolderEvidenceAnalyzer", analyzerTypes);
            Assert.Contains("BrickRuntimeEvidenceAnalyzer", analyzerTypes);
        }

        [Fact]
        public async Task FolderEvidenceAnalyzerReportsFolderPathNamespaceRolePatterns()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: NamespaceRole(""Domain/Orders"", ""Domain"")]
",
                new BrickFolderEvidenceAnalyzer());

            Assert.Equal(new[] { "XMoleculesBricks0002" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task RuntimeEvidenceAnalyzerReportsRuntimeDependencyWithoutRuntimeEvidence()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Dependency(
    ""D1"",
    ""SourceType"",
    ""TargetType"",
    ""RuntimeActivation"",
    layer: BrickDependencyLayer.Runtime,
    evidenceLevel: BrickEvidenceLevel.ConfigurationDeclared)]
",
                new BrickRuntimeEvidenceAnalyzer());

            Assert.Equal(new[] { "XMoleculesBricks0002" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DefaultPolicyAnalyzerReportsConflictingDefaultDecisions()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Policy(""P1"", defaultDecision: BrickPermissionDefault.Allow)]
[assembly: Policy(""P1"", defaultDecision: BrickPermissionDefault.Deny)]
",
                new BrickDefaultPolicyAnalyzer());

            Assert.Equal(new[] { "XMoleculesBricks0002" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task RuleFilterAnalyzerReportsUnknownRuleReference()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: RequiredSourceNameContains(""MissingRule"", ""Generated"")]
",
                new BrickRuleFilterAnalyzer());

            Assert.Equal(new[] { "XMoleculesBricks0002" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task NamespaceRoleAnalyzerReportsPatternWithoutMatchingNamespace()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: NamespaceRole(""Sales.*"", ""Domain"")]

namespace Billing.Orders
{
    public sealed class Order { }
}
",
                new BrickNamespaceRoleAnalyzer());

            Assert.Equal(new[] { "XMoleculesBricks0002" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task ProjectEvidenceAnalyzerReportsDependencyEndpointsWithoutTypeEvidence()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Dependency(""D1"", ""SourceType"", ""MissingTarget"", ""TypeReference"")]

public sealed class SourceType { }
",
                new BrickProjectEvidenceAnalyzer());

            Assert.Equal(new[] { "XMoleculesBricks0002" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task InheritanceDependencyAnalyzerReportsUncoveredRoledInheritance()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[Role(""Application"")]
public sealed class ApplicationService : DomainService { }

[Role(""Domain"")]
public class DomainService { }
",
                new BrickInheritanceDependencyAnalyzer());

            Assert.Equal(new[] { "XMoleculesBricks0002" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyRuleAnalyzerPropagatesBaseTypeRolesToDerivedTypes()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Contracts"", ""Infrastructure"", RuleMode.ForbidDependency)]

[Role(""Contracts"")]
public abstract class ContractMessageBase { }

public sealed class InvoiceDto : ContractMessageBase
{
    private readonly SqlGateway _gateway = default!;
}

[Role(""Infrastructure"")]
public sealed class SqlGateway { }
",
                new BrickDependencyRuleAnalyzer());

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyRuleAnalyzerPropagatesInterfaceRolesToImplementations()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Contracts"", ""Infrastructure"", RuleMode.ForbidDependency)]

[Role(""Contracts"")]
public interface IContractMessage { }

public sealed class InvoiceDto : IContractMessage
{
    private readonly SqlGateway _gateway = default!;
}

[Role(""Infrastructure"")]
public sealed class SqlGateway { }
",
                new BrickDependencyRuleAnalyzer());

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DependencyRuleAnalyzerPropagatesInheritedInterfaceRolesToImplementations()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Contracts"", ""Infrastructure"", RuleMode.ForbidDependency)]

[Role(""Contracts"")]
public interface IContractMessage { }

public interface IInvoiceContract : IContractMessage { }

public sealed class InvoiceDto : IInvoiceContract
{
    private readonly SqlGateway _gateway = default!;
}

[Role(""Infrastructure"")]
public sealed class SqlGateway { }
",
                new BrickDependencyRuleAnalyzer());

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task SampleConsistencyAnalyzerReportsInvalidAnalyzerSampleMarker()
        {
            var diagnostics = await AnalyzeAsync(
                "public sealed class Sample { }",
                new BrickSampleConsistencyAnalyzer(),
                additionalFiles: new[] { new InMemoryAdditionalText("sample.md", "```csharp analyzer-broken\npublic class Broken { }\n```") });

            Assert.Equal(new[] { "XMoleculesBricks0002" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task PackageBoundaryAnalyzerReportsAnalyzerRuntimeReference()
        {
            var diagnostics = await AnalyzeAsync(
                "public sealed class Sample { }",
                new BrickPackageBoundaryAnalyzer(),
                "NMolecules.Bricks.Analyzers");

            Assert.Equal(new[] { "XMoleculesBricks0002" }, DiagnosticIds(diagnostics));
        }

        private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync(
            string source,
            DiagnosticAnalyzer analyzer,
            string assemblyName = "RequestedAnalyzerFixture",
            IEnumerable<AdditionalText> additionalFiles = null)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp10));
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
                .Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path))
                .Concat(new[] { MetadataReference.CreateFromFile(typeof(RoleAttribute).Assembly.Location) })
                .ToArray();
            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var analyzerOptions = new AnalyzerOptions((additionalFiles ?? Enumerable.Empty<AdditionalText>()).ToImmutableArray());
            var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer), analyzerOptions);

            return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
        }

        private static string[] DiagnosticIds(IEnumerable<Diagnostic> diagnostics) =>
            diagnostics.Select(diagnostic => diagnostic.Id).OrderBy(id => id).ToArray();

        private sealed class InMemoryAdditionalText : AdditionalText
        {
            private readonly SourceText text;

            public InMemoryAdditionalText(string path, string text)
            {
                Path = path;
                this.text = SourceText.From(text);
            }

            public override string Path { get; }

            public override SourceText GetText(System.Threading.CancellationToken cancellationToken = default) => text;
        }
    }
}
