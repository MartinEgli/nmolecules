using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Validates namespace-to-role mappings declared with <c>NamespaceRoleAttribute</c>.
    /// </summary>
    /// <remarks>
    /// Namespace role mappings are useful when a project wants to assign Bricks roles by folder or
    /// namespace convention instead of annotating every type. The analyzer keeps those mappings
    /// usable by reporting empty namespace patterns or role names before the dependency analyzer
    /// consumes them. See <c>tests/nMolecules.Bricks.Analyzers.Test/BrickAnalyzerCoverageTest.cs</c>
    /// for pass and violation samples.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickNamespaceRoleMetadataAnalyzer : DiagnosticAnalyzer
    {
        private const string NamespaceRoleAttributeName = "NMolecules.Bricks.NamespaceRoleAttribute";

        /// <summary>
        /// Gets the metadata diagnostics produced by this analyzer.
        /// </summary>
        /// <value>
        /// Contains <see cref="BrickAnalyzerDiagnostics.BrickConfiguration"/> for invalid
        /// namespace role declarations.
        /// </value>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(
                BrickAnalyzerDiagnostics.BrickConfiguration,
                BrickAnalyzerDiagnostics.BrickNamespaceRoleConfiguration);

        /// <summary>
        /// Registers attribute syntax analysis for namespace role declarations.
        /// </summary>
        /// <param name="context">
        /// The Roslyn analysis context supplied by Visual Studio, MSBuild, or test hosts.
        /// </param>
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
                context.ReportDiagnostic(BrickDiagnosticProperties.Create(
                    BrickAnalyzerDiagnostics.BrickNamespaceRoleConfiguration,
                    attribute.GetLocation(),
                    message,
                    configurationKind: "NamespaceRole"));
            }
        }
    }
}
