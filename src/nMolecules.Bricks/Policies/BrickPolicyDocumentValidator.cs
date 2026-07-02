using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Validates policy document validator input and returns structured issues that consumers can report or fix.
/// </summary>
public static class BrickPolicyDocumentValidator
    {
        /// <summary>
        /// Identifies the issue emitted when validation is requested without a policy document.
        /// </summary>
        public static readonly RuleId MissingDocumentRuleId = RuleId.From("XMoleculesBricks0200");

        /// <summary>
        /// Identifies the issue emitted when a policy document uses an unsupported schema version.
        /// </summary>
        public static readonly RuleId UnsupportedSchemaRuleId = RuleId.From("XMoleculesBricks0201");

        /// <summary>
        /// Identifies the issue emitted when a policy declares the same rule id more than once.
        /// </summary>
        public static readonly RuleId DuplicateRuleId = RuleId.From("XMoleculesBricks0205");

        /// <summary>
        /// Identifies the issue emitted when a policy repeats the same rule endpoint, decision, and priority.
        /// </summary>
        public static readonly RuleId DuplicateRuleSignatureRuleId = RuleId.From("XMoleculesBricks0206");

        /// <summary>
        /// Identifies the issue emitted when rules for the same endpoint mix conflicting allow and deny decisions.
        /// </summary>
        public static readonly RuleId ConflictingRuleDecisionRuleId = RuleId.From("XMoleculesBricks0207");

        /// <summary>
        /// Identifies the issue emitted when role combination rules assign conflicting combination kinds.
        /// </summary>
        public static readonly RuleId ConflictingRoleCombinationRuleId = RuleId.From("XMoleculesBricks0208");

        /// <summary>
        /// Validates a composed Brick policy document and returns issues that should be shown to framework users.
        /// </summary>
        /// <param name="document">The policy document to validate before it is used by analyzers or tooling.</param>
        /// <returns>A read-only list of validation issues. An empty list means that no policy document issue was found.</returns>
        public static IReadOnlyList<BrickPolicyDocumentIssue> Validate(BrickPolicyDocument document)
        {
            if (document == null)
            {
                return new[]
                {
                    new BrickPolicyDocumentIssue(
                        MissingDocumentRuleId,
                        BrickSeverity.Error,
                        "Policy document is required.")
                };
            }

            if (!document.IsCurrentSchema)
            {
                return new[]
                {
                    new BrickPolicyDocumentIssue(
                        UnsupportedSchemaRuleId,
                        BrickSeverity.Error,
                        $"Policy schema '{document.Schema}' is not supported. Expected '{BrickPolicyDocument.CurrentSchema}'.")
                };
            }

            var issues = new List<BrickPolicyDocumentIssue>();
            ValidateRules(document.Policy, issues);
            ValidateRoleCombinations(document.Policy, issues);
            return issues.ToArray();
        }

        private static void ValidateRules(BrickPolicy policy, ICollection<BrickPolicyDocumentIssue> issues)
        {
            var seenById = new HashSet<RuleId>();
            var seenSignatures = new HashSet<string>(StringComparer.Ordinal);
            var decisionByEndpoint = new Dictionary<string, BrickDecision>(StringComparer.Ordinal);

            foreach (var rule in policy.Rules)
            {
                if (!rule.RuleId.IsEmpty && !seenById.Add(rule.RuleId))
                {
                    issues.Add(new BrickPolicyDocumentIssue(
                        DuplicateRuleId,
                        BrickSeverity.Warning,
                        $"Rule '{rule.RuleId}' is declared more than once."));
                    continue;
                }

                var endpoint = CreateRuleEndpointKey(rule);
                var signature = endpoint + "\u001f" + rule.Decision + "\u001f" + rule.Priority;
                if (!seenSignatures.Add(signature))
                {
                    issues.Add(new BrickPolicyDocumentIssue(
                        DuplicateRuleSignatureRuleId,
                        BrickSeverity.Warning,
                        $"Rule for '{rule.SourceRole}' to '{rule.TargetRole}' with decision '{rule.Decision}' and priority '{rule.Priority}' is declared more than once."));
                    continue;
                }

                if (!decisionByEndpoint.TryGetValue(endpoint, out var previousDecision))
                {
                    decisionByEndpoint.Add(endpoint, rule.Decision);
                    continue;
                }

                if (!AreConflictingDecisions(previousDecision, rule.Decision))
                {
                    continue;
                }

                issues.Add(new BrickPolicyDocumentIssue(
                    ConflictingRuleDecisionRuleId,
                    BrickSeverity.Warning,
                    $"Rule for '{rule.SourceRole}' to '{rule.TargetRole}' is declared with conflicting decisions '{previousDecision}' and '{rule.Decision}'."));
            }
        }

        private static void ValidateRoleCombinations(BrickPolicy policy, ICollection<BrickPolicyDocumentIssue> issues)
        {
            var kindByPair = new Dictionary<string, BrickCombinationKind>(StringComparer.Ordinal);
            foreach (var rule in policy.CombinationRules)
            {
                if (string.IsNullOrWhiteSpace(rule.LeftRoles.Pattern) ||
                    string.IsNullOrWhiteSpace(rule.RightRoles.Pattern))
                {
                    continue;
                }

                var pair = CreateUnorderedPairKey(rule.LeftRoles.Pattern, rule.RightRoles.Pattern);
                if (!kindByPair.TryGetValue(pair, out var previousKind))
                {
                    kindByPair.Add(pair, rule.Kind);
                    continue;
                }

                if (previousKind == rule.Kind)
                {
                    continue;
                }

                issues.Add(new BrickPolicyDocumentIssue(
                    ConflictingRoleCombinationRuleId,
                    BrickSeverity.Warning,
                    $"Role combination for '{rule.LeftRoles.Pattern}' and '{rule.RightRoles.Pattern}' is declared with conflicting kinds '{previousKind}' and '{rule.Kind}'."));
            }
        }

        private static string CreateRuleEndpointKey(BrickRule rule) =>
            rule.Scope + "\u001f" + rule.SourceRole.Value + "\u001f" + rule.TargetRole.Value;

        private static bool AreConflictingDecisions(BrickDecision left, BrickDecision right) =>
            left != right && (left == BrickDecision.Deny || right == BrickDecision.Deny);

        private static string CreateUnorderedPairKey(string left, string right)
        {
            var normalizedLeft = left ?? string.Empty;
            var normalizedRight = right ?? string.Empty;
            return string.CompareOrdinal(normalizedLeft, normalizedRight) <= 0
                ? normalizedLeft + "\u001f" + normalizedRight
                : normalizedRight + "\u001f" + normalizedLeft;
        }
    }
}
