using System.Collections.Immutable;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Validates default Bricks policy declarations that apply before dependency rules are evaluated.
    /// </summary>
    /// <remarks>
    /// Default-deny dependency violations are still reported by <see cref="BrickDependencyRuleAnalyzer"/>.
    /// This analyzer focuses on policy metadata that would make that evaluation ambiguous.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickDefaultPolicyAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Gets the diagnostics produced directly by this entry point.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(
                BrickAnalyzerDiagnostics.BrickConfiguration,
                BrickAnalyzerDiagnostics.BrickPolicyConfiguration);

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
            var decisionByPolicy = new Dictionary<string, int>(System.StringComparer.Ordinal);
            foreach (var attribute in BrickAnalyzerAttributeUtilities.GetAttributes(context.Compilation, BrickAnalyzerFacts.PolicyAttribute))
            {
                var policyId = BrickAnalyzerFacts.GetAttributeString(attribute, 0, "Id");
                if (string.IsNullOrWhiteSpace(policyId))
                {
                    continue;
                }

                var defaultDecision = BrickAnalyzerFacts.GetAttributeEnum(attribute, 2, "DefaultDecision", 0);
                int previousDecision;
                if (!decisionByPolicy.TryGetValue(policyId, out previousDecision))
                {
                    decisionByPolicy.Add(policyId, defaultDecision);
                    continue;
                }

                if (previousDecision == defaultDecision)
                {
                    continue;
                }

                context.ReportDiagnostic(BrickDiagnosticProperties.Create(
                    BrickAnalyzerDiagnostics.BrickPolicyConfiguration,
                    BrickAnalyzerAttributeUtilities.GetLocation(attribute),
                    $"Policy '{policyId}' declares conflicting default decisions",
                    configurationKind: "Policy",
                    policyId: policyId,
                    correlationMode: "PolicyId"));
            }
        }
    }
}
