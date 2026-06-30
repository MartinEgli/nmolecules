using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Entry point for default Bricks policy analysis.
    /// </summary>
    /// <remarks>
    /// Default-deny dependency evaluation is executed by <see cref="BrickDependencyRuleAnalyzer"/>
    /// so that projects receive one deterministic diagnostic per uncovered dependency.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickDefaultPolicyAnalyzer : DiagnosticAnalyzer
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
