using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Entry point for rule-filter-specific Bricks analysis.
    /// </summary>
    /// <remarks>
    /// Rule filter application is evaluated by <see cref="BrickDependencyRuleAnalyzer"/> and
    /// conflicting filter declarations are validated by <see cref="BrickMetadataAnalyzer"/>.
    /// Keeping this entry point non-reporting prevents duplicate diagnostics while preserving a
    /// stable analyzer surface for tooling and documentation.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickRuleFilterAnalyzer : DiagnosticAnalyzer
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
