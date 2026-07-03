using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    internal static class BrickAnalyzerFacts
    {
        public const string PolicyAttribute = "NMolecules.Bricks.PolicyAttribute";
        public const string DefaultPolicyAttribute = "NMolecules.Bricks.DefaultPolicyAttribute";
        public const string PolicyImportAttribute = "NMolecules.Bricks.PolicyImportAttribute";
        public const string RoleAttribute = "NMolecules.Bricks.RoleAttribute";
        public const string RoleAliasAttribute = "NMolecules.Bricks.RoleAliasAttribute";
        public const string TypeRoleAttribute = "NMolecules.Bricks.TypeRoleAttribute";
        public const string RoleCombinationAttribute = "NMolecules.Bricks.RoleCombinationAttribute";
        public const string RuleAttribute = "NMolecules.Bricks.RuleAttribute";
        public const string DependencyAttribute = "NMolecules.Bricks.DependencyAttribute";
        public const string RuleFilterAttribute = "NMolecules.Bricks.RuleFilterAttribute";
        public const string ExcludedSourceNameContainsAttribute = "NMolecules.Bricks.ExcludedSourceNameContainsAttribute";
        public const string ExcludedTargetNameContainsAttribute = "NMolecules.Bricks.ExcludedTargetNameContainsAttribute";
        public const string RequiredSourceNameContainsAttribute = "NMolecules.Bricks.RequiredSourceNameContainsAttribute";
        public const string RequiredTargetNameContainsAttribute = "NMolecules.Bricks.RequiredTargetNameContainsAttribute";
        public const string RequireExactlyOneMemberAttribute = "NMolecules.Bricks.RequireExactlyOneMemberAttribute";
        public const string RequireMemberCountAttribute = "NMolecules.Bricks.RequireMemberCountAttribute";
        public const string RequireMemberRangeAttribute = "NMolecules.Bricks.RequireMemberRangeAttribute";
        public const string RequireAllMembersAttribute = "NMolecules.Bricks.RequireAllMembersAttribute";
        public const string RequireExclusiveChoiceAttribute = "NMolecules.Bricks.RequireExclusiveChoiceAttribute";
        public const string ForbidMemberAttribute = "NMolecules.Bricks.ForbidMemberAttribute";
        public const string RequireNamedMembersAttribute = "NMolecules.Bricks.RequireNamedMembersAttribute";
        public const string RequireUniqueNamedMemberAttribute = "NMolecules.Bricks.RequireUniqueNamedMemberAttribute";

        public static bool IsOrDerivesFrom(INamedTypeSymbol type, string metadataName)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                if (ToMetadataName(current) == metadataName)
                {
                    return true;
                }
            }

            return false;
        }

        public static string ToMetadataName(INamedTypeSymbol type) =>
            type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty);

        public static string GetStringArgument(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            int ordinal,
            params string[] names)
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
                var candidateName = GetArgumentName(candidate);
                if (candidateName != null && names.Contains(candidateName, StringComparer.Ordinal))
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
            if (!constant.HasValue)
            {
                return null;
            }

            return constant.Value == null
                ? string.Empty
                : constant.Value as string;
        }

        public static int? GetIntArgument(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            int ordinal,
            params string[] names)
        {
            var argument = FindArgument(attribute, ordinal, names);
            if (argument == null)
            {
                return null;
            }

            var constant = context.SemanticModel.GetConstantValue(argument.Expression, context.CancellationToken);
            return constant.HasValue && constant.Value is int ? (int?)constant.Value : null;
        }

        public static ITypeSymbol GetTypeArgument(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            int ordinal,
            params string[] names)
        {
            var argument = FindArgument(attribute, ordinal, names);
            var typeOf = argument?.Expression as TypeOfExpressionSyntax;
            return typeOf == null
                ? null
                : context.SemanticModel.GetTypeInfo(typeOf.Type, context.CancellationToken).Type;
        }

        public static IReadOnlyList<ITypeSymbol> GetTypeArrayArgument(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute)
        {
            var result = new List<ITypeSymbol>();
            var arguments = attribute.ArgumentList?.Arguments;
            if (arguments == null)
            {
                return result;
            }

            for (var i = 0; i < arguments.Value.Count; i++)
            {
                var typeOf = arguments.Value[i].Expression as TypeOfExpressionSyntax;
                if (typeOf == null)
                {
                    continue;
                }

                var type = context.SemanticModel.GetTypeInfo(typeOf.Type, context.CancellationToken).Type;
                if (type != null)
                {
                    result.Add(type);
                }
            }

            return result;
        }

        public static string FindAnnotatedTypeName(AttributeSyntax attribute)
        {
            var list = attribute.Parent as AttributeListSyntax;
            var declaration = list?.Parent as TypeDeclarationSyntax;
            return declaration?.Identifier.ValueText;
        }

        public static string GetAttributeString(AttributeData attribute, int ordinal, string propertyName)
        {
            if (ordinal < attribute.ConstructorArguments.Length)
            {
                var value = attribute.ConstructorArguments[ordinal].Value as string;
                if (value != null)
                {
                    return value;
                }
            }

            foreach (var argument in attribute.NamedArguments)
            {
                if (argument.Key == propertyName)
                {
                    return argument.Value.Value as string;
                }
            }

            return string.Empty;
        }

        public static int GetAttributeEnum(AttributeData attribute, int ordinal, string propertyName, int defaultValue)
        {
            if (ordinal < attribute.ConstructorArguments.Length && attribute.ConstructorArguments[ordinal].Value is int)
            {
                return (int)attribute.ConstructorArguments[ordinal].Value;
            }

            foreach (var argument in attribute.NamedArguments)
            {
                if (argument.Key == propertyName && argument.Value.Value is int)
                {
                    return (int)argument.Value.Value;
                }
            }

            return defaultValue;
        }

        private static AttributeArgumentSyntax FindArgument(
            AttributeSyntax attribute,
            int ordinal,
            IEnumerable<string> names)
        {
            var arguments = attribute.ArgumentList?.Arguments;
            if (arguments == null || arguments.Value.Count == 0)
            {
                return null;
            }

            foreach (var candidate in arguments.Value)
            {
                var candidateName = GetArgumentName(candidate);
                if (candidateName != null && names.Contains(candidateName, StringComparer.Ordinal))
                {
                    return candidate;
                }
            }

            return ordinal < arguments.Value.Count ? arguments.Value[ordinal] : null;
        }

        private static string GetArgumentName(AttributeArgumentSyntax argument) =>
            argument.NameEquals?.Name.Identifier.ValueText ??
            argument.NameColon?.Name.Identifier.ValueText;
    }
}
