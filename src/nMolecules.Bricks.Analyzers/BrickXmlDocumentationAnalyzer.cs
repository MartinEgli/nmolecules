using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Reports public API declarations that do not provide XML documentation comments.
    /// </summary>
    /// <remarks>
    /// Bricks is intended to be used as a framework by other developers. This analyzer helps keep
    /// public types, constructors, methods, properties, fields, events, delegates and enum members
    /// documented so generated documentation can explain usage and link to samples or tests.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickXmlDocumentationAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Gets the XML documentation diagnostics produced by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(BrickXmlDocumentationAnalyzerDiagnostics.MissingXmlDocumentation);

        /// <summary>
        /// Registers syntax analysis for public API declarations.
        /// </summary>
        /// <param name="context">
        /// The Roslyn analysis context supplied by Visual Studio, MSBuild, or test hosts.
        /// </param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeDeclaration,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.RecordDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.EnumDeclaration,
                SyntaxKind.DelegateDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.PropertyDeclaration,
                SyntaxKind.IndexerDeclaration,
                SyntaxKind.EventDeclaration,
                SyntaxKind.EventFieldDeclaration,
                SyntaxKind.FieldDeclaration,
                SyntaxKind.EnumMemberDeclaration);
        }

        private static void AnalyzeDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (!IsPublicApiDeclaration(context))
            {
                return;
            }

            if (HasXmlDocumentation(context.Node))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                BrickXmlDocumentationAnalyzerDiagnostics.MissingXmlDocumentation,
                GetDiagnosticLocation(context.Node),
                $"Public API member '{GetDeclarationName(context.Node)}' should provide XML documentation"));
        }

        private static bool IsPublicApiDeclaration(SyntaxNodeAnalysisContext context)
        {
            var symbol = GetDeclaredSymbol(context);
            return symbol != null &&
                symbol.DeclaredAccessibility == Accessibility.Public &&
                HasPublicContainingTypes(symbol);
        }

        private static ISymbol GetDeclaredSymbol(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is BaseFieldDeclarationSyntax fieldDeclaration)
            {
                var variable = fieldDeclaration.Declaration.Variables.FirstOrDefault();
                return variable == null
                    ? null
                    : context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken);
            }

            return context.SemanticModel.GetDeclaredSymbol(context.Node, context.CancellationToken);
        }

        private static bool HasPublicContainingTypes(ISymbol symbol)
        {
            for (var containingType = symbol.ContainingType; containingType != null; containingType = containingType.ContainingType)
            {
                if (containingType.DeclaredAccessibility != Accessibility.Public)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasXmlDocumentation(SyntaxNode node)
        {
            if (HasXmlDocumentationTrivia(node.GetLeadingTrivia()))
            {
                return true;
            }

            if (node is MemberDeclarationSyntax memberDeclaration &&
                memberDeclaration.AttributeLists.Count > 0 &&
                HasXmlDocumentationTrivia(memberDeclaration.AttributeLists[0].GetLeadingTrivia()))
            {
                return true;
            }

            return false;
        }

        private static bool HasXmlDocumentationTrivia(SyntaxTriviaList triviaList) =>
            triviaList.Any(IsXmlDocumentationTrivia);

        private static bool IsXmlDocumentationTrivia(SyntaxTrivia trivia)
        {
            if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) ||
                trivia.GetStructure() is DocumentationCommentTriviaSyntax)
            {
                return true;
            }

            var text = trivia.ToFullString().TrimStart();
            return text.StartsWith("///", System.StringComparison.Ordinal) ||
                text.StartsWith("/**", System.StringComparison.Ordinal);
        }

        private static Location GetDiagnosticLocation(SyntaxNode node)
        {
            if (node is BaseFieldDeclarationSyntax fieldDeclaration)
            {
                var variable = fieldDeclaration.Declaration.Variables.FirstOrDefault();
                return variable?.Identifier.GetLocation() ?? fieldDeclaration.GetLocation();
            }

            if (node is BaseTypeDeclarationSyntax typeDeclaration)
            {
                return typeDeclaration.Identifier.GetLocation();
            }

            if (node is DelegateDeclarationSyntax delegateDeclaration)
            {
                return delegateDeclaration.Identifier.GetLocation();
            }

            if (node is BaseMethodDeclarationSyntax methodDeclaration)
            {
                return methodDeclaration.GetLocation();
            }

            if (node is BasePropertyDeclarationSyntax propertyDeclaration)
            {
                return propertyDeclaration.GetLocation();
            }

            if (node is EnumMemberDeclarationSyntax enumMember)
            {
                return enumMember.Identifier.GetLocation();
            }

            return node.GetLocation();
        }

        private static string GetDeclarationName(SyntaxNode node)
        {
            if (node is BaseTypeDeclarationSyntax typeDeclaration)
            {
                return typeDeclaration.Identifier.ValueText;
            }

            if (node is DelegateDeclarationSyntax delegateDeclaration)
            {
                return delegateDeclaration.Identifier.ValueText;
            }

            if (node is ConstructorDeclarationSyntax constructorDeclaration)
            {
                return constructorDeclaration.Identifier.ValueText;
            }

            if (node is MethodDeclarationSyntax methodDeclaration)
            {
                return methodDeclaration.Identifier.ValueText;
            }

            if (node is BasePropertyDeclarationSyntax propertyDeclaration)
            {
                var property = propertyDeclaration as PropertyDeclarationSyntax;
                if (property != null)
                {
                    return property.Identifier.ValueText;
                }

                if (propertyDeclaration is IndexerDeclarationSyntax)
                {
                    return "this[]";
                }

                return propertyDeclaration.Kind().ToString();
            }

            if (node is BaseFieldDeclarationSyntax fieldDeclaration)
            {
                return fieldDeclaration.Declaration.Variables.FirstOrDefault()?.Identifier.ValueText ?? "field";
            }

            if (node is EnumMemberDeclarationSyntax enumMember)
            {
                return enumMember.Identifier.ValueText;
            }

            return node.Kind().ToString();
        }
    }
}
