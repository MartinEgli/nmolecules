using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Evaluates name-convention attributes against class, struct and interface names.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickNameConventionAnalyzer : DiagnosticAnalyzer
    {
        private const int NamePositionAny = 0;
        private const int NamePositionPrefix = 1;
        private const int NamePositionSuffix = 2;
        private const int NamePositionContains = 3;
        private const int NamePositionExact = 4;
        private const int OverrideSuppress = 0;
        private const int OverridePrefer = 1;

        /// <summary>
        /// Gets the diagnostics supported by the analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(
                BrickAnalyzerDiagnostics.BrickNameConventionViolation,
                BrickAnalyzerDiagnostics.BrickNameConventionConflict,
                BrickAnalyzerDiagnostics.BrickNameConventionAliasConfiguration,
                BrickAnalyzerDiagnostics.BrickNameConventionOverrideConfiguration);

        /// <summary>
        /// Registers compilation analysis for name-convention metadata.
        /// </summary>
        /// <param name="context">Analyzer registration context.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationAction(AnalyzeCompilation);
        }

        private static void AnalyzeCompilation(CompilationAnalysisContext context)
        {
            var namedTypes = CollectNamedTypes(context.Compilation.GlobalNamespace).ToArray();
            var overrideAliases = CollectOverrideAliases(context.Compilation, namedTypes);

            foreach (var type in namedTypes)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                ReportInvalidAliases(context, type);

                if (!CanHaveNameConventionDiagnostics(type))
                {
                    continue;
                }

                var constraints = CollectCandidateConstraints(type).ToList();
                var overrides = CollectOverrides(type, overrideAliases);
                ApplyOverrides(context, type, constraints, overrides);
                ReportConflicts(context, type, constraints);
                ReportViolations(context, type, constraints);
            }
        }

        private static bool CanHaveNameConventionDiagnostics(INamedTypeSymbol type) =>
            type.TypeKind == TypeKind.Class ||
            type.TypeKind == TypeKind.Struct ||
            type.TypeKind == TypeKind.Interface;

        private static IEnumerable<INamedTypeSymbol> CollectNamedTypes(INamespaceSymbol namespaceSymbol)
        {
            foreach (var member in namespaceSymbol.GetMembers())
            {
                var nestedNamespace = member as INamespaceSymbol;
                if (nestedNamespace != null)
                {
                    foreach (var nestedType in CollectNamedTypes(nestedNamespace))
                    {
                        yield return nestedType;
                    }

                    continue;
                }

                var namedType = member as INamedTypeSymbol;
                if (namedType != null)
                {
                    foreach (var type in CollectNamedTypes(namedType))
                    {
                        yield return type;
                    }
                }
            }
        }

        private static IEnumerable<INamedTypeSymbol> CollectNamedTypes(INamedTypeSymbol type)
        {
            yield return type;

            foreach (var nestedType in type.GetTypeMembers())
            {
                foreach (var candidate in CollectNamedTypes(nestedType))
                {
                    yield return candidate;
                }
            }
        }

        private static IReadOnlyList<NameOverride> CollectOverrideAliases(
            Compilation compilation,
            IReadOnlyList<INamedTypeSymbol> namedTypes)
        {
            var overrides = new List<NameOverride>();
            AddOverrideAliases(overrides, compilation.Assembly.GetAttributes());
            AddOverrideAliases(overrides, compilation.SourceModule.GetAttributes());

            foreach (var type in namedTypes)
            {
                AddOverrideAliases(overrides, type.GetAttributes());
            }

            return overrides;
        }

        private static void AddOverrideAliases(List<NameOverride> overrides, ImmutableArray<AttributeData> attributes)
        {
            foreach (var attribute in attributes)
            {
                if (!IsAttribute(attribute, BrickAnalyzerFacts.NameConventionOverrideAliasAttribute))
                {
                    continue;
                }

                var targetType = GetTypeArgument(attribute, 0, "TargetType");
                var sourceType = GetTypeArgument(attribute, 1, "SuppressedSource");
                overrides.Add(new NameOverride(
                    targetType,
                    sourceType,
                    GetEnumArgument(attribute, 2, "Behavior", OverrideSuppress),
                    attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation()));
            }
        }

        private static void ReportInvalidAliases(CompilationAnalysisContext context, INamedTypeSymbol type)
        {
            foreach (var alias in GetAttributes(type, BrickAnalyzerFacts.NameConventionAliasAttribute))
            {
                var sourceType = GetTypeArgument(alias, 0, "SourceType");
                if (sourceType != null && HasDirectNameConventions(sourceType))
                {
                    continue;
                }

                var message = sourceType == null
                    ? $"NameConventionAlias on '{type.Name}' must reference a convention source type."
                    : $"NameConventionAlias on '{type.Name}' references '{sourceType.Name}', but that type declares no NameConventionAttribute.";

                context.ReportDiagnostic(BrickDiagnosticProperties.Create(
                    BrickAnalyzerDiagnostics.BrickNameConventionAliasConfiguration,
                    alias.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? FirstLocation(type),
                    message,
                    violationKind: "ElementConstraint",
                    configurationKind: "NameConventionAlias",
                    source: type.Name,
                    target: sourceType?.Name));
            }
        }

        private static List<NameConstraint> CollectCandidateConstraints(INamedTypeSymbol type)
        {
            var result = new List<NameConstraint>();
            foreach (var carrier in GetConventionCarriers(type))
            {
                AddDirectConstraints(result, type, carrier);
                AddAliasConstraints(result, type, carrier);
            }

            return result;
        }

        private static IEnumerable<INamedTypeSymbol> GetConventionCarriers(INamedTypeSymbol type)
        {
            for (var current = type.BaseType; current != null; current = current.BaseType)
            {
                if (current.SpecialType == SpecialType.System_Object)
                {
                    break;
                }

                yield return current;
            }

            foreach (var candidate in type.AllInterfaces)
            {
                yield return candidate;
            }
        }

        private static void AddDirectConstraints(
            List<NameConstraint> constraints,
            INamedTypeSymbol targetType,
            INamedTypeSymbol carrier)
        {
            foreach (var attribute in GetAttributes(carrier, BrickAnalyzerFacts.NameConventionAttribute))
            {
                var constraint = CreateConstraint(targetType, carrier, carrier, attribute, null);
                if (constraint != null)
                {
                    constraints.Add(constraint);
                }
            }
        }

        private static void AddAliasConstraints(
            List<NameConstraint> constraints,
            INamedTypeSymbol targetType,
            INamedTypeSymbol carrier)
        {
            foreach (var alias in GetAttributes(carrier, BrickAnalyzerFacts.NameConventionAliasAttribute))
            {
                var sourceType = GetTypeArgument(alias, 0, "SourceType");
                if (sourceType == null)
                {
                    continue;
                }

                var restriction = GetEnumArgument(alias, -1, "RestrictToPosition", NamePositionAny);
                var aliasReason = GetStringArgument(alias, "Reason");
                foreach (var convention in GetAttributes(sourceType, BrickAnalyzerFacts.NameConventionAttribute))
                {
                    var position = GetEnumArgument(convention, 1, "Position", NamePositionContains);
                    if (restriction != NamePositionAny && restriction != position)
                    {
                        continue;
                    }

                    var constraint = CreateConstraint(targetType, carrier, sourceType, convention, aliasReason);
                    if (constraint != null)
                    {
                        constraints.Add(constraint);
                    }
                }
            }
        }

        private static NameConstraint CreateConstraint(
            INamedTypeSymbol targetType,
            INamedTypeSymbol carrier,
            INamedTypeSymbol sourceType,
            AttributeData attribute,
            string aliasReason)
        {
            var directOnly = GetBoolArgument(attribute, "DirectOnly");
            if (directOnly && !IsDirectCarrier(targetType, carrier))
            {
                return null;
            }

            var pattern = GetStringArgument(attribute, 0, "Pattern") ?? string.Empty;
            var position = GetEnumArgument(attribute, 1, "Position", NamePositionContains);
            var reason = GetStringArgument(attribute, "Reason");
            if (string.IsNullOrWhiteSpace(reason))
            {
                reason = aliasReason;
            }

            return new NameConstraint(
                sourceType,
                pattern,
                position,
                reason,
                attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation());
        }

        private static bool IsDirectCarrier(INamedTypeSymbol targetType, INamedTypeSymbol carrier)
        {
            if (carrier.TypeKind == TypeKind.Interface)
            {
                return targetType.Interfaces.Any(candidate => SymbolEqualityComparer.Default.Equals(candidate, carrier));
            }

            return SymbolEqualityComparer.Default.Equals(targetType.BaseType, carrier);
        }

        private static IReadOnlyList<NameOverride> CollectOverrides(
            INamedTypeSymbol type,
            IReadOnlyList<NameOverride> overrideAliases)
        {
            var result = new List<NameOverride>();
            foreach (var attribute in GetAttributes(type, BrickAnalyzerFacts.NameConventionOverrideAttribute))
            {
                result.Add(new NameOverride(
                    type,
                    GetTypeArgument(attribute, 0, "SuppressedSource"),
                    GetEnumArgument(attribute, 1, "Behavior", OverrideSuppress),
                    attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation()));
            }

            result.AddRange(overrideAliases.Where(overrideAlias =>
                SymbolEqualityComparer.Default.Equals(overrideAlias.TargetType, type)));

            return result;
        }

        private static void ApplyOverrides(
            CompilationAnalysisContext context,
            INamedTypeSymbol type,
            List<NameConstraint> constraints,
            IReadOnlyList<NameOverride> overrides)
        {
            foreach (var nameOverride in overrides)
            {
                if (nameOverride.SourceType == null ||
                    !constraints.Any(constraint => SymbolEqualityComparer.Default.Equals(constraint.SourceType, nameOverride.SourceType)))
                {
                    var sourceName = nameOverride.SourceType?.Name ?? "<missing>";
                    context.ReportDiagnostic(BrickDiagnosticProperties.Create(
                        BrickAnalyzerDiagnostics.BrickNameConventionOverrideConfiguration,
                        nameOverride.Location ?? FirstLocation(type),
                        $"NameConventionOverride on '{type.Name}' references '{sourceName}', but that convention source is not active on the target type.",
                        violationKind: "ElementConstraint",
                        configurationKind: "NameConventionOverride",
                        source: type.Name,
                        target: sourceName));
                    continue;
                }

                if (nameOverride.Behavior == OverridePrefer)
                {
                    ApplyPreferOverride(constraints, nameOverride.SourceType);
                }
                else
                {
                    constraints.RemoveAll(constraint => SymbolEqualityComparer.Default.Equals(constraint.SourceType, nameOverride.SourceType));
                }
            }
        }

        private static void ApplyPreferOverride(List<NameConstraint> constraints, INamedTypeSymbol preferredSource)
        {
            var preferred = constraints
                .Where(constraint => SymbolEqualityComparer.Default.Equals(constraint.SourceType, preferredSource))
                .ToArray();

            if (preferred.Length == 0)
            {
                return;
            }

            constraints.RemoveAll(constraint =>
                !SymbolEqualityComparer.Default.Equals(constraint.SourceType, preferredSource) &&
                preferred.Any(preferredConstraint => ConstraintsConflict(preferredConstraint, constraint)));
        }

        private static void ReportConflicts(
            CompilationAnalysisContext context,
            INamedTypeSymbol type,
            IReadOnlyList<NameConstraint> constraints)
        {
            for (var leftIndex = 0; leftIndex < constraints.Count; leftIndex++)
            {
                for (var rightIndex = leftIndex + 1; rightIndex < constraints.Count; rightIndex++)
                {
                    var left = constraints[leftIndex];
                    var right = constraints[rightIndex];
                    if (!ConstraintsConflict(left, right))
                    {
                        continue;
                    }

                    context.ReportDiagnostic(BrickDiagnosticProperties.Create(
                        BrickAnalyzerDiagnostics.BrickNameConventionConflict,
                        FirstLocation(type),
                        $"'{type.Name}' has conflicting naming conventions: '{left.SourceType.Name}' requires {FormatPosition(left.Position)} '{left.Pattern}', while '{right.SourceType.Name}' requires {FormatPosition(right.Position)} '{right.Pattern}'.",
                        violationKind: "ElementConstraintConflict",
                        source: left.SourceType.Name,
                        target: type.Name));
                    return;
                }
            }
        }

        private static void ReportViolations(
            CompilationAnalysisContext context,
            INamedTypeSymbol type,
            IEnumerable<NameConstraint> constraints)
        {
            foreach (var constraint in constraints)
            {
                if (NameMatches(type.Name, constraint.Pattern, constraint.Position))
                {
                    continue;
                }

                var reason = string.IsNullOrWhiteSpace(constraint.Reason)
                    ? string.Empty
                    : " " + constraint.Reason;
                context.ReportDiagnostic(BrickDiagnosticProperties.Create(
                    BrickAnalyzerDiagnostics.BrickNameConventionViolation,
                    FirstLocation(type),
                    $"'{type.Name}' implements or inherits '{constraint.SourceType.Name}' but its name does not {FormatPositionVerb(constraint.Position)} '{constraint.Pattern}'. Convention source: {constraint.SourceType.Name}.{reason}",
                    violationKind: "ElementConstraint",
                    source: constraint.SourceType.Name,
                    target: type.Name));
            }
        }

        private static bool ConstraintsConflict(NameConstraint left, NameConstraint right)
        {
            if (string.IsNullOrEmpty(left.Pattern) || string.IsNullOrEmpty(right.Pattern))
            {
                return false;
            }

            if (left.Position == NamePositionExact)
            {
                return !NameMatches(left.Pattern, right.Pattern, right.Position);
            }

            if (right.Position == NamePositionExact)
            {
                return !NameMatches(right.Pattern, left.Pattern, left.Position);
            }

            if (left.Position == NamePositionPrefix && right.Position == NamePositionPrefix)
            {
                return !left.Pattern.StartsWith(right.Pattern, StringComparison.Ordinal) &&
                    !right.Pattern.StartsWith(left.Pattern, StringComparison.Ordinal);
            }

            if (left.Position == NamePositionSuffix && right.Position == NamePositionSuffix)
            {
                return !left.Pattern.EndsWith(right.Pattern, StringComparison.Ordinal) &&
                    !right.Pattern.EndsWith(left.Pattern, StringComparison.Ordinal);
            }

            return false;
        }

        private static bool NameMatches(string name, string pattern, int position)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return true;
            }

            switch (position)
            {
                case NamePositionPrefix:
                    return name.StartsWith(pattern, StringComparison.Ordinal);
                case NamePositionSuffix:
                    return name.EndsWith(pattern, StringComparison.Ordinal);
                case NamePositionExact:
                    return string.Equals(name, pattern, StringComparison.Ordinal);
                case NamePositionAny:
                case NamePositionContains:
                default:
                    return name.Contains(pattern);
            }
        }

        private static bool HasDirectNameConventions(INamedTypeSymbol type) =>
            type.GetAttributes().Any(attribute => IsAttribute(attribute, BrickAnalyzerFacts.NameConventionAttribute));

        private static IEnumerable<AttributeData> GetAttributes(INamedTypeSymbol type, string metadataName) =>
            type.GetAttributes().Where(attribute => IsAttribute(attribute, metadataName));

        private static bool IsAttribute(AttributeData attribute, string metadataName)
        {
            var attributeType = attribute.AttributeClass;
            return attributeType != null && BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, metadataName);
        }

        private static INamedTypeSymbol GetTypeArgument(AttributeData attribute, int ordinal, string propertyName)
        {
            if (ordinal >= 0 && ordinal < attribute.ConstructorArguments.Length)
            {
                return attribute.ConstructorArguments[ordinal].Value as INamedTypeSymbol;
            }

            foreach (var argument in attribute.NamedArguments)
            {
                if (argument.Key == propertyName)
                {
                    return argument.Value.Value as INamedTypeSymbol;
                }
            }

            return null;
        }

        private static string GetStringArgument(AttributeData attribute, int ordinal, string propertyName)
        {
            if (ordinal >= 0 && ordinal < attribute.ConstructorArguments.Length)
            {
                return attribute.ConstructorArguments[ordinal].Value as string;
            }

            return GetStringArgument(attribute, propertyName);
        }

        private static string GetStringArgument(AttributeData attribute, string propertyName)
        {
            foreach (var argument in attribute.NamedArguments)
            {
                if (argument.Key == propertyName)
                {
                    return argument.Value.Value as string;
                }
            }

            return string.Empty;
        }

        private static int GetEnumArgument(AttributeData attribute, int ordinal, string propertyName, int defaultValue)
        {
            if (ordinal >= 0 && ordinal < attribute.ConstructorArguments.Length && attribute.ConstructorArguments[ordinal].Value is int)
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

        private static bool GetBoolArgument(AttributeData attribute, string propertyName)
        {
            foreach (var argument in attribute.NamedArguments)
            {
                if (argument.Key == propertyName && argument.Value.Value is bool)
                {
                    return (bool)argument.Value.Value;
                }
            }

            return false;
        }

        private static Location FirstLocation(INamedTypeSymbol type) =>
            type.Locations.FirstOrDefault() ?? Location.None;

        private static string FormatPosition(int position)
        {
            switch (position)
            {
                case NamePositionPrefix:
                    return "prefix";
                case NamePositionSuffix:
                    return "suffix";
                case NamePositionExact:
                    return "exact name";
                case NamePositionContains:
                default:
                    return "contains";
            }
        }

        private static string FormatPositionVerb(int position)
        {
            switch (position)
            {
                case NamePositionPrefix:
                    return "begin with";
                case NamePositionSuffix:
                    return "end with";
                case NamePositionExact:
                    return "equal";
                case NamePositionContains:
                default:
                    return "contain";
            }
        }

        private sealed class NameConstraint
        {
            public NameConstraint(
                INamedTypeSymbol sourceType,
                string pattern,
                int position,
                string reason,
                Location location)
            {
                SourceType = sourceType;
                Pattern = pattern ?? string.Empty;
                Position = position;
                Reason = reason ?? string.Empty;
                Location = location;
            }

            public INamedTypeSymbol SourceType { get; }

            public string Pattern { get; }

            public int Position { get; }

            public string Reason { get; }

            public Location Location { get; }
        }

        private sealed class NameOverride
        {
            public NameOverride(
                INamedTypeSymbol targetType,
                INamedTypeSymbol sourceType,
                int behavior,
                Location location)
            {
                TargetType = targetType;
                SourceType = sourceType;
                Behavior = behavior;
                Location = location;
            }

            public INamedTypeSymbol TargetType { get; }

            public INamedTypeSymbol SourceType { get; }

            public int Behavior { get; }

            public Location Location { get; }
        }
    }
}
