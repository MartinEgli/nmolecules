using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickMemberContractAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(
                BrickAnalyzerDiagnostics.BrickExactlyOneMemberContract,
                BrickAnalyzerDiagnostics.BrickRequireAllMembersContract,
                BrickAnalyzerDiagnostics.BrickMemberCountContract,
                BrickAnalyzerDiagnostics.BrickExclusiveChoiceContract,
                BrickAnalyzerDiagnostics.BrickMemberRangeContract,
                BrickAnalyzerDiagnostics.BrickForbiddenMemberContract,
                BrickAnalyzerDiagnostics.BrickUniqueNamedMemberContract,
                BrickAnalyzerDiagnostics.BrickRequiredNamedMemberContract);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeType,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.RecordDeclaration,
                SyntaxKind.InterfaceDeclaration);
        }

        private static void AnalyzeType(SyntaxNodeAnalysisContext context)
        {
            var declaration = (TypeDeclarationSyntax)context.Node;
            var type = context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) as INamedTypeSymbol;
            if (!ShouldAnalyzeType(type))
            {
                return;
            }

            var memberMarkerCounts = CountMemberMarkers(type);
            foreach (var contractCarrier in type.GetAttributes())
            {
                foreach (var contract in GetMemberContracts(contractCarrier))
                {
                    var contractType = contract.AttributeClass;
                    var contractName = GetContractName(contractCarrier, contract);
                    if (BrickAnalyzerFacts.IsOrDerivesFrom(contractType, BrickAnalyzerFacts.RequireExactlyOneMemberAttribute))
                    {
                        var marker = GetType(contract, 0);
                        if (marker != null)
                        {
                            var count = GetMarkedMemberCount(memberMarkerCounts, marker);
                            ReportWhen(
                                context,
                                contractCarrier,
                                type,
                                count != 1,
                                BrickAnalyzerDiagnostics.BrickExactlyOneMemberContract,
                                $"Brick contract '{contractName}' requires exactly one member marked with '{marker.Name}', but '{type.Name}' declares {count}.");
                        }

                        continue;
                    }

                    if (BrickAnalyzerFacts.IsOrDerivesFrom(contractType, BrickAnalyzerFacts.RequireMemberCountAttribute))
                    {
                        var marker = GetType(contract, 0);
                        var expected = GetInt(contract, 1, 0);
                        if (marker != null && expected >= 0)
                        {
                            var count = GetMarkedMemberCount(memberMarkerCounts, marker);
                            ReportWhen(
                                context,
                                contractCarrier,
                                type,
                                count != expected,
                                BrickAnalyzerDiagnostics.BrickMemberCountContract,
                                $"Brick contract '{contractName}' requires exactly {expected} members marked with '{marker.Name}', but '{type.Name}' declares {count}.");
                        }

                        continue;
                    }

                    if (BrickAnalyzerFacts.IsOrDerivesFrom(contractType, BrickAnalyzerFacts.RequireMemberRangeAttribute))
                    {
                        var marker = GetType(contract, 0);
                        var minimum = GetInt(contract, 1, 0);
                        var maximum = GetInt(contract, 2, 0);
                        if (marker != null && minimum >= 0 && maximum >= 0 && minimum <= maximum)
                        {
                            var count = GetMarkedMemberCount(memberMarkerCounts, marker);
                            ReportWhen(
                                context,
                                contractCarrier,
                                type,
                                count < minimum || count > maximum,
                                BrickAnalyzerDiagnostics.BrickMemberRangeContract,
                                $"Brick contract '{contractName}' requires between {minimum} and {maximum} members marked with '{marker.Name}', but '{type.Name}' declares {count}.");
                        }

                        continue;
                    }

                    if (BrickAnalyzerFacts.IsOrDerivesFrom(contractType, BrickAnalyzerFacts.RequireAllMembersAttribute))
                    {
                        foreach (var marker in GetTypeArray(contract))
                        {
                            ReportWhen(
                                context,
                                contractCarrier,
                                type,
                                GetMarkedMemberCount(memberMarkerCounts, marker) == 0,
                                BrickAnalyzerDiagnostics.BrickRequireAllMembersContract,
                                $"Brick contract '{contractName}' requires a member marked with '{marker.Name}', but '{type.Name}' does not declare one.");
                        }

                        continue;
                    }

                    if (BrickAnalyzerFacts.IsOrDerivesFrom(contractType, BrickAnalyzerFacts.RequireExclusiveChoiceAttribute))
                    {
                        var left = GetType(contract, 0);
                        var right = GetType(contract, 1);
                        if (left == null || right == null)
                        {
                            continue;
                        }

                        if (SymbolEqualityComparer.Default.Equals(left, right))
                        {
                            continue;
                        }

                        var hasLeft = GetMarkedMemberCount(memberMarkerCounts, left) > 0;
                        var hasRight = GetMarkedMemberCount(memberMarkerCounts, right) > 0;
                        ReportWhen(
                            context,
                            contractCarrier,
                            type,
                            hasLeft == hasRight,
                            BrickAnalyzerDiagnostics.BrickExclusiveChoiceContract,
                            $"Brick contract '{contractName}' requires exactly one of '{left.Name}' or '{right.Name}' on '{type.Name}'.");
                        continue;
                    }

                    if (BrickAnalyzerFacts.IsOrDerivesFrom(contractType, BrickAnalyzerFacts.ForbidMemberAttribute))
                    {
                        var marker = GetType(contract, 0);
                        if (marker != null)
                        {
                            var count = GetMarkedMemberCount(memberMarkerCounts, marker);
                            ReportWhen(
                                context,
                                contractCarrier,
                                type,
                                count > 0,
                                BrickAnalyzerDiagnostics.BrickForbiddenMemberContract,
                                $"Brick contract '{contractName}' forbids members marked with '{marker.Name}', but '{type.Name}' declares {count}.");
                        }

                        continue;
                    }

                    if (BrickAnalyzerFacts.IsOrDerivesFrom(contractType, BrickAnalyzerFacts.RequireNamedMembersAttribute))
                    {
                        var marker = GetType(contract, 0);
                        var nameArgument = GetNamedString(contract, "NameArgument", "Name");
                        var requiredNames = GetStringArray(contract, 1).ToArray();
                        if (marker != null &&
                            requiredNames.Length > 0 &&
                            !HasDuplicateValues(requiredNames) &&
                            !HasEmptyNamedString(contract, "NameArgument") &&
                            !string.IsNullOrWhiteSpace(nameArgument))
                        {
                            foreach (var requiredName in requiredNames.Distinct(System.StringComparer.Ordinal))
                            {
                                var count = GetMarkerNameCount(type, marker, nameArgument, requiredName);
                                ReportWhen(
                                    context,
                                    contractCarrier,
                                    type,
                                    count == 0,
                                    BrickAnalyzerDiagnostics.BrickRequiredNamedMemberContract,
                                    $"Brick contract '{contractName}' requires a member marked with '{marker.Name}' name {FormatMarkerName(requiredName)}, but '{type.Name}' declares none.");
                            }
                        }

                        continue;
                    }

                    if (BrickAnalyzerFacts.IsOrDerivesFrom(contractType, BrickAnalyzerFacts.RequireUniqueNamedMemberAttribute))
                    {
                        var marker = GetType(contract, 0);
                        var nameArgument = GetString(contract, 1, "Name");
                        if (marker != null && !string.IsNullOrWhiteSpace(nameArgument))
                        {
                            foreach (var duplicate in GetDuplicateMarkerNames(type, marker, nameArgument))
                            {
                                ReportWhen(
                                    context,
                                    contractCarrier,
                                    type,
                                    duplicate.Value > 1,
                                    BrickAnalyzerDiagnostics.BrickUniqueNamedMemberContract,
                                    $"Brick contract '{contractName}' allows at most one member marked with '{marker.Name}' name {FormatMarkerName(duplicate.Key)}, but '{type.Name}' declares {duplicate.Value}.");
                            }
                        }
                    }
                }
            }
        }

        private static bool ShouldAnalyzeType(INamedTypeSymbol type) =>
            type != null && !IsAttributeType(type);

        private static IEnumerable<AttributeData> GetMemberContracts(AttributeData contractCarrier)
        {
            var carrierType = contractCarrier.AttributeClass;
            if (carrierType == null)
            {
                return Enumerable.Empty<AttributeData>();
            }

            var contracts = new List<AttributeData>();
            if (IsMemberContract(carrierType))
            {
                contracts.Add(contractCarrier);
            }

            for (var current = carrierType; current != null; current = current.BaseType)
            {
                foreach (var attribute in current.GetAttributes())
                {
                    var attributeType = attribute.AttributeClass;
                    if (IsMemberContract(attributeType))
                    {
                        contracts.Add(attribute);
                    }
                }
            }

            return contracts;
        }

        private static bool IsMemberContract(INamedTypeSymbol attributeType) =>
            BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireExactlyOneMemberAttribute) ||
            BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireMemberCountAttribute) ||
            BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireMemberRangeAttribute) ||
            BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireAllMembersAttribute) ||
            BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireExclusiveChoiceAttribute) ||
            BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.ForbidMemberAttribute) ||
            BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireNamedMembersAttribute) ||
            BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireUniqueNamedMemberAttribute);

        private static bool IsAttributeType(INamedTypeSymbol type)
        {
            for (var current = type.BaseType; current != null; current = current.BaseType)
            {
                if (current.ToDisplayString() == "System.Attribute")
                {
                    return true;
                }
            }

            return false;
        }

        private static IReadOnlyDictionary<ITypeSymbol, int> CountMemberMarkers(INamedTypeSymbol type)
        {
            var counts = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);
            foreach (var member in type.GetMembers().Where(IsEligibleContractMember))
            {
                var markersOnMember = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
                foreach (var attribute in member.GetAttributes())
                {
                    for (var current = attribute.AttributeClass; current != null; current = current.BaseType)
                    {
                        markersOnMember.Add(current);
                    }
                }

                foreach (var marker in markersOnMember)
                {
                    int count;
                    counts.TryGetValue(marker, out count);
                    counts[marker] = count + 1;
                }
            }

            return counts;
        }

        private static int GetMarkedMemberCount(
            IReadOnlyDictionary<ITypeSymbol, int> memberMarkerCounts,
            ITypeSymbol marker)
        {
            int count;
            return memberMarkerCounts.TryGetValue(marker, out count) ? count : 0;
        }

        private static bool IsEligibleContractMember(ISymbol member) =>
            member.IsImplicitlyDeclared == false &&
             (member is IFieldSymbol ||
             member is IPropertySymbol ||
             member is IEventSymbol ||
             (member is IMethodSymbol method &&
              method.MethodKind != MethodKind.Constructor &&
              method.MethodKind != MethodKind.StaticConstructor &&
              method.MethodKind != MethodKind.PropertyGet &&
              method.MethodKind != MethodKind.PropertySet &&
              method.MethodKind != MethodKind.EventAdd &&
              method.MethodKind != MethodKind.EventRemove));

        private static ITypeSymbol GetType(AttributeData attribute, int ordinal)
        {
            if (ordinal >= attribute.ConstructorArguments.Length)
            {
                return null;
            }

            return attribute.ConstructorArguments[ordinal].Value as ITypeSymbol;
        }

        private static int GetInt(AttributeData attribute, int ordinal, int defaultValue)
        {
            if (ordinal >= attribute.ConstructorArguments.Length || !(attribute.ConstructorArguments[ordinal].Value is int))
            {
                return defaultValue;
            }

            return (int)attribute.ConstructorArguments[ordinal].Value;
        }

        private static string GetString(AttributeData attribute, int ordinal, string defaultValue)
        {
            if (ordinal < attribute.ConstructorArguments.Length)
            {
                var value = attribute.ConstructorArguments[ordinal].Value as string;
                return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
            }

            return defaultValue;
        }

        private static string GetNamedString(AttributeData attribute, string propertyName, string defaultValue)
        {
            foreach (var argument in attribute.NamedArguments)
            {
                if (argument.Key == propertyName)
                {
                    var value = argument.Value.Value as string;
                    return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
                }
            }

            return defaultValue;
        }

        private static IEnumerable<string> GetStringArray(AttributeData attribute, int ordinal)
        {
            if (ordinal >= attribute.ConstructorArguments.Length)
            {
                return Enumerable.Empty<string>();
            }

            var argument = attribute.ConstructorArguments[ordinal];
            if (argument.IsNull)
            {
                return Enumerable.Empty<string>();
            }

            if (argument.Kind == TypedConstantKind.Array)
            {
                return argument.Values.Select(value => NormalizeMarkerName(value.Value as string));
            }

            return Enumerable.Empty<string>();
        }

        private static IEnumerable<ITypeSymbol> GetTypeArray(AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length == 0)
            {
                return Enumerable.Empty<ITypeSymbol>();
            }

            var argument = attribute.ConstructorArguments[0];
            if (argument.IsNull || argument.Kind != TypedConstantKind.Array)
            {
                return Enumerable.Empty<ITypeSymbol>();
            }

            return argument
                .Values
                .Select(value => value.Value as ITypeSymbol)
                .Where(value => value != null);
        }

        private static IEnumerable<KeyValuePair<string, int>> GetDuplicateMarkerNames(
            INamedTypeSymbol type,
            ITypeSymbol marker,
            string nameArgument)
        {
            var counts = new Dictionary<string, int>(System.StringComparer.Ordinal);
            foreach (var member in type.GetMembers().Where(IsEligibleContractMember))
            {
                foreach (var attribute in member.GetAttributes())
                {
                    if (!AttributeMatchesMarker(attribute, marker))
                    {
                        continue;
                    }

                    var name = GetMarkerName(attribute, nameArgument);
                    int count;
                    counts.TryGetValue(name, out count);
                    counts[name] = count + 1;
                }
            }

            return counts.Where(count => count.Value > 1);
        }

        private static int GetMarkerNameCount(
            INamedTypeSymbol type,
            ITypeSymbol marker,
            string nameArgument,
            string requiredName)
        {
            var count = 0;
            foreach (var member in type.GetMembers().Where(IsEligibleContractMember))
            {
                foreach (var attribute in member.GetAttributes())
                {
                    if (AttributeMatchesMarker(attribute, marker) &&
                        string.Equals(GetMarkerName(attribute, nameArgument), requiredName, System.StringComparison.Ordinal))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static bool AttributeMatchesMarker(AttributeData attribute, ITypeSymbol marker)
        {
            for (var current = attribute.AttributeClass; current != null; current = current.BaseType)
            {
                if (SymbolEqualityComparer.Default.Equals(current, marker))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetMarkerName(AttributeData attribute, string nameArgument)
        {
            var constructor = attribute.AttributeConstructor;
            if (constructor != null)
            {
                var limit = System.Math.Min(constructor.Parameters.Length, attribute.ConstructorArguments.Length);
                for (var index = 0; index < limit; index++)
                {
                    if (string.Equals(constructor.Parameters[index].Name, nameArgument, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return NormalizeMarkerName(attribute.ConstructorArguments[index].Value as string);
                    }
                }
            }

            foreach (var argument in attribute.NamedArguments)
            {
                if (string.Equals(argument.Key, nameArgument, System.StringComparison.OrdinalIgnoreCase))
                {
                    return NormalizeMarkerName(argument.Value.Value as string);
                }
            }

            foreach (var argument in attribute.ConstructorArguments)
            {
                var value = argument.Value as string;
                if (value != null)
                {
                    return NormalizeMarkerName(value);
                }
            }

            return string.Empty;
        }

        private static string NormalizeMarkerName(string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value;

        private static bool HasDuplicateValues(IEnumerable<string> values)
        {
            var seen = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (var value in values)
            {
                if (!seen.Add(value))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasEmptyNamedString(AttributeData attribute, string propertyName)
        {
            foreach (var argument in attribute.NamedArguments)
            {
                if (argument.Key != propertyName)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(argument.Value.Value as string))
                {
                    return true;
                }
            }

            return false;
        }

        private static string FormatMarkerName(string name) =>
            string.IsNullOrWhiteSpace(name) ? "<unnamed>" : $"'{name}'";

        private static void ReportWhen(
            SyntaxNodeAnalysisContext context,
            AttributeData contractCarrier,
            INamedTypeSymbol type,
            bool condition,
            DiagnosticDescriptor descriptor,
            string message)
        {
            if (!condition)
            {
                return;
            }

            var location = contractCarrier.ApplicationSyntaxReference.GetSyntax(context.CancellationToken).GetLocation();
            context.ReportDiagnostic(Diagnostic.Create(
                descriptor,
                location,
                message));
        }

        private static string GetContractName(AttributeData contractCarrier, AttributeData contract)
        {
            var carrierType = contractCarrier.AttributeClass;
            var contractType = contract.AttributeClass;
            if (carrierType != null &&
                contractType != null &&
                !SymbolEqualityComparer.Default.Equals(carrierType, contractType))
            {
                return TrimAttributeSuffix(carrierType.Name);
            }

            return TrimAttributeSuffix(contractType?.Name ?? "MemberContract");
        }

        private static string TrimAttributeSuffix(string name) =>
            name.EndsWith("Attribute", System.StringComparison.Ordinal)
                ? name.Substring(0, name.Length - "Attribute".Length)
                : name;
    }
}
