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
    public class BrickSourceTargetMatrixExecutableTest
    {
        [Theory]
        [MemberData(nameof(RoslynSupportedSourceTargetCases))]
        public async Task RoslynSupportedSourceTargetMatrixReportsForbiddenDependencies(
            string sourceShape,
            string targetShape,
            string sourceDeclaration,
            string targetDeclaration)
        {
            Assert.False(string.IsNullOrWhiteSpace(sourceShape));
            Assert.False(string.IsNullOrWhiteSpace(targetShape));

            var diagnostics = await AnalyzeAsync(BuildSource(sourceDeclaration, targetDeclaration));

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task RoslynSupportedMatrixReportsInheritedInterfaceTargetDeclaration()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]

[Role(""Source"")]
public sealed class SourceType : ITargetType
{
}

public interface ITargetType : ITargetBase
{
}

[Role(""Target"")]
public interface ITargetBase
{
}
");

            Assert.Equal(new[] { "XMoleculesBricks0001" }, DiagnosticIds(diagnostics));
        }

        public static IEnumerable<object[]> RoslynSupportedSourceTargetCases()
        {
            foreach (var sourceShape in SourceShapes())
            {
                foreach (var targetShape in TargetShapes())
                {
                    yield return new object[]
                    {
                        sourceShape.Name,
                        targetShape.Name,
                        sourceShape.BuildSourceDeclaration(targetShape.UsageType),
                        targetShape.Declaration
                    };
                }
            }
        }

        private static IEnumerable<SourceShape> SourceShapes()
        {
            yield return new SourceShape(
                "type",
                usageType => @"
[Role(""Source"")]
public sealed class SourceType
{
    private readonly " + usageType + @" _target = default!;
}
");

            yield return new SourceShape(
                "derived type",
                usageType => @"
[Role(""Source"")]
public abstract class SourceBase
{
}

public sealed class SourceType : SourceBase
{
    private readonly " + usageType + @" _target = default!;
}
");

            yield return new SourceShape(
                "interface",
                usageType => @"
[Role(""Source"")]
public interface ISourceType
{
    " + usageType + @" Target { get; }
}
");

            yield return new SourceShape(
                "member",
                usageType => @"
[Role(""Source"")]
public sealed class SourceType
{
    public " + usageType + @" Create() => default!;
}
");

            yield return new SourceShape(
                "property",
                usageType => @"
[Role(""Source"")]
public sealed class SourceType
{
    public " + usageType + @" Target { get; } = default!;
}
");

            yield return new SourceShape(
                "constructor",
                usageType => @"
[Role(""Source"")]
public sealed class SourceType
{
    public SourceType(" + usageType + @" target)
    {
        _ = target;
    }
}
");

            yield return new SourceShape(
                "destructor",
                usageType => @"
[Role(""Source"")]
public sealed class SourceType
{
    ~SourceType()
    {
        " + usageType + @" target = default!;
        _ = target;
    }
}
");
        }

        private static IEnumerable<TargetShape> TargetShapes()
        {
            yield return new TargetShape(
                "type",
                "TargetType",
                @"
[Role(""Target"")]
public sealed class TargetType
{
}
");

            yield return new TargetShape(
                "derived type",
                "TargetType",
                @"
[Role(""Target"")]
public abstract class TargetBase
{
}

public sealed class TargetType : TargetBase
{
}
");

            yield return new TargetShape(
                "interface",
                "ITargetType",
                @"
[Role(""Target"")]
public interface ITargetType
{
}
");
        }

        private static string BuildSource(string sourceDeclaration, string targetDeclaration) => @"
using NMolecules.Bricks;

[assembly: Rule(""R1"", ""Source"", ""Target"", RuleMode.ForbidDependency)]
" + sourceDeclaration + targetDeclaration;

        private static string[] DiagnosticIds(IEnumerable<Diagnostic> diagnostics) =>
            diagnostics.Select(diagnostic => diagnostic.Id).OrderBy(id => id).ToArray();

        private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp10));
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
                .Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path))
                .Concat(new[] { MetadataReference.CreateFromFile(typeof(RoleAttribute).Assembly.Location) })
                .ToArray();
            var compilation = CSharpCompilation.Create(
                "SourceTargetMatrixFixture",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
                new BrickMetadataAnalyzer(),
                new BrickNamespaceRoleMetadataAnalyzer(),
                new BrickDependencyRuleAnalyzer());
            var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);

            return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
        }

        private readonly struct SourceShape
        {
            public SourceShape(string name, Func<string, string> buildSourceDeclaration)
            {
                Name = name;
                BuildSourceDeclaration = buildSourceDeclaration;
            }

            public string Name { get; }

            public Func<string, string> BuildSourceDeclaration { get; }
        }

        private readonly struct TargetShape
        {
            public TargetShape(string name, string usageType, string declaration)
            {
                Name = name;
                UsageType = usageType;
                Declaration = declaration;
            }

            public string Name { get; }

            public string UsageType { get; }

            public string Declaration { get; }
        }
    }
}
