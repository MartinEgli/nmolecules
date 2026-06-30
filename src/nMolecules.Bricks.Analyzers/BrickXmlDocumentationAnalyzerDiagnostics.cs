using Microsoft.CodeAnalysis;

namespace NMolecules.Bricks.Analyzers
{
    internal static class BrickXmlDocumentationAnalyzerDiagnostics
    {
        private const string Category = "nMolecules.Bricks.Documentation";

        public static readonly DiagnosticDescriptor MissingXmlDocumentation = new DiagnosticDescriptor(
            "XMoleculesBricks0011",
            "Public Bricks API should be documented",
            "{0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Public framework APIs should provide XML documentation so generated developer documentation, samples and tests can be linked from the API surface.");
    }
}
