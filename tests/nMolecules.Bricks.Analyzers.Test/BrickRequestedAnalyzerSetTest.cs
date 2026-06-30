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
            string assemblyName = "RequestedAnalyzerFixture")
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
            var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));

            return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
        }

        private static string[] DiagnosticIds(IEnumerable<Diagnostic> diagnostics) =>
            diagnostics.Select(diagnostic => diagnostic.Id).OrderBy(id => id).ToArray();
    }
}
