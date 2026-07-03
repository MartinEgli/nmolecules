using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers.Test
{
    internal static class BrickAnalyzerTestFixture
    {
        public static readonly CSharpParseOptions ParseOptions = new CSharpParseOptions(LanguageVersion.CSharp10);

        public static readonly CSharpCompilationOptions LibraryOptions =
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        public static readonly CSharpCompilationOptions UnsafeLibraryOptions =
            LibraryOptions.WithAllowUnsafe(true);

        private static readonly ImmutableArray<MetadataReference> PlatformReferences =
            CreateReferences(includeBricks: false);

        private static readonly ImmutableArray<MetadataReference> BricksReferences =
            CreateReferences(includeBricks: true);

        public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
            string assemblyName,
            string source,
            ImmutableArray<DiagnosticAnalyzer> analyzers,
            CSharpCompilationOptions options = null,
            bool includeBricks = true,
            AnalyzerOptions analyzerOptions = null)
        {
            var compilation = CreateCompilation(
                assemblyName,
                source,
                options ?? LibraryOptions,
                includeBricks);

            var compilationWithAnalyzers = analyzerOptions == null
                ? compilation.WithAnalyzers(analyzers)
                : compilation.WithAnalyzers(analyzers, analyzerOptions);

            return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
        }

        public static CSharpCompilation CreateCompilation(
            string assemblyName,
            string source,
            CSharpCompilationOptions options = null,
            bool includeBricks = true)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source, ParseOptions);
            return CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                includeBricks ? BricksReferences : PlatformReferences,
                options ?? LibraryOptions);
        }

        private static ImmutableArray<MetadataReference> CreateReferences(bool includeBricks)
        {
            var trustedAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
            var references = trustedAssemblies
                .Split(Path.PathSeparator)
                .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path));

            if (includeBricks)
            {
                references = references.Concat(new[] { MetadataReference.CreateFromFile(typeof(RoleAttribute).Assembly.Location) });
            }

            return references.ToImmutableArray();
        }
    }
}
