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
    public sealed class BrickDependencyRuleAnalyzer : DiagnosticAnalyzer
    {
        private const int ForbidDependencyMode = 0;
        private const int RequireDependencyMode = 1;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(BrickAnalyzerDiagnostics.BrickRuleViolation);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationAction(AnalyzeCompilation);
        }

        private static void AnalyzeCompilation(CompilationAnalysisContext context)
        {
            var rules = context.Compilation.Assembly
                .GetAttributes()
                .Where(IsRuleAttribute)
                .Select(ReadRule)
                .Where(rule => rule.IsUsable)
                .ToArray();

            var typeDeclarations = GetTypeDeclarations(context.Compilation).ToArray();
            var roleMap = BuildRoleMap(typeDeclarations);
            if (roleMap.Count == 0)
            {
                return;
            }

            var dependencies = CollectObservedDependencies(typeDeclarations, roleMap).ToList();
            dependencies.AddRange(CollectDeclaredDependencies(context.Compilation, roleMap));

            ReportSelfDependencies(context, dependencies);

            if (rules.Length == 0)
            {
                return;
            }

            ReportForbiddenDependencies(context, rules, roleMap, dependencies);
            ReportMissingRequiredDependencies(context, rules, roleMap, dependencies);
        }

        private static IEnumerable<TypeDeclarationInfo> GetTypeDeclarations(Compilation compilation)
        {
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var root = syntaxTree.GetRoot();
                foreach (var declaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    var symbol = semanticModel.GetDeclaredSymbol(declaration) as INamedTypeSymbol;
                    if (symbol != null)
                    {
                        yield return new TypeDeclarationInfo(symbol, declaration, semanticModel);
                    }
                }
            }
        }

        private static Dictionary<INamedTypeSymbol, IReadOnlyList<string>> BuildRoleMap(IEnumerable<TypeDeclarationInfo> declarations)
        {
            var roleMap = new Dictionary<INamedTypeSymbol, IReadOnlyList<string>>(SymbolEqualityComparer.Default);
            foreach (var declaration in declarations)
            {
                var roles = new List<string>();
                foreach (var attribute in declaration.Symbol.GetAttributes())
                {
                    var role = TryGetRoleName(attribute);
                    if (!string.IsNullOrWhiteSpace(role) && !roles.Contains(role))
                    {
                        roles.Add(role);
                    }
                }

                if (roles.Count > 0)
                {
                    roleMap[declaration.Symbol] = roles;
                }
            }

            return roleMap;
        }

        private static IEnumerable<ObservedDependency> CollectObservedDependencies(
            IEnumerable<TypeDeclarationInfo> declarations,
            IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<string>> roleMap)
        {
            var dependencies = new List<ObservedDependency>();
            foreach (var declaration in declarations)
            {
                if (!roleMap.ContainsKey(declaration.Symbol))
                {
                    continue;
                }

                foreach (var member in declaration.Symbol.GetMembers())
                {
                    if (member.IsImplicitlyDeclared)
                    {
                        continue;
                    }

                    foreach (var target in GetMemberTargetTypes(member).SelectMany(ExpandTargetTypes))
                    {
                        var normalizedTarget = NormalizeType(target);
                        if (normalizedTarget == null || !roleMap.ContainsKey(normalizedTarget))
                        {
                            continue;
                        }

                        dependencies.Add(new ObservedDependency(
                            declaration.Symbol,
                            normalizedTarget,
                            member.Name,
                            member.Locations.FirstOrDefault()));
                    }
                }

                foreach (var syntaxTarget in CollectSyntaxTargetTypes(declaration))
                {
                    foreach (var target in ExpandTargetTypes(syntaxTarget.Type))
                    {
                        var normalizedTarget = NormalizeType(target);
                        if (normalizedTarget == null || !roleMap.ContainsKey(normalizedTarget))
                        {
                            continue;
                        }

                        dependencies.Add(new ObservedDependency(
                            declaration.Symbol,
                            normalizedTarget,
                            syntaxTarget.MemberName,
                            syntaxTarget.Location));
                    }
                }
            }

            return dependencies;
        }

        private static IEnumerable<ObservedDependency> CollectDeclaredDependencies(
            Compilation compilation,
            IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<string>> roleMap)
        {
            var dependencies = new List<ObservedDependency>();
            foreach (var attribute in compilation.Assembly.GetAttributes().Where(IsDependencyAttribute))
            {
                var sourceName = BrickAnalyzerFacts.GetAttributeString(attribute, 1, "Source");
                var targetName = BrickAnalyzerFacts.GetAttributeString(attribute, 2, "Target");
                var source = FindType(roleMap.Keys, sourceName);
                var target = FindType(roleMap.Keys, targetName);
                if (source == null || target == null)
                {
                    continue;
                }

                var location = attribute.ApplicationSyntaxReference.GetSyntax().GetLocation();
                dependencies.Add(new ObservedDependency(
                    source,
                    target,
                    attribute.AttributeClass.Name,
                    location));
            }

            return dependencies;
        }

        private static void ReportForbiddenDependencies(
            CompilationAnalysisContext context,
            IEnumerable<BrickRuleInfo> rules,
            IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<string>> roleMap,
            IEnumerable<ObservedDependency> dependencies)
        {
            var forbidRules = rules.Where(rule => rule.Mode == ForbidDependencyMode).ToArray();
            var reported = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (var dependency in dependencies)
            {
                var sourceRoles = roleMap[dependency.Source];
                var targetRoles = roleMap[dependency.Target];
                foreach (var rule in forbidRules)
                {
                    if (!sourceRoles.Contains(rule.SourceRole) || !targetRoles.Contains(rule.TargetRole))
                    {
                        continue;
                    }

                    var reportKey = $"{rule.Id}|{dependency.Source.Name}|{dependency.Target.Name}|{dependency.MemberName}";
                    if (!reported.Add(reportKey))
                    {
                        continue;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(
                        BrickAnalyzerDiagnostics.BrickRuleViolation,
                        dependency.Location,
                        $"Brick rule '{rule.Id}' forbids dependency from '{dependency.Source.Name}' to '{dependency.Target.Name}' through '{dependency.MemberName}'"));
                }
            }
        }

        private static void ReportSelfDependencies(
            CompilationAnalysisContext context,
            IEnumerable<ObservedDependency> dependencies)
        {
            var reported = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (var dependency in dependencies)
            {
                if (!SymbolEqualityComparer.Default.Equals(dependency.Source, dependency.Target))
                {
                    continue;
                }

                var reportKey = $"{dependency.Source.ToDisplayString()}|{dependency.MemberName}";
                if (!reported.Add(reportKey))
                {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                    BrickAnalyzerDiagnostics.BrickRuleViolation,
                    dependency.Location,
                    $"Brick dependency source and target must not be the same element '{dependency.Source.Name}' through '{dependency.MemberName}'"));
            }
        }

        private static void ReportMissingRequiredDependencies(
            CompilationAnalysisContext context,
            IEnumerable<BrickRuleInfo> rules,
            IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<string>> roleMap,
            IEnumerable<ObservedDependency> dependencies)
        {
            var dependencyList = dependencies.ToArray();
            foreach (var rule in rules.Where(rule => rule.Mode == RequireDependencyMode))
            {
                foreach (var source in roleMap.Where(entry => entry.Value.Contains(rule.SourceRole)))
                {
                    var hasRequiredDependency = dependencyList.Any(dependency =>
                        SymbolEqualityComparer.Default.Equals(dependency.Source, source.Key) &&
                        roleMap[dependency.Target].Contains(rule.TargetRole));

                    if (hasRequiredDependency)
                    {
                        continue;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(
                        BrickAnalyzerDiagnostics.BrickRuleViolation,
                        source.Key.Locations.FirstOrDefault(),
                        $"Brick rule '{rule.Id}' requires '{source.Key.Name}' to depend on a target with role '{rule.TargetRole}'"));
                }
            }
        }

        private static IEnumerable<SyntaxTargetType> CollectSyntaxTargetTypes(TypeDeclarationInfo declaration)
        {
            var targetTypes = new List<SyntaxTargetType>();
            var descendants = declaration.Declaration.DescendantNodes(node =>
                object.ReferenceEquals(node, declaration.Declaration) || !(node is TypeDeclarationSyntax));

            foreach (var localDeclaration in descendants.OfType<LocalDeclarationStatementSyntax>())
            {
                var type = declaration.SemanticModel.GetTypeInfo(localDeclaration.Declaration.Type).Type;
                if (type != null)
                {
                    targetTypes.Add(new SyntaxTargetType(
                        type,
                        FindContainingMemberName(declaration.SemanticModel, localDeclaration),
                        localDeclaration.Declaration.Type.GetLocation()));
                }
            }

            foreach (var objectCreation in descendants.OfType<ObjectCreationExpressionSyntax>())
            {
                var type = declaration.SemanticModel.GetTypeInfo(objectCreation).Type;
                if (type != null)
                {
                    targetTypes.Add(new SyntaxTargetType(
                        type,
                        FindContainingMemberName(declaration.SemanticModel, objectCreation),
                        objectCreation.Type.GetLocation()));
                }
            }

            foreach (var objectCreation in descendants.OfType<ImplicitObjectCreationExpressionSyntax>())
            {
                var type = declaration.SemanticModel.GetTypeInfo(objectCreation).Type;
                if (type != null)
                {
                    targetTypes.Add(new SyntaxTargetType(
                        type,
                        FindContainingMemberName(declaration.SemanticModel, objectCreation),
                        objectCreation.GetLocation()));
                }
            }

            return targetTypes;
        }

        private static string FindContainingMemberName(SemanticModel semanticModel, SyntaxNode node)
        {
            var member = node.Ancestors().OfType<MemberDeclarationSyntax>().FirstOrDefault();
            if (member == null)
            {
                return "member body";
            }

            var symbol = semanticModel.GetDeclaredSymbol(member);
            return symbol?.Name ?? "member body";
        }

        private static IEnumerable<ITypeSymbol> GetMemberTargetTypes(ISymbol member)
        {
            var field = member as IFieldSymbol;
            if (field != null)
            {
                yield return field.Type;
            }

            var property = member as IPropertySymbol;
            if (property != null)
            {
                yield return property.Type;
            }

            var method = member as IMethodSymbol;
            if (method != null && method.MethodKind != MethodKind.PropertyGet && method.MethodKind != MethodKind.PropertySet)
            {
                if (method.ReturnsVoid == false)
                {
                    yield return method.ReturnType;
                }

                foreach (var parameter in method.Parameters)
                {
                    yield return parameter.Type;
                }
            }
        }

        private static INamedTypeSymbol NormalizeType(ITypeSymbol type)
        {
            var named = type as INamedTypeSymbol;
            if (named == null)
            {
                return null;
            }

            return named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T && named.TypeArguments.Length == 1
                ? named.TypeArguments[0] as INamedTypeSymbol
                : named;
        }

        private static IEnumerable<ITypeSymbol> ExpandTargetTypes(ITypeSymbol type)
        {
            yield return type;

            var array = type as IArrayTypeSymbol;
            if (array != null)
            {
                foreach (var nested in ExpandTargetTypes(array.ElementType))
                {
                    yield return nested;
                }

                yield break;
            }

            var named = type as INamedTypeSymbol;
            if (named != null)
            {
                foreach (var typeArgument in named.TypeArguments)
                {
                    foreach (var nested in ExpandTargetTypes(typeArgument))
                    {
                        yield return nested;
                    }
                }

                yield break;
            }

            var pointer = type as IPointerTypeSymbol;
            if (pointer != null)
            {
                foreach (var nested in ExpandTargetTypes(pointer.PointedAtType))
                {
                    yield return nested;
                }
            }
        }

        private static INamedTypeSymbol FindType(IEnumerable<INamedTypeSymbol> types, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return types.FirstOrDefault(type =>
                type.Name == name ||
                type.ToDisplayString() == name ||
                type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty) == name);
        }

        private static bool IsRuleAttribute(AttributeData attribute)
        {
            var attributeType = attribute.AttributeClass;
            return attributeType != null && BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RuleAttribute);
        }

        private static bool IsDependencyAttribute(AttributeData attribute)
        {
            var attributeType = attribute.AttributeClass;
            return attributeType != null && BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.DependencyAttribute);
        }

        private static BrickRuleInfo ReadRule(AttributeData attribute)
        {
            return new BrickRuleInfo(
                BrickAnalyzerFacts.GetAttributeString(attribute, 0, "Id"),
                BrickAnalyzerFacts.GetAttributeString(attribute, 1, "SourceRole"),
                BrickAnalyzerFacts.GetAttributeString(attribute, 2, "TargetRole"),
                BrickAnalyzerFacts.GetAttributeEnum(attribute, 3, "Mode", ForbidDependencyMode));
        }

        private static string TryGetRoleName(AttributeData attribute)
        {
            var attributeType = attribute.AttributeClass;
            if (attributeType == null)
            {
                return null;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RoleAttribute))
            {
                var directRole = BrickAnalyzerFacts.GetAttributeString(attribute, 0, "Name");
                if (!string.IsNullOrWhiteSpace(directRole))
                {
                    return directRole;
                }
            }

            foreach (var marker in attributeType.GetAttributes())
            {
                var markerType = marker.AttributeClass;
                if (BrickAnalyzerFacts.IsOrDerivesFrom(markerType, BrickAnalyzerFacts.RoleAliasAttribute))
                {
                    return BrickAnalyzerFacts.GetAttributeString(marker, 0, "Role");
                }
            }

            return null;
        }

        private readonly struct TypeDeclarationInfo
        {
            public TypeDeclarationInfo(INamedTypeSymbol symbol, TypeDeclarationSyntax declaration, SemanticModel semanticModel)
            {
                Symbol = symbol;
                Declaration = declaration;
                SemanticModel = semanticModel;
            }

            public INamedTypeSymbol Symbol { get; }

            public TypeDeclarationSyntax Declaration { get; }

            public SemanticModel SemanticModel { get; }
        }

        private readonly struct BrickRuleInfo
        {
            public BrickRuleInfo(string id, string sourceRole, string targetRole, int mode)
            {
                Id = id;
                SourceRole = sourceRole;
                TargetRole = targetRole;
                Mode = mode;
            }

            public string Id { get; }

            public string SourceRole { get; }

            public string TargetRole { get; }

            public int Mode { get; }

            public bool IsUsable =>
                !string.IsNullOrWhiteSpace(Id) &&
                !string.IsNullOrWhiteSpace(SourceRole) &&
                !string.IsNullOrWhiteSpace(TargetRole);
        }

        private readonly struct ObservedDependency
        {
            public ObservedDependency(INamedTypeSymbol source, INamedTypeSymbol target, string memberName, Location location)
            {
                Source = source;
                Target = target;
                MemberName = memberName;
                Location = location;
            }

            public INamedTypeSymbol Source { get; }

            public INamedTypeSymbol Target { get; }

            public string MemberName { get; }

            public Location Location { get; }
        }

        private readonly struct SyntaxTargetType
        {
            public SyntaxTargetType(ITypeSymbol type, string memberName, Location location)
            {
                Type = type;
                MemberName = memberName;
                Location = location;
            }

            public ITypeSymbol Type { get; }

            public string MemberName { get; }

            public Location Location { get; }
        }
    }
}
