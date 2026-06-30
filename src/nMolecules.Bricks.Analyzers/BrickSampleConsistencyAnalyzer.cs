using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Validates Bricks analyzer sample markers supplied through analyzer additional files.
    /// </summary>
    /// <remarks>
    /// The analyzer inspects Markdown additional files for <c>```csharp analyzer-...</c> fences
    /// and reports markers that cannot be mapped to pass or violation expectations.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickSampleConsistencyAnalyzer : DiagnosticAnalyzer
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
            foreach (var additionalFile in context.Options.AdditionalFiles.Where(file => file.Path.EndsWith(".md", System.StringComparison.OrdinalIgnoreCase)))
            {
                var text = additionalFile.GetText(context.CancellationToken);
                if (text == null)
                {
                    continue;
                }

                foreach (var line in text.Lines)
                {
                    var value = line.ToString().Trim();
                    if (!value.StartsWith("```csharp analyzer-", System.StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var marker = value.Substring("```csharp ".Length).Trim();
                    if (IsValidMarker(marker))
                    {
                        continue;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(
                        BrickAnalyzerDiagnostics.BrickConfiguration,
                        Location.None,
                        $"Analyzer sample marker '{marker}' must be 'analyzer-pass' or 'analyzer-violation <diagnostic-id>...'"));
                }
            }
        }

        private static bool IsValidMarker(string marker)
        {
            var parts = marker.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1 && parts[0] == "analyzer-pass")
            {
                return true;
            }

            return parts.Length >= 2 &&
                parts[0] == "analyzer-violation" &&
                parts.Skip(1).All(IsDiagnosticId);
        }

        private static bool IsDiagnosticId(string value) =>
            !string.IsNullOrWhiteSpace(value) &&
            value.StartsWith("XMoleculesBricks", System.StringComparison.Ordinal);
    }
}
