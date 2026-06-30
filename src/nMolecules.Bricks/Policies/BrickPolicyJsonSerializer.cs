using System;
using System.Linq;
using System.Text.Json;

namespace NMolecules.Bricks
{
    /// <summary>
/// Serializes and deserializes policy serializer documents using the stable Bricks JSON format.
/// </summary>
    public static class BrickPolicyJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Deserializes a Bricks policy document from JSON.
        /// </summary>
        /// <param name="json">The JSON policy document.</param>
        /// <returns>The deserialized policy document.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the JSON does not contain a policy document object.</exception>
        public static BrickPolicyDocument Deserialize(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            var dto = JsonSerializer.Deserialize<PolicyDocumentDto>(json, Options);
            if (dto == null)
            {
                throw new ArgumentException("Policy JSON must contain an object.", nameof(json));
            }

            return new BrickPolicyDocument(ToPolicy(dto.Policy), dto.Schema);
        }

        private static BrickPolicy ToPolicy(PolicyDto dto) =>
            new BrickPolicy(
                BrickPolicyId.From(dto.Id),
                dto.Name,
                (dto.Imports ?? new ImportDto[0]).Select(ToImport),
                (dto.Rules ?? new RuleDto[0]).Select(ToRule),
                (dto.CombinationRules ?? new CombinationRuleDto[0]).Select(ToCombinationRule),
                (dto.ExternalAssignments ?? new AssignmentDto[0]).Select(ToAssignment),
                (dto.Aliases ?? new AliasDto[0]).Select(ToAlias),
                Parse<BrickPermissionDefault>(dto.DefaultDecision),
                Parse<BrickEnforcementMode>(dto.Enforcement));

        private static BrickPolicyImport ToImport(ImportDto dto) =>
            new BrickPolicyImport(BrickPolicyId.From(dto.Id), Parse<BrickPolicyImportMode>(dto.Mode));

        private static BrickRule ToRule(RuleDto dto) =>
            new BrickRule(
                RuleId.From(dto.RuleId),
                dto.Name,
                RoleId.From(dto.SourceRole),
                RoleId.From(dto.TargetRole),
                Parse<BrickDecision>(dto.Decision),
                Parse<BrickScope>(dto.Scope),
                Parse<BrickSeverity>(dto.Severity),
                dto.Priority,
                dto.Reason);

        private static BrickRoleCombinationRule ToCombinationRule(CombinationRuleDto dto) =>
            new BrickRoleCombinationRule(
                dto.Name,
                BrickRoleSelector.From(dto.LeftRole),
                BrickRoleSelector.From(dto.RightRole),
                Parse<BrickCombinationKind>(dto.Kind),
                dto.Reason);

        private static BrickRoleAssignment ToAssignment(AssignmentDto dto) =>
            new BrickRoleAssignment(
                ToSelector(dto.Selector),
                RoleId.From(dto.RoleId),
                Parse<BrickAssignmentMode>(dto.Mode),
                Parse<BrickAssignmentSource>(dto.Source),
                ToPrecedence(dto.Precedence),
                Parse<BrickAssignmentBehavior>(dto.Behavior),
                dto.Reason);

        private static BrickAlias ToAlias(AliasDto dto) =>
            new BrickAlias(
                dto.Name,
                ToSelector(dto.Selector),
                RoleId.From(dto.CanonicalRoleId),
                ToPrecedence(dto.Precedence),
                Parse<BrickAssignmentBehavior>(dto.Behavior),
                dto.Reason);

        private static BrickElementSelector ToSelector(SelectorDto dto) =>
            new BrickElementSelector(Parse<BrickElementKind>(dto.Kind), dto.Pattern, dto.AssemblyName);

        private static BrickAssignmentPrecedence ToPrecedence(PrecedenceDto dto) =>
            new BrickAssignmentPrecedence(
                Parse<BrickAssignmentSpecificity>(dto.Specificity),
                Parse<BrickAssignmentAuthority>(dto.Authority));

        private static TEnum Parse<TEnum>(string value)
            where TEnum : struct =>
            (TEnum)Enum.Parse(typeof(TEnum), value, false);

        private sealed class PolicyDocumentDto
        {
            public string Schema { get; set; }
            public PolicyDto Policy { get; set; }
        }

        private sealed class PolicyDto
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string DefaultDecision { get; set; }
            public string Enforcement { get; set; }
            public ImportDto[] Imports { get; set; }
            public RuleDto[] Rules { get; set; }
            public CombinationRuleDto[] CombinationRules { get; set; }
            public AssignmentDto[] ExternalAssignments { get; set; }
            public AliasDto[] Aliases { get; set; }
        }

        private sealed class ImportDto
        {
            public string Id { get; set; }
            public string Mode { get; set; }
        }

        private sealed class RuleDto
        {
            public string RuleId { get; set; }
            public string Name { get; set; }
            public string SourceRole { get; set; }
            public string TargetRole { get; set; }
            public string Decision { get; set; }
            public string Scope { get; set; }
            public string Severity { get; set; }
            public int Priority { get; set; }
            public string Reason { get; set; }
        }

        private sealed class CombinationRuleDto
        {
            public string Name { get; set; }
            public string LeftRole { get; set; }
            public string RightRole { get; set; }
            public string Kind { get; set; }
            public string Reason { get; set; }
        }

        private sealed class AssignmentDto
        {
            public SelectorDto Selector { get; set; }
            public string RoleId { get; set; }
            public string Mode { get; set; }
            public string Source { get; set; }
            public PrecedenceDto Precedence { get; set; }
            public string Behavior { get; set; }
            public string Reason { get; set; }
        }

        private sealed class AliasDto
        {
            public string Name { get; set; }
            public SelectorDto Selector { get; set; }
            public string CanonicalRoleId { get; set; }
            public PrecedenceDto Precedence { get; set; }
            public string Behavior { get; set; }
            public string Reason { get; set; }
        }

        private sealed class SelectorDto
        {
            public string Kind { get; set; }
            public string Pattern { get; set; }
            public string AssemblyName { get; set; }
        }

        private sealed class PrecedenceDto
        {
            public string Specificity { get; set; }
            public string Authority { get; set; }
        }
    }
}
