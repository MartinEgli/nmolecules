using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Provides built-in Bricks defaults for roadmap stages, readiness checks, and implementation progress.
/// </summary>
public static class BrickBuiltInRoadmapStages
    {
        /// <summary>
        /// Gets the v1 value used by Bricks developer tooling.
        /// </summary>
        public static BrickRoadmapStageDefinition V1 => Stage(
            BrickRoadmapStage.V1,
            "V1",
            "Deliberately small baseline for direct marking and static validation.",
            new[]
            {
                Item("direct-role-assignment", "Direct role assignment", true, "Roles can be assigned directly."),
                Item("alias-based-role-mapping", "Alias-based role mapping", true, "Aliases can map existing markers to roles."),
                Item("typed-role-rule-identifiers", "Typed role and rule identifiers", true, "Identifiers are stable value objects."),
                Item("minimal-role-dimensions", "Minimal role dimensions", true, "Role dimensions exist for core classification."),
                Item("deterministic-role-resolution", "Deterministic role resolution", true, "Role resolution is stable for the same input."),
                Item("basic-resolution-conflict-diagnostics", "Basic resolution conflict diagnostics", true, "Conflicts are represented explicitly."),
                Item("deterministic-rule-evaluation", "Deterministic rule evaluation", true, "Policy rules evaluate deterministically."),
                Item("explicit-policy-defaults", "Explicit policy defaults", true, "Policies expose explicit defaults."),
                Item("static-role-based-dependency-validation", "Static role-based dependency validation", true, "Static dependencies can be evaluated by role."),
                Item("allow-deny-require-decision-semantics", "Allow, Deny, and Require semantics", true, "Decision types remain distinct."),
                Item("type-scope-support", "Type scope support", true, "At least type-level evaluation is supported."),
                Item("member-cardinality-contract-validation", "Member cardinality contract validation", true, "Member cardinality contracts can be checked."),
                Item("analyzer-diagnostics", "Analyzer diagnostics", true, "Validation can produce diagnostics."),
                Item("minimal-suppression-support", "Minimal suppression support", true, "Accepted exceptions can be represented.")
            },
            new[]
            {
                Item("full-external-policy-files", "Full external policy files", false, "V1 does not require full external policy files."),
                Item("di-registration-analysis", "DI registration analysis", false, "V1 does not require DI registration analysis."),
                Item("reflection-analysis", "Reflection analysis", false, "V1 does not require reflection analysis."),
                Item("runtime-wiring-analysis", "Runtime wiring analysis", false, "V1 does not require runtime wiring analysis."),
                Item("rich-export-reporting", "Rich export reporting", false, "V1 does not require rich export reporting."),
                Item("broad-ide-visualisation", "Broad IDE visualisation", false, "V1 does not require broad IDE visualisation."),
                Item("complete-pack-bridge-ecosystem", "Complete pack bridge ecosystem", false, "V1 does not require a complete bridge ecosystem.")
            });

        /// <summary>
        /// Gets the v1 1 value used by Bricks developer tooling.
        /// </summary>
        public static BrickRoadmapStageDefinition V1_1 => Stage(
            BrickRoadmapStage.V1_1,
            "V1.1",
            "Policy file and report foundations.",
            new[]
            {
                Item("external-policy-file-prototype", "External policy file prototype", true, "External policy input has a first contract."),
                Item("schema-versioning-policy-files", "Schema versioning for policy files", true, "Policy files declare a schema."),
                Item("resolution-trace-export", "Resolution trace export", true, "Resolution traces can be exported."),
                Item("baseline-file-support", "Baseline file support", true, "Accepted legacy findings can be baselined."),
                Item("first-report-output", "First report output", true, "Validation output can be serialized."),
                Item("stricter-diagnostic-governance", "Stricter diagnostic governance", true, "Diagnostic IDs are governed."),
                Item("basic-pack-bridge-ddd", "Basic pack bridge for DDD", true, "DDD bridge metadata is represented.")
            },
            null);

        /// <summary>
        /// Gets the v1 2 value used by Bricks developer tooling.
        /// </summary>
        public static BrickRoadmapStageDefinition V1_2 => Stage(
            BrickRoadmapStage.V1_2,
            "V1.2",
            "Testing APIs and richer export surfaces.",
            new[]
            {
                Item("architecture-testing-api", "Architecture testing API", true, "Bricks can support architecture tests."),
                Item("json-report-export", "JSON report export", true, "Reports can be serialized to JSON."),
                Item("role-map-export", "Role map export", true, "Resolved roles can be exported."),
                Item("dependency-graph-export", "Dependency graph export", true, "Dependency graphs can be exported."),
                Item("suppression-baseline-reports", "Suppression and baseline reports", true, "Suppression and baseline state can be reported."),
                Item("pack-bridges-events-architecture", "Pack bridges for Events and Architecture", true, "Events and Architecture bridges are represented.")
            },
            null);

        /// <summary>
        /// Gets the v2 value used by Bricks developer tooling.
        /// </summary>
        public static BrickRoadmapStageDefinition V2 => Stage(
            BrickRoadmapStage.V2,
            "V2",
            "Runtime-aware analysis and richer profiles.",
            new[]
            {
                Item("di-registration-dependency-analysis", "DI registration dependency analysis", true, "DI registration dependencies can be analysed."),
                Item("internals-visible-to-analysis", "InternalsVisibleTo analysis", true, "Friend assembly visibility is represented."),
                Item("visibility-dependency-layer", "Visibility dependency layer", true, "Visibility dependencies are first-class."),
                Item("runtime-wiring-dependency-layer", "Runtime and wiring dependency layer", true, "Runtime wiring dependencies are first-class."),
                Item("reflection-modelling-confidence", "Reflection modelling with confidence", true, "Reflection dependencies carry confidence levels."),
                Item("advanced-policy-composition", "Advanced policy composition", true, "Policies can compose through explicit modes."),
                Item("profile-packages", "Profile packages", true, "Layered, Onion, Hexagonal, and CQRS profiles exist."),
                Item("richer-ide-support", "Richer IDE support", true, "IDE support can consume richer Bricks state.")
            },
            null);

        /// <summary>
        /// Gets the All value used by Bricks developer tooling.
        /// </summary>
        public static IReadOnlyList<BrickRoadmapStageDefinition> All => new[] { V1, V1_1, V1_2, V2 };

        private static BrickRoadmapStageDefinition Stage(
            BrickRoadmapStage stage,
            string displayName,
            string description,
            IEnumerable<BrickRoadmapItem> includedItems,
            IEnumerable<BrickRoadmapItem> excludedItems) =>
            new BrickRoadmapStageDefinition(stage, displayName, description, includedItems, excludedItems);

        private static BrickRoadmapItem Item(
            string id,
            string displayName,
            bool required,
            string rationale) =>
            new BrickRoadmapItem(id, displayName, required, rationale);
    }
}
