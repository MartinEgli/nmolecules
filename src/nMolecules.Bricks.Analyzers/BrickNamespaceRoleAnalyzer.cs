using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Validates namespace-to-role Bricks mappings against the namespaces present in the compilation.
    /// </summary>
    /// <remarks>
    /// Empty namespace role declarations are handled by <see cref="BrickNamespaceRoleMetadataAnalyzer"/>.
    /// This analyzer reports mappings that are syntactically usable but do not match any declared
    /// namespace in the current project.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickNamespaceRoleAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Gets the diagnostics produced directly by this entry point.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(
                BrickAnalyzerDiagnostics.BrickConfiguration,
                BrickAnalyzerDiagnostics.BrickNamespaceRoleConfiguration);

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
            var namespaces = BrickAnalyzerAttributeUtilities.GetDeclaredTypes(context.Compilation)
                .Select(type => type.ContainingNamespace?.ToDisplayString())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(System.StringComparer.Ordinal)
                .ToArray();

            foreach (var attribute in BrickAnalyzerAttributeUtilities.GetAttributes(context.Compilation, "NMolecules.Bricks.NamespaceRoleAttribute"))
            {
                var namespacePattern = BrickAnalyzerFacts.GetAttributeString(attribute, 0, "NamespacePattern");
                var role = BrickAnalyzerFacts.GetAttributeString(attribute, 1, "Role");
                if (string.IsNullOrWhiteSpace(namespacePattern) ||
                    string.IsNullOrWhiteSpace(role) ||
                    NamespaceMatchesAny(namespacePattern, namespaces))
                {
                    continue;
                }

                context.ReportDiagnostic(BrickDiagnosticProperties.Create(
                    BrickAnalyzerDiagnostics.BrickNamespaceRoleConfiguration,
                    BrickAnalyzerAttributeUtilities.GetLocation(attribute),
                    $"NamespaceRoleAttribute pattern '{namespacePattern}' does not match any declared namespace",
                    configurationKind: "NamespaceRole",
                    source: namespacePattern,
                    target: role));
            }
        }

        private static bool NamespaceMatchesAny(string pattern, string[] namespaces)
        {
            if (pattern == "*")
            {
                return namespaces.Length > 0;
            }

            if (pattern.EndsWith("*", System.StringComparison.Ordinal))
            {
                var prefix = pattern.Substring(0, pattern.Length - 1);
                return namespaces.Any(name => name.StartsWith(prefix, System.StringComparison.Ordinal));
            }

            return namespaces.Any(name => string.Equals(name, pattern, System.StringComparison.Ordinal));
        }
    }
}
