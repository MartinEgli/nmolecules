using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Validates folder-style Bricks evidence that is declared through namespace role metadata.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickFolderEvidenceAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Gets the diagnostics produced by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(
                BrickAnalyzerDiagnostics.BrickConfiguration,
                BrickAnalyzerDiagnostics.BrickEvidenceConfiguration);

        /// <summary>
        /// Registers compilation analysis for namespace patterns that accidentally use file-system paths.
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
            foreach (var attribute in BrickAnalyzerAttributeUtilities.GetAttributes(context.Compilation, "NMolecules.Bricks.NamespaceRoleAttribute"))
            {
                var namespacePattern = BrickAnalyzerFacts.GetAttributeString(attribute, 0, "NamespacePattern");
                if (namespacePattern.Contains("/") || namespacePattern.Contains("\\"))
                {
                    context.ReportDiagnostic(BrickDiagnosticProperties.Create(
                        BrickAnalyzerDiagnostics.BrickEvidenceConfiguration,
                        BrickAnalyzerAttributeUtilities.GetLocation(attribute),
                        "NamespaceRoleAttribute must use namespace patterns, not folder paths",
                        configurationKind: "Evidence",
                        source: namespacePattern,
                        target: "NamespacePattern"));
                }
            }
        }
    }
}
