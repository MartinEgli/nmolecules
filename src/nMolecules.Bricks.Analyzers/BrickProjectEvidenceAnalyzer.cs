using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Validates project-level Bricks evidence declared through assembly and module attributes.
    /// </summary>
    /// <remarks>
    /// The analyzer checks whether declared dependency endpoints can be resolved to source types in
    /// the current project. Dependency rule evaluation ignores unknown endpoints, so this analyzer
    /// makes that evidence gap visible to framework users.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickProjectEvidenceAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Gets the diagnostics produced directly by this entry point.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(BrickAnalyzerDiagnostics.BrickConfiguration);

        /// <summary>
        /// Initializes the analyzer entry point.
        /// </summary>
        /// <param name="context">The Roslyn analysis context.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationAction(AnalyzeCompilation);
        }

        private static void AnalyzeCompilation(CompilationAnalysisContext context)
        {
            var types = BrickAnalyzerAttributeUtilities.GetDeclaredTypes(context.Compilation).ToArray();
            foreach (var attribute in BrickAnalyzerAttributeUtilities.GetAttributes(context.Compilation, BrickAnalyzerFacts.DependencyAttribute))
            {
                var sourceName = BrickAnalyzerFacts.GetAttributeString(attribute, 1, "Source");
                var targetName = BrickAnalyzerFacts.GetAttributeString(attribute, 2, "Target");
                if (string.IsNullOrWhiteSpace(sourceName) ||
                    string.IsNullOrWhiteSpace(targetName))
                {
                    continue;
                }

                if (FindType(types, sourceName) == null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        BrickAnalyzerDiagnostics.BrickConfiguration,
                        BrickAnalyzerAttributeUtilities.GetLocation(attribute),
                        $"DependencyAttribute source '{sourceName}' does not match any declared type"));
                }

                if (FindType(types, targetName) == null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        BrickAnalyzerDiagnostics.BrickConfiguration,
                        BrickAnalyzerAttributeUtilities.GetLocation(attribute),
                        $"DependencyAttribute target '{targetName}' does not match any declared type"));
                }
            }
        }

        private static INamedTypeSymbol FindType(INamedTypeSymbol[] types, string name) =>
            types.FirstOrDefault(type =>
                type.Name == name ||
                type.ToDisplayString() == name ||
                type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty) == name);
    }
}
