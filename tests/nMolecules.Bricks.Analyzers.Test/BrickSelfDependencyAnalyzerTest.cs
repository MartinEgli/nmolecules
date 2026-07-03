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
    public sealed class BrickSelfDependencyAnalyzerTest
    {
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
        }

        private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync(string source)
        {
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new BrickDependencyRuleAnalyzer());
            var diagnostics = await BrickAnalyzerTestFixture.AnalyzeAsync(
                "AnalyzerFixture",
                source,
                analyzers);

            return diagnostics.OrderBy(diagnostic => diagnostic.Id).ToArray();
        }
    }
}
