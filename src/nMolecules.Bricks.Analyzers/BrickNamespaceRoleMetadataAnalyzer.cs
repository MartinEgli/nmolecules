using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickNamespaceRoleMetadataAnalyzer : DiagnosticAnalyzer
    {
        private const string NamespaceRoleAttributeName = "NMolecules.Bricks.NamespaceRoleAttribute";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(BrickAnalyzerDiagnostics.BrickConfiguration);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var attribute = (AttributeSyntax)context.Node;
            var symbol = context.SemanticModel.GetSymbolInfo(attribute, context.CancellationToken).Symbol as IMethodSymbol;
            var attributeType = symbol?.ContainingType;
            if (attributeType == null || BrickAnalyzerFacts.ToMetadataName(attributeType) != NamespaceRoleAttributeName)
            {
                return;
            }

            ReportIfEmpty(context, attribute, "NamespaceRoleAttribute must declare a non-empty namespace pattern", 0, "namespacePattern");
            ReportIfEmpty(context, attribute, "NamespaceRoleAttribute must declare a non-empty role", 1, "role");
        }

        private static void ReportIfEmpty(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            string message,
            int ordinal,
            params string[] names)
        {
            var value = BrickAnalyzerFacts.GetStringArgument(context, attribute, ordinal, names);
            if (value != null && value.Trim().Length == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    BrickAnalyzerDiagnostics.BrickConfiguration,
                    attribute.GetLocation(),
                    message));
            }
        }
    }
}
