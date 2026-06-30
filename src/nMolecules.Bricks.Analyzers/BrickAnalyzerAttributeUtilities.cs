using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NMolecules.Bricks.Analyzers
{
    internal static class BrickAnalyzerAttributeUtilities
    {
        public static IEnumerable<AttributeData> GetAttributes(
            Compilation compilation,
            string metadataName)
        {
            foreach (var attribute in compilation.Assembly.GetAttributes().Where(attribute => IsAttribute(attribute, metadataName)))
            {
                yield return attribute;
            }

            foreach (var attribute in compilation.SourceModule.GetAttributes().Where(attribute => IsAttribute(attribute, metadataName)))
            {
                yield return attribute;
            }

            foreach (var type in GetDeclaredTypes(compilation))
            {
                foreach (var attribute in type.GetAttributes().Where(attribute => IsAttribute(attribute, metadataName)))
                {
                    yield return attribute;
                }
            }
        }

        public static IEnumerable<INamedTypeSymbol> GetDeclaredTypes(Compilation compilation)
        {
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var root = syntaxTree.GetRoot();
                foreach (var declaration in root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax>())
                {
                    var symbol = semanticModel.GetDeclaredSymbol(declaration) as INamedTypeSymbol;
                    if (symbol != null)
                    {
                        yield return symbol;
                    }
                }
            }
        }

        public static bool IsAttribute(AttributeData attribute, string metadataName)
        {
            var attributeType = attribute.AttributeClass;
            return attributeType != null && BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, metadataName);
        }

        public static Location GetLocation(AttributeData attribute) =>
            attribute.ApplicationSyntaxReference?.GetSyntax()?.GetLocation() ?? Location.None;
    }
}
