using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Provides built-in Bricks defaults for core elements, dependencies, violations, and source locations.
/// </summary>
public static class BrickBuiltInConformanceLevels
    {
        /// <summary>
        /// Gets the Marking value used by Bricks developer tooling.
        /// </summary>
        public static BrickConformanceLevelDefinition Marking => Level(
            BrickConformanceLevel.Marking,
            "Marking",
            "Structural roles can be marked and named consistently.",
            Capability("role-attributes", "Role attributes", true, "Roles can be expressed in code."),
            Capability("alias-attributes", "Alias attributes", true, "Aliases can adapt existing structural names."),
            Capability("typed-identifiers", "Typed identifiers", true, "Core identifiers avoid stringly typed policy code."));

        /// <summary>
        /// Gets the Static Validation value used by Bricks developer tooling.
        /// </summary>
        public static BrickConformanceLevelDefinition StaticValidation => Level(
            BrickConformanceLevel.StaticValidation,
            "Static validation",
            "Compiler-visible structure can be evaluated deterministically.",
            Capability("type-references", "Type references", true, "Static type dependencies are modelled."),
            Capability("inheritance", "Inheritance", true, "Inheritance dependencies are visible to static validation."),
            Capability("interface-implementation", "Interface implementation", true, "Interface implementation edges are visible to static validation."),
            Capability("object-creation", "Object creation", true, "Object creation dependencies are represented."),
            Capability("basic-rule-validation", "Basic rule validation", true, "Policies can evaluate allowed and forbidden dependencies."),
            Capability("analyzer-diagnostics", "Analyzer diagnostics", true, "Violations can surface as diagnostics."));

        /// <summary>
        /// Gets the Explainability value used by Bricks developer tooling.
        /// </summary>
        public static BrickConformanceLevelDefinition Explainability => Level(
            BrickConformanceLevel.Explainability,
            "Explainability",
            "Resolution and violation decisions can be explained.",
            Capability("resolution-traces", "Resolution traces", true, "Role resolution keeps explainable trace output."),
            Capability("evidence", "Evidence", true, "Assignments and violations preserve evidence quality."),
            Capability("normalized-violations", "Normalized violations", true, "Policy outcomes are normalized for consumption."),
            Capability("structured-diagnostic-messages", "Structured diagnostic messages", true, "Diagnostics use stable structured message data."));

        /// <summary>
        /// Gets the Policy Files value used by Bricks developer tooling.
        /// </summary>
        public static BrickConformanceLevelDefinition PolicyFiles => Level(
            BrickConformanceLevel.PolicyFiles,
            "Policy files",
            "External policy configuration can be versioned and composed.",
            Capability("schema-versioned-policy-files", "Schema-versioned policy files", true, "Policy files have a machine-readable schema."),
            Capability("baseline-files", "Baseline files", true, "Accepted legacy violations can be represented."),
            Capability("suppression-files", "Suppression files", true, "Suppressions can be represented outside source code."),
            Capability("policy-imports", "Policy imports", true, "Policies can compose other policies."),
            Capability("configuration-precedence", "Configuration precedence", true, "Configuration sources resolve deterministically."));

        /// <summary>
        /// Gets the Runtime Aware Analysis value used by Bricks developer tooling.
        /// </summary>
        public static BrickConformanceLevelDefinition RuntimeAwareAnalysis => Level(
            BrickConformanceLevel.RuntimeAwareAnalysis,
            "Runtime-aware analysis",
            "Runtime-relevant dependency kinds can be represented with evidence quality.",
            Capability("dependency-registrations", "Dependency registrations", true, "Dependency injection registrations are modelled."),
            Capability("friend-assembly", "Friend assembly", true, "InternalsVisibleTo visibility dependencies are modelled."),
            Capability("reflection-access", "Reflection access", true, "Reflection access can be represented explicitly."),
            Capability("runtime-activation", "Runtime activation", true, "Runtime activation edges are modelled."),
            Capability("evidence-confidence-levels", "Evidence confidence levels", true, "Runtime-aware analysis carries evidence confidence."));

        /// <summary>
        /// Gets the Integration And Augmentation value used by Bricks developer tooling.
        /// </summary>
        public static BrickConformanceLevelDefinition IntegrationAndAugmentation => Level(
            BrickConformanceLevel.IntegrationAndAugmentation,
            "Integration and augmentation",
            "Bricks output can be consumed by tools and improvement loops.",
            Capability("ide-visualisation", "IDE visualisation", true, "IDE surfaces can consume Bricks status."),
            Capability("report-generation", "Report generation", true, "Machine-readable reports are emitted."),
            Capability("generated-architecture-documentation", "Generated architecture documentation", true, "Architecture documentation can be generated from reports."),
            Capability("technical-integrations", "Technical integrations", true, "External tools can integrate with Bricks outputs."),
            Capability("sarif-output", "SARIF output", true, "Analyzer-compatible SARIF reports can be produced."),
            Capability("benchmarking", "Benchmarking", true, "Central Bricks behaviour can be benchmarked."));

        /// <summary>
        /// Gets the All value used by Bricks developer tooling.
        /// </summary>
        public static IReadOnlyList<BrickConformanceLevelDefinition> All => new[]
        {
            Marking,
            StaticValidation,
            Explainability,
            PolicyFiles,
            RuntimeAwareAnalysis,
            IntegrationAndAugmentation
        };

        private static BrickConformanceLevelDefinition Level(
            BrickConformanceLevel level,
            string displayName,
            string description,
            params BrickConformanceCapability[] capabilities) =>
            new BrickConformanceLevelDefinition(level, displayName, description, capabilities);

        private static BrickConformanceCapability Capability(
            string id,
            string displayName,
            bool required,
            string rationale) =>
            new BrickConformanceCapability(id, displayName, required, rationale);
    }
}
