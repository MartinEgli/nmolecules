using Microsoft.CodeAnalysis;

namespace NMolecules.Bricks.Analyzers
{
    internal static class BrickAnalyzerDiagnostics
    {
        private const string Category = "nMolecules.Bricks";

        public static readonly DiagnosticDescriptor EmptyRoleName = new DiagnosticDescriptor(
            "XMoleculesBricks0001",
            "Brick role name must not be empty",
            "Brick role attribute on '{0}' must declare a non-empty role name",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "RoleAttribute needs a non-empty role name so Bricks role resolution and Visual Studio diagnostics can reason about the annotated type.");

        public static readonly DiagnosticDescriptor EmptyRulePart = new DiagnosticDescriptor(
            "XMoleculesBricks0002",
            "Brick rule must declare id, source role, and target role",
            "Brick rule attribute must declare non-empty {0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "RuleAttribute needs id, source role, and target role so Bricks can produce deterministic diagnostics.");

        public static readonly DiagnosticDescriptor EmptyDependencyPart = new DiagnosticDescriptor(
            "XMoleculesBricks0003",
            "Brick dependency must declare id, source, target, and kind",
            "Brick dependency attribute must declare non-empty {0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "DependencyAttribute needs id, source, target, and kind so dependency evidence can be evaluated by Bricks policies.");
    }
}
