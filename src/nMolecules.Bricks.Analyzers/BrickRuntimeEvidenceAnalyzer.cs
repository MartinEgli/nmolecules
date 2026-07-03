using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Validates runtime Bricks dependency evidence declarations.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickRuntimeEvidenceAnalyzer : DiagnosticAnalyzer
    {
        private const int RuntimeLayer = 2;
        private const int RuntimeInferredEvidence = 4;

        /// <summary>
        /// Gets the diagnostics produced by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(
                BrickAnalyzerDiagnostics.BrickConfiguration,
                BrickAnalyzerDiagnostics.BrickEvidenceConfiguration);

        /// <summary>
        /// Registers compilation analysis for declared runtime dependencies.
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
            foreach (var attribute in BrickAnalyzerAttributeUtilities.GetAttributes(context.Compilation, BrickAnalyzerFacts.DependencyAttribute))
            {
                var layer = BrickAnalyzerFacts.GetAttributeEnum(attribute, 5, "Layer", 0);
                var evidenceLevel = BrickAnalyzerFacts.GetAttributeEnum(attribute, 7, "EvidenceLevel", 1);
                if (layer == RuntimeLayer && evidenceLevel < RuntimeInferredEvidence)
                {
                    context.ReportDiagnostic(BrickDiagnosticProperties.Create(
                        BrickAnalyzerDiagnostics.BrickEvidenceConfiguration,
                        BrickAnalyzerAttributeUtilities.GetLocation(attribute),
                        "Runtime DependencyAttribute evidence must use RuntimeInferred evidence level",
                        configurationKind: "Evidence",
                        source: BrickAnalyzerFacts.GetAttributeString(attribute, 1, "Source"),
                        target: BrickAnalyzerFacts.GetAttributeString(attribute, 2, "Target")));
                }
            }
        }
    }
}
