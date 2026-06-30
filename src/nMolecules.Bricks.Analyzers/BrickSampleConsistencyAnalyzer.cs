using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Entry point for Bricks sample consistency analysis.
    /// </summary>
    /// <remarks>
    /// Sample consistency is currently enforced by repository tests because the samples live in
    /// Markdown documentation rather than in normal compilations. This analyzer entry point keeps
    /// the capability visible to hosts that enumerate Bricks analyzers and can be extended to read
    /// analyzer additional files later.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickSampleConsistencyAnalyzer : DiagnosticAnalyzer
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
