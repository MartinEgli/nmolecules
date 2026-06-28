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
            description: "Analyzer-enforced brick policies and contracts must be fully declared so the rule engine can evaluate them deterministically.");

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
    }
}
