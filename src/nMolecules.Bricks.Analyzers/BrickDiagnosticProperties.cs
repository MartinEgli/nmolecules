using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace NMolecules.Bricks.Analyzers
{
    internal static class BrickDiagnosticProperties
    {
        public const string ViolationKind = "BrickViolationKind";
        public const string ConfigurationKind = "BrickConfigurationKind";
        public const string RuleId = "RuleId";
        public const string PolicyId = "PolicyId";
        public const string SourceRole = "SourceRole";
        public const string TargetRole = "TargetRole";
        public const string Source = "Source";
        public const string Target = "Target";
        public const string ContractKind = "ContractKind";
        public const string CorrelationMode = "CorrelationMode";

        public static ImmutableDictionary<string, string> Create(
            string violationKind = null,
            string configurationKind = null,
            string ruleId = null,
            string policyId = null,
            string sourceRole = null,
            string targetRole = null,
            string source = null,
            string target = null,
            string contractKind = null,
            string correlationMode = null)
        {
            var builder = ImmutableDictionary.CreateBuilder<string, string>();
            Add(builder, ViolationKind, violationKind);
            Add(builder, ConfigurationKind, configurationKind);
            Add(builder, RuleId, ruleId);
            Add(builder, PolicyId, policyId);
            Add(builder, SourceRole, sourceRole);
            Add(builder, TargetRole, targetRole);
            Add(builder, Source, source);
            Add(builder, Target, target);
            Add(builder, ContractKind, contractKind);
            Add(builder, CorrelationMode, correlationMode);
            return builder.ToImmutable();
        }

        public static string ContractKindFor(DiagnosticDescriptor descriptor)
        {
            switch (descriptor.Id)
            {
                case "XMoleculesBricks0003":
                    return "ExactlyOneMember";
                case "XMoleculesBricks0004":
                    return "RequireAllMembers";
                case "XMoleculesBricks0005":
                    return "RequireMemberCount";
                case "XMoleculesBricks0006":
                    return "ExclusiveChoice";
                case "XMoleculesBricks0007":
                    return "MemberRange";
                case "XMoleculesBricks0008":
                    return "ForbiddenMember";
                case "XMoleculesBricks0009":
                    return "UniqueNamedMember";
                case "XMoleculesBricks0010":
                    return "RequiredNamedMembers";
                default:
                    return string.Empty;
            }
        }

        public static Diagnostic Create(
            DiagnosticDescriptor descriptor,
            Location location,
            string message,
            string violationKind = null,
            string configurationKind = null,
            string ruleId = null,
            string policyId = null,
            string sourceRole = null,
            string targetRole = null,
            string source = null,
            string target = null,
            string contractKind = null,
            string correlationMode = null)
        {
            return Diagnostic.Create(
                descriptor,
                location,
                additionalLocations: null,
                properties: Create(
                    violationKind,
                    configurationKind,
                    ruleId,
                    policyId,
                    sourceRole,
                    targetRole,
                    source,
                    target,
                    contractKind,
                    correlationMode),
                messageArgs: new object[] { message });
        }

        private static void Add(ImmutableDictionary<string, string>.Builder builder, string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                builder[key] = value;
            }
        }
    }
}
