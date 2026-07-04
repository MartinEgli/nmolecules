using Microsoft.CodeAnalysis;

namespace NMolecules.Bricks.Analyzers
{
    internal static class BrickAnalyzerDiagnostics
    {
        private const string Category = "nMolecules.Bricks";

        public static readonly DiagnosticDescriptor BrickRuleViolation = new DiagnosticDescriptor(
            "XMoleculesBricks0001",
            "Brick rules must be honored",
            "{0}",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A dependency or usage pattern violates a declared brick rule between custom architectural roles.",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public static readonly DiagnosticDescriptor BrickConfiguration = new DiagnosticDescriptor(
            "XMoleculesBricks0002",
            "Brick configuration must be valid",
            "{0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Analyzer-enforced brick policies and contracts must be fully declared so the rule engine can evaluate them deterministically.",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public static readonly DiagnosticDescriptor BrickPolicyConfiguration = new DiagnosticDescriptor(
            "XMoleculesBricks0200",
            "Brick policy configuration must be valid",
            "{0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Brick policy declarations must have stable IDs and non-conflicting defaults so rules can be correlated deterministically.",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public static readonly DiagnosticDescriptor BrickRoleConfiguration = new DiagnosticDescriptor(
            "XMoleculesBricks0201",
            "Brick role configuration must be valid",
            "{0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Brick role declarations must have stable names and avoid repeated effective role assignments.",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public static readonly DiagnosticDescriptor BrickRuleConfiguration = new DiagnosticDescriptor(
            "XMoleculesBricks0202",
            "Brick rule configuration must be valid",
            "{0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Brick rule declarations must have stable IDs, endpoints, modes and policy correlation.",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public static readonly DiagnosticDescriptor BrickDependencyConfiguration = new DiagnosticDescriptor(
            "XMoleculesBricks0203",
            "Brick dependency configuration must be valid",
            "{0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Brick dependency declarations must have stable IDs, source and target metadata, kind and evidence.",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public static readonly DiagnosticDescriptor BrickRuleFilterConfiguration = new DiagnosticDescriptor(
            "XMoleculesBricks0204",
            "Brick rule-filter configuration must be valid",
            "{0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Brick rule filters must reference declared rules and avoid contradictory source, target or member filters.",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public static readonly DiagnosticDescriptor BrickMemberContractConfiguration = new DiagnosticDescriptor(
            "XMoleculesBricks0205",
            "Brick member-contract configuration must be valid",
            "{0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Brick member-contract declarations must use valid marker attributes, counts, ranges, choices and named-member metadata.",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public static readonly DiagnosticDescriptor BrickNamespaceRoleConfiguration = new DiagnosticDescriptor(
            "XMoleculesBricks0206",
            "Brick namespace-role configuration must be valid",
            "{0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Brick namespace role declarations must use valid namespace patterns that match intended source namespaces.",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public static readonly DiagnosticDescriptor BrickEvidenceConfiguration = new DiagnosticDescriptor(
            "XMoleculesBricks0207",
            "Brick evidence configuration must be valid",
            "{0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Brick declared evidence must match available project, runtime, folder and inheritance evidence.",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public static readonly DiagnosticDescriptor BrickPackageBoundaryConfiguration = new DiagnosticDescriptor(
            "XMoleculesBricks0208",
            "Brick package-boundary configuration must be valid",
            "{0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Brick package boundaries must not create forbidden analyzer/runtime references or split-candidate leaks.",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public static readonly DiagnosticDescriptor BrickSampleConfiguration = new DiagnosticDescriptor(
            "XMoleculesBricks0209",
            "Brick sample configuration must be valid",
            "{0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Brick sample documentation and code fences must use supported analyzer sample markers.",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public static readonly DiagnosticDescriptor BrickExactlyOneMemberContract = new DiagnosticDescriptor(
            "XMoleculesBricks0003",
            "Brick member contract must declare exactly one marker",
            "{0}",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A type marked with a brick contract does not declare exactly one member carrying the required marker attribute.");

        public static readonly DiagnosticDescriptor BrickRequireAllMembersContract = new DiagnosticDescriptor(
            "XMoleculesBricks0004",
            "Brick member contract must include all required markers",
            "{0}",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A type marked with a brick contract is missing one or more required marker attributes on its members.");

        public static readonly DiagnosticDescriptor BrickMemberCountContract = new DiagnosticDescriptor(
            "XMoleculesBricks0005",
            "Brick member contract must use the configured marker count",
            "{0}",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A type marked with a brick contract does not declare the exact number of members required for a repeated marker.");

        public static readonly DiagnosticDescriptor BrickExclusiveChoiceContract = new DiagnosticDescriptor(
            "XMoleculesBricks0006",
            "Brick member contract must satisfy an exclusive choice",
            "{0}",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A type marked with a brick contract does not satisfy the required exclusive choice between two marker attributes.");

        public static readonly DiagnosticDescriptor BrickMemberRangeContract = new DiagnosticDescriptor(
            "XMoleculesBricks0007",
            "Brick member contract must stay within the configured marker range",
            "{0}",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A type marked with a brick contract does not declare a marker count within the configured inclusive range.");

        public static readonly DiagnosticDescriptor BrickForbiddenMemberContract = new DiagnosticDescriptor(
            "XMoleculesBricks0008",
            "Brick member contract must not declare a forbidden marker",
            "{0}",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A type marked with a brick contract declares one or more members carrying a forbidden marker attribute.");

        public static readonly DiagnosticDescriptor BrickUniqueNamedMemberContract = new DiagnosticDescriptor(
            "XMoleculesBricks0009",
            "Brick member contract must not duplicate a marker name",
            "{0}",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A type marked with a brick contract declares more than one member carrying the same named marker attribute.");

        public static readonly DiagnosticDescriptor BrickRequiredNamedMemberContract = new DiagnosticDescriptor(
            "XMoleculesBricks0010",
            "Brick member contract must declare required marker names",
            "{0}",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A type marked with a brick contract is missing a member carrying a required named marker attribute.");

        public static readonly DiagnosticDescriptor BrickNameConventionViolation = new DiagnosticDescriptor(
            "XMoleculesBricks0020",
            "Brick name convention must be honored",
            "{0}",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A type that inherits or implements a name-convention source does not satisfy the active naming constraint.");

        public static readonly DiagnosticDescriptor BrickNameConventionConflict = new DiagnosticDescriptor(
            "XMoleculesBricks0021",
            "Brick name conventions must not conflict",
            "{0}",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A type has active naming constraints that cannot be satisfied at the same time and no override suppresses the conflict.");

        public static readonly DiagnosticDescriptor BrickNameConventionAliasConfiguration = new DiagnosticDescriptor(
            "XMoleculesBricks0022",
            "Brick name-convention alias must reference a convention source",
            "{0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A name-convention alias references a source type that does not declare a name convention.");

        public static readonly DiagnosticDescriptor BrickNameConventionOverrideConfiguration = new DiagnosticDescriptor(
            "XMoleculesBricks0023",
            "Brick name-convention override must reference an active convention source",
            "{0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A name-convention override references a convention source that is not active on the target type.");
    }
}
