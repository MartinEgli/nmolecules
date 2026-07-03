using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Validates rule filter declarations before they are consumed by dependency rule evaluation.
    /// </summary>
    /// <remarks>
    /// Rule filter application is evaluated by <see cref="BrickDependencyRuleAnalyzer"/> and
    /// conflicting required/excluded filters are validated by <see cref="BrickMetadataAnalyzer"/>.
    /// This analyzer reports filter declarations that cannot be attached to a usable rule.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickRuleFilterAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Gets the diagnostics produced directly by this entry point.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(
                BrickAnalyzerDiagnostics.BrickConfiguration,
                BrickAnalyzerDiagnostics.BrickRuleFilterConfiguration);

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
            var ruleIds = new HashSet<string>(
                BrickAnalyzerAttributeUtilities.GetAttributes(context.Compilation, BrickAnalyzerFacts.RuleAttribute)
                    .Select(attribute => BrickAnalyzerFacts.GetAttributeString(attribute, 0, "Id"))
                    .Where(id => !string.IsNullOrWhiteSpace(id)),
                System.StringComparer.Ordinal);

            foreach (var attribute in BrickAnalyzerAttributeUtilities.GetAttributes(context.Compilation, BrickAnalyzerFacts.RuleFilterAttribute))
            {
                var ruleId = BrickAnalyzerFacts.GetAttributeString(attribute, 0, "Rule");
                if (string.IsNullOrWhiteSpace(ruleId))
                {
                    continue;
                }

                if (!HasFilterTokens(attribute))
                {
                    context.ReportDiagnostic(BrickDiagnosticProperties.Create(
                        BrickAnalyzerDiagnostics.BrickRuleFilterConfiguration,
                        BrickAnalyzerAttributeUtilities.GetLocation(attribute),
                        $"RuleFilterAttribute for rule '{ruleId}' must declare at least one token",
                        configurationKind: "RuleFilter",
                        ruleId: ruleId));
                    continue;
                }

                if (!ruleIds.Contains(ruleId))
                {
                    context.ReportDiagnostic(BrickDiagnosticProperties.Create(
                        BrickAnalyzerDiagnostics.BrickRuleFilterConfiguration,
                        BrickAnalyzerAttributeUtilities.GetLocation(attribute),
                        $"RuleFilterAttribute references unknown rule '{ruleId}'",
                        configurationKind: "RuleFilter",
                        ruleId: ruleId));
                }
            }
        }

        private static bool HasFilterTokens(AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length < 2)
            {
                return false;
            }

            var argument = attribute.ConstructorArguments[1];
            if (argument.Kind == TypedConstantKind.Array)
            {
                return argument.Values.Any(item => !string.IsNullOrWhiteSpace(item.Value as string));
            }

            return !string.IsNullOrWhiteSpace(argument.Value as string);
        }
    }
}
