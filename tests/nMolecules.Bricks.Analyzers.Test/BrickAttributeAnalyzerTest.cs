using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NMolecules.Bricks.Analyzers;
using Xunit;

namespace NMolecules.Bricks.Analyzers.Test
{
    public class BrickAttributeAnalyzerTest
    {
        [Fact]
        public async Task ReportsEmptyRoleRuleAndDependencyMetadata()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[Role("""")]
public class EmptyRole;

[Rule("""", """", """")]
public class EmptyRule;

[Dependency("""", """", """", """")]
public class EmptyDependency;
");

            Assert.Equal(
                new[]
                {
                    "XMoleculesBricks0001",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0002",
                    "XMoleculesBricks0003",
                    "XMoleculesBricks0003",
                    "XMoleculesBricks0003",
                    "XMoleculesBricks0003"
                },
                diagnostics.Select(diagnostic => diagnostic.Id).ToArray());
        }

        [Fact]
        public async Task AcceptsValidBricksMetadata()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[Role(""Domain"")]
public class DomainType;

[Rule(""BRK-001"", ""Domain"", ""Infrastructure"")]
public class RuleHolder;

[Dependency(""BRK-DEP-001"", ""type:Domain"", ""type:Infrastructure"")]
public class DependencyHolder;
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
            var analyzer = new BrickAttributeAnalyzer();
            var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

            return (await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync()).OrderBy(diagnostic => diagnostic.Id).ToArray();
        }
    }
}
