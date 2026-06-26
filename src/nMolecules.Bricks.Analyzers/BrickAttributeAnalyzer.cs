using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(
                BrickAnalyzerDiagnostics.EmptyRoleName,
                BrickAnalyzerDiagnostics.EmptyRulePart,
                BrickAnalyzerDiagnostics.EmptyDependencyPart);

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
            if (attributeType == null)
            {
                return;
            }

            if (IsOrDerivesFrom(attributeType, "NMolecules.Bricks.RoleAttribute"))
            {
                AnalyzeRoleAttribute(context, attribute);
                return;
            }

            if (IsOrDerivesFrom(attributeType, "NMolecules.Bricks.RuleAttribute"))
            {
                AnalyzeRuleAttribute(context, attribute);
                return;
            }

            if (IsOrDerivesFrom(attributeType, "NMolecules.Bricks.DependencyAttribute"))
            {
                AnalyzeDependencyAttribute(context, attribute);
            }
        }

        private static void AnalyzeRoleAttribute(SyntaxNodeAnalysisContext context, AttributeSyntax attribute)
        {
            var value = GetStringArgument(context, attribute, 0, "name");
            if (value != null && value.Trim().Length == 0)
            {
                var target = FindAnnotatedTypeName(attribute) ?? "type";
                context.ReportDiagnostic(Diagnostic.Create(
                    BrickAnalyzerDiagnostics.EmptyRoleName,
                    attribute.GetLocation(),
                    target));
            }
        }

        private static void AnalyzeRuleAttribute(SyntaxNodeAnalysisContext context, AttributeSyntax attribute)
        {
            ReportIfEmpty(context, attribute, BrickAnalyzerDiagnostics.EmptyRulePart, 0, "id");
            ReportIfEmpty(context, attribute, BrickAnalyzerDiagnostics.EmptyRulePart, 1, "source role");
            ReportIfEmpty(context, attribute, BrickAnalyzerDiagnostics.EmptyRulePart, 2, "target role");
        }

        private static void AnalyzeDependencyAttribute(SyntaxNodeAnalysisContext context, AttributeSyntax attribute)
        {
            ReportIfEmpty(context, attribute, BrickAnalyzerDiagnostics.EmptyDependencyPart, 0, "id");
            ReportIfEmpty(context, attribute, BrickAnalyzerDiagnostics.EmptyDependencyPart, 1, "source");
            ReportIfEmpty(context, attribute, BrickAnalyzerDiagnostics.EmptyDependencyPart, 2, "target");
            ReportIfEmpty(context, attribute, BrickAnalyzerDiagnostics.EmptyDependencyPart, 3, "kind");
        }

        private static void ReportIfEmpty(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            DiagnosticDescriptor descriptor,
            int ordinal,
            string name)
        {
            var value = GetStringArgument(context, attribute, ordinal, name);
            if (value != null && value.Trim().Length == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptor, attribute.GetLocation(), name));
            }
        }

        private static string GetStringArgument(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            int ordinal,
            string name)
        {
            var arguments = attribute.ArgumentList?.Arguments;
            if (arguments == null || arguments.Value.Count == 0)
            {
                return null;
            }

            AttributeArgumentSyntax argument = null;
            for (var i = 0; i < arguments.Value.Count; i++)
            {
                var candidate = arguments.Value[i];
                if (candidate.NameEquals != null &&
                    candidate.NameEquals.Name.Identifier.ValueText == name)
                {
                    argument = candidate;
                    break;
                }
            }

            if (argument == null && ordinal < arguments.Value.Count)
            {
                argument = arguments.Value[ordinal];
            }

            if (argument == null)
            {
                return null;
            }

            var constant = context.SemanticModel.GetConstantValue(argument.Expression, context.CancellationToken);
            return constant.HasValue ? constant.Value as string : null;
        }

        private static bool IsOrDerivesFrom(INamedTypeSymbol type, string metadataName)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                if (current.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty) == metadataName)
                {
                    return true;
                }
            }

            return false;
        }

        private static string FindAnnotatedTypeName(AttributeSyntax attribute)
        {
            var list = attribute.Parent as AttributeListSyntax;
            var declaration = list?.Parent as TypeDeclarationSyntax;
            return declaration?.Identifier.ValueText;
        }
    }
}
