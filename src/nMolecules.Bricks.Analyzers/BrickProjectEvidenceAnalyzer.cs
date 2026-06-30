using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Entry point for project-level Bricks evidence analysis.
    /// </summary>
    /// <remarks>
    /// Project evidence is currently represented through assembly, module, and type metadata
    /// consumed by the dependency and metadata analyzers. The dedicated entry point is kept
    /// non-reporting until project-system evidence is supplied through MSBuild analyzer options.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickProjectEvidenceAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Gets the diagnostics produced directly by this entry point.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray<DiagnosticDescriptor>.Empty;

        /// <summary>
        /// Initializes the analyzer entry point.
        /// </summary>
        /// <param name="context">The Roslyn analysis context.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
        }
    }
}
