using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Entry point for inheritance-specific Bricks dependency analysis.
    /// </summary>
    /// <remarks>
    /// Inheritance, implemented interfaces, inherited interfaces, and generic constraints are
    /// currently evaluated by <see cref="BrickDependencyRuleAnalyzer"/> to avoid duplicate
    /// diagnostics when the analyzer package is loaded by MSBuild or Visual Studio.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickInheritanceDependencyAnalyzer : DiagnosticAnalyzer
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
