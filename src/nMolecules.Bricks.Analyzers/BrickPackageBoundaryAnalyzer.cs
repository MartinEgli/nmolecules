using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Validates Bricks package boundaries that can be inferred from referenced assemblies.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickPackageBoundaryAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Gets the diagnostics produced by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(BrickAnalyzerDiagnostics.BrickConfiguration);

        /// <summary>
        /// Registers compilation analysis for package reference boundaries.
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
            var assemblyName = context.Compilation.AssemblyName ?? string.Empty;
            var references = context.Compilation.ReferencedAssemblyNames
                .Select(reference => reference.Name ?? string.Empty)
                .ToArray();

            if (assemblyName.Equals("NMolecules.Bricks", StringComparison.OrdinalIgnoreCase) &&
                references.Any(IsOptionalBricksPackage))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    BrickAnalyzerDiagnostics.BrickConfiguration,
                    Location.None,
                    "NMolecules.Bricks must not reference optional Bricks analyzer, AI, or extension packages"));
            }

            if (assemblyName.Equals("NMolecules.Bricks.Analyzers", StringComparison.OrdinalIgnoreCase) &&
                references.Any(reference => reference.Equals("NMolecules.Bricks", StringComparison.OrdinalIgnoreCase)))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    BrickAnalyzerDiagnostics.BrickConfiguration,
                    Location.None,
                    "NMolecules.Bricks.Analyzers must not reference the runtime Bricks package directly"));
            }
        }

        private static bool IsOptionalBricksPackage(string reference) =>
            reference.Equals("NMolecules.Bricks.Analyzers", StringComparison.OrdinalIgnoreCase) ||
            reference.Equals("NMolecules.Bricks.Ai", StringComparison.OrdinalIgnoreCase) ||
            reference.Equals("NMolecules.Bricks.Extensions", StringComparison.OrdinalIgnoreCase);
    }
}
