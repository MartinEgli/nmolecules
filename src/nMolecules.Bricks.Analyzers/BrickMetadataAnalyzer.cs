using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickMetadataAnalyzer : DiagnosticAnalyzer
    {
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
            if (attributeType == null)
            {
                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.PolicyAttribute))
            {
                ReportIfEmpty(context, attribute, "PolicyAttribute must declare a non-empty policy id", 0, "id");
                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RoleAttribute))
            {
                var value = BrickAnalyzerFacts.GetStringArgument(context, attribute, 0, "name");
                if (string.IsNullOrWhiteSpace(value) && !HasNonEmptyRoleAlias(attributeType))
                {
                    var target = BrickAnalyzerFacts.FindAnnotatedTypeName(attribute) ?? "type";
                    ReportConfiguration(context, attribute, $"RoleAttribute on '{target}' must declare a non-empty role name");
                }

                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RuleAttribute))
            {
                ReportIfEmpty(context, attribute, "RuleAttribute must declare a non-empty id", 0, "id");
                ReportIfEmpty(context, attribute, "RuleAttribute must declare a non-empty source role", 1, "sourceRole");
                ReportIfEmpty(context, attribute, "RuleAttribute must declare a non-empty target role", 2, "targetRole");
                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.DependencyAttribute))
            {
                ReportIfEmpty(context, attribute, "DependencyAttribute must declare a non-empty id", 0, "id");
                ReportIfEmpty(context, attribute, "DependencyAttribute must declare a non-empty source", 1, "source");
                ReportIfEmpty(context, attribute, "DependencyAttribute must declare a non-empty target", 2, "target");
                ReportIfEmpty(context, attribute, "DependencyAttribute must declare a non-empty kind", 3, "kind");
                return;
            }

            AnalyzeMemberContractConfiguration(context, attribute, attributeType);
        }

        private static bool HasNonEmptyRoleAlias(INamedTypeSymbol attributeType)
        {
            foreach (var marker in attributeType.GetAttributes())
            {
                var markerType = marker.AttributeClass;
                if (BrickAnalyzerFacts.IsOrDerivesFrom(markerType, BrickAnalyzerFacts.RoleAliasAttribute) &&
                    !string.IsNullOrWhiteSpace(BrickAnalyzerFacts.GetAttributeString(marker, 0, "Role")))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AnalyzeMemberContractConfiguration(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            INamedTypeSymbol attributeType)
        {
            var target = BrickAnalyzerFacts.FindAnnotatedTypeName(attribute) ?? "type";

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireExactlyOneMemberAttribute))
            {
                if (BrickAnalyzerFacts.GetTypeArgument(context, attribute, 0, "memberAttributeType") == null)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireExactlyOneMember", target, "missing marker attribute type");
                }

                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireMemberCountAttribute))
            {
                if (BrickAnalyzerFacts.GetTypeArgument(context, attribute, 0, "memberAttributeType") == null)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireMemberCount", target, "missing marker attribute type");
                }

                var count = BrickAnalyzerFacts.GetIntArgument(context, attribute, 1, "count");
                if (count.HasValue && count.Value < 0)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireMemberCount", target, "count must not be negative");
                }

                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireMemberRangeAttribute))
            {
                if (BrickAnalyzerFacts.GetTypeArgument(context, attribute, 0, "memberAttributeType") == null)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireMemberRange", target, "missing marker attribute type");
                }

                var minimumCount = BrickAnalyzerFacts.GetIntArgument(context, attribute, 1, "minimumCount");
                var maximumCount = BrickAnalyzerFacts.GetIntArgument(context, attribute, 2, "maximumCount");
                if (minimumCount.HasValue && minimumCount.Value < 0)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireMemberRange", target, "minimum count must not be negative");
                }

                if (maximumCount.HasValue && maximumCount.Value < 0)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireMemberRange", target, "maximum count must not be negative");
                }

                if (minimumCount.HasValue && maximumCount.HasValue && minimumCount.Value > maximumCount.Value)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireMemberRange", target, "minimum count must not exceed maximum count");
                }

                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireAllMembersAttribute))
            {
                if (BrickAnalyzerFacts.GetTypeArrayArgument(context, attribute).Count == 0)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireAllMembers", target, "at least one marker attribute type is required");
                }

                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireExclusiveChoiceAttribute))
            {
                var left = BrickAnalyzerFacts.GetTypeArgument(context, attribute, 0, "leftMemberAttributeType");
                var right = BrickAnalyzerFacts.GetTypeArgument(context, attribute, 1, "rightMemberAttributeType");
                if (left == null || right == null)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireExclusiveChoice", target, "both marker attribute types are required");
                    return;
                }

                if (SymbolEqualityComparer.Default.Equals(left, right))
                {
                    ReportInvalidMemberContract(context, attribute, "RequireExclusiveChoice", target, "left and right marker types must be different");
                }

                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.ForbidMemberAttribute))
            {
                if (BrickAnalyzerFacts.GetTypeArgument(context, attribute, 0, "memberAttributeType") == null)
                {
                    ReportInvalidMemberContract(context, attribute, "ForbidMember", target, "missing marker attribute type");
                }

                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireNamedMembersAttribute))
            {
                if (BrickAnalyzerFacts.GetTypeArgument(context, attribute, 0, "memberAttributeType") == null)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireNamedMembers", target, "missing marker attribute type");
                }

                var requiredNames = GetStringArgumentsFromOrdinal(context, attribute, 1);
                if (requiredNames.Count == 0)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireNamedMembers", target, "at least one required marker name is required");
                }

                if (requiredNames.GroupBy(name => name, System.StringComparer.Ordinal).Any(group => group.Count() > 1))
                {
                    ReportInvalidMemberContract(context, attribute, "RequireNamedMembers", target, "required marker names must be unique");
                }

                var nameArgument = BrickAnalyzerFacts.GetStringArgument(context, attribute, int.MaxValue, "NameArgument", "nameArgument");
                if (nameArgument != null && string.IsNullOrWhiteSpace(nameArgument))
                {
                    ReportInvalidMemberContract(context, attribute, "RequireNamedMembers", target, "name argument must not be empty");
                }

                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireUniqueNamedMemberAttribute))
            {
                if (BrickAnalyzerFacts.GetTypeArgument(context, attribute, 0, "memberAttributeType") == null)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireUniqueNamedMember", target, "missing marker attribute type");
                }

                var nameArgument = BrickAnalyzerFacts.GetStringArgument(context, attribute, 1, "nameArgument");
                if (nameArgument != null && string.IsNullOrWhiteSpace(nameArgument))
                {
                    ReportInvalidMemberContract(context, attribute, "RequireUniqueNamedMember", target, "name argument must not be empty");
                }
            }
        }

        private static IReadOnlyList<string> GetStringArgumentsFromOrdinal(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            int ordinal)
        {
            var result = new List<string>();
            var arguments = attribute.ArgumentList?.Arguments;
            if (arguments == null || arguments.Value.Count == 0)
            {
                return result;
            }

            var positionalIndex = 0;
            foreach (var argument in arguments.Value)
            {
                if (argument.NameEquals != null || argument.NameColon != null)
                {
                    continue;
                }

                if (positionalIndex++ < ordinal)
                {
                    continue;
                }

                if (TryAddStringArrayConstants(context, argument.Expression, result))
                {
                    continue;
                }

                var constant = context.SemanticModel.GetConstantValue(argument.Expression, context.CancellationToken);
                if (constant.HasValue)
                {
                    result.Add(NormalizeMarkerName(constant.Value as string));
                }
            }

            return result;
        }

        private static bool TryAddStringArrayConstants(
            SyntaxNodeAnalysisContext context,
            ExpressionSyntax expression,
            ICollection<string> result)
        {
            var initializer = (expression as ArrayCreationExpressionSyntax)?.Initializer ??
                (expression as ImplicitArrayCreationExpressionSyntax)?.Initializer;
            if (initializer == null)
            {
                return false;
            }

            foreach (var item in initializer.Expressions)
            {
                var constant = context.SemanticModel.GetConstantValue(item, context.CancellationToken);
                if (constant.HasValue)
                {
                    result.Add(NormalizeMarkerName(constant.Value as string));
                }
            }

            return true;
        }

        private static string NormalizeMarkerName(string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value;

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
                ReportConfiguration(context, attribute, message);
            }
        }

        private static void ReportInvalidMemberContract(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            string contractName,
            string target,
            string reason)
        {
            ReportConfiguration(context, attribute, $"Brick member contract '{contractName}' on '{target}' is invalid: {reason}");
        }

        private static void ReportConfiguration(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            string message)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                BrickAnalyzerDiagnostics.BrickConfiguration,
                attribute.GetLocation(),
                message));
        }
    }
}
