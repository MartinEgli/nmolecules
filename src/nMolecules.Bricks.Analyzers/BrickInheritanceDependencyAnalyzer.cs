using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Validates inheritance-specific Bricks dependency evidence.
    /// </summary>
    /// <remarks>
    /// Inheritance rule violations are evaluated by <see cref="BrickDependencyRuleAnalyzer"/>.
    /// This analyzer reports roled inheritance edges that are not covered by an explicit
    /// <c>RuleAttribute</c>, so teams can decide whether the architectural inheritance edge is
    /// intentional.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickInheritanceDependencyAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Gets the diagnostics produced directly by this entry point.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(
                BrickAnalyzerDiagnostics.BrickConfiguration,
                BrickAnalyzerDiagnostics.BrickEvidenceConfiguration);

        /// <summary>
        /// Initializes the analyzer entry point.
        /// </summary>
        /// <param name="context">The Roslyn analysis context.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationAction(AnalyzeCompilation);
        }

        private static void AnalyzeCompilation(CompilationAnalysisContext context)
        {
            var rulePairs = new HashSet<string>(
                BrickAnalyzerAttributeUtilities.GetAttributes(context.Compilation, BrickAnalyzerFacts.RuleAttribute)
                    .Select(attribute => Pair(
                        BrickAnalyzerFacts.GetAttributeString(attribute, 1, "SourceRole"),
                        BrickAnalyzerFacts.GetAttributeString(attribute, 2, "TargetRole")))
                    .Where(pair => pair != "\u001f"),
                System.StringComparer.Ordinal);

            var rolesByType = BrickAnalyzerAttributeUtilities.GetDeclaredTypes(context.Compilation)
                .Select(type => new TypeRoles(type, GetRoles(type)))
                .Where(item => item.Roles.Length > 0)
                .ToArray();

            var rolesBySymbol = rolesByType.ToDictionary(item => item.Type, item => item.Roles, SymbolEqualityComparer.Default);
            foreach (var item in rolesByType)
            {
                foreach (var target in GetInheritanceTargets(item.Type))
                {
                    string[] targetRoles;
                    if (!rolesBySymbol.TryGetValue(target, out targetRoles))
                    {
                        continue;
                    }

                    foreach (var sourceRole in item.Roles)
                    {
                        foreach (var targetRole in targetRoles)
                        {
                            if (rulePairs.Contains(Pair(sourceRole, targetRole)))
                            {
                                continue;
                            }

                            context.ReportDiagnostic(BrickDiagnosticProperties.Create(
                                BrickAnalyzerDiagnostics.BrickEvidenceConfiguration,
                                item.Type.Locations.FirstOrDefault(),
                                $"Inheritance dependency from role '{sourceRole}' to role '{targetRole}' is not covered by an explicit Brick rule",
                                configurationKind: "Evidence",
                                sourceRole: sourceRole,
                                targetRole: targetRole,
                                source: item.Type.Name,
                                target: target.Name));
                        }
                    }
                }
            }
        }

        private static IEnumerable<INamedTypeSymbol> GetInheritanceTargets(INamedTypeSymbol type)
        {
            if (type.BaseType != null && type.BaseType.SpecialType != SpecialType.System_Object)
            {
                yield return type.BaseType;
            }

            foreach (var implementedInterface in type.AllInterfaces)
            {
                yield return implementedInterface;
            }
        }

        private static string[] GetRoles(INamedTypeSymbol type)
        {
            var roles = new List<string>();
            foreach (var attribute in type.GetAttributes())
            {
                var role = TryGetRoleName(attribute);
                if (!string.IsNullOrWhiteSpace(role) && !roles.Contains(role))
                {
                    roles.Add(role);
                }
            }

            return roles.ToArray();
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

        private static string Pair(string sourceRole, string targetRole) =>
            (sourceRole ?? string.Empty) + "\u001f" + (targetRole ?? string.Empty);

        private readonly struct TypeRoles
        {
            public TypeRoles(INamedTypeSymbol type, string[] roles)
            {
                Type = type;
                Roles = roles;
            }

            public INamedTypeSymbol Type { get; }

            public string[] Roles { get; }
        }
    }
}
