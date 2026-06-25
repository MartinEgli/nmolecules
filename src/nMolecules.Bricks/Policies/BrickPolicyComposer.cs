using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents policy composer data used by policy documents, aliases, imports, composition, and
/// policy-driven role assignment.
/// </summary>
public static class BrickPolicyComposer
    {
        public static readonly RuleId MissingPolicyRuleId = RuleId.From("XMoleculesBricks0202");
        public static readonly RuleId CircularImportRuleId = RuleId.From("XMoleculesBricks0203");
        public static readonly RuleId WeakerNarrowingRuleId = RuleId.From("XMoleculesBricks0204");

        public static BrickPolicyCompositionResult Compose(
            BrickPolicy root,
            IEnumerable<BrickPolicy> catalog)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            var catalogById = (catalog ?? Enumerable.Empty<BrickPolicy>())
                .Where(candidate => candidate != null)
                .GroupBy(candidate => candidate.Id)
                .ToDictionary(group => group.Key, group => group.First());
            var accumulator = new Accumulator();

            AppendPolicy(root, BrickPolicyImportMode.Import, catalogById, new HashSet<BrickPolicyId>(), accumulator);

            var composedPolicy = new BrickPolicy(
                root.Id,
                root.Name,
                Enumerable.Empty<BrickPolicyImport>(),
                accumulator.Rules,
                accumulator.CombinationRules,
                accumulator.ExternalAssignments,
                accumulator.Aliases,
                root.DefaultDecision,
                root.Enforcement);

            return new BrickPolicyCompositionResult(composedPolicy, accumulator.Steps, accumulator.Issues);
        }

        private static void AppendPolicy(
            BrickPolicy policy,
            BrickPolicyImportMode mode,
            IReadOnlyDictionary<BrickPolicyId, BrickPolicy> catalog,
            ISet<BrickPolicyId> activePolicies,
            Accumulator accumulator)
        {
            if (!activePolicies.Add(policy.Id))
            {
                accumulator.Issues.Add(new BrickPolicyDocumentIssue(
                    CircularImportRuleId,
                    BrickSeverity.Error,
                    $"Policy import cycle detected at '{policy.Id}'."));
                accumulator.Steps.Add(new BrickPolicyCompositionStep(policy.Id, mode, false));
                return;
            }

            foreach (var import in policy.Imports)
            {
                if (import.Mode == BrickPolicyImportMode.Disable)
                {
                    accumulator.Steps.Add(new BrickPolicyCompositionStep(import.ImportedPolicyId, import.Mode, false));
                    continue;
                }

                if (!catalog.TryGetValue(import.ImportedPolicyId, out var importedPolicy))
                {
                    accumulator.Issues.Add(new BrickPolicyDocumentIssue(
                        MissingPolicyRuleId,
                        BrickSeverity.Error,
                        $"Imported policy '{import.ImportedPolicyId}' was not found."));
                    accumulator.Steps.Add(new BrickPolicyCompositionStep(import.ImportedPolicyId, import.Mode, false));
                    continue;
                }

                AppendPolicy(importedPolicy, import.Mode, catalog, activePolicies, accumulator);
            }

            ApplyRules(policy, LocalRuleMode(policy.Imports), accumulator);
            accumulator.CombinationRules.AddRange(policy.CombinationRules);
            accumulator.ExternalAssignments.AddRange(policy.ExternalAssignments);
            accumulator.Aliases.AddRange(policy.Aliases);
            accumulator.Steps.Add(new BrickPolicyCompositionStep(policy.Id, mode, true));
            activePolicies.Remove(policy.Id);
        }

        private static BrickPolicyImportMode LocalRuleMode(IEnumerable<BrickPolicyImport> imports)
        {
            var modes = imports.Select(import => import.Mode).ToArray();
            if (modes.Contains(BrickPolicyImportMode.Override))
            {
                return BrickPolicyImportMode.Override;
            }

            if (modes.Contains(BrickPolicyImportMode.Narrow))
            {
                return BrickPolicyImportMode.Narrow;
            }

            return BrickPolicyImportMode.Extend;
        }

        private static void ApplyRules(
            BrickPolicy policy,
            BrickPolicyImportMode mode,
            Accumulator accumulator)
        {
            foreach (var rule in policy.Rules)
            {
                switch (mode)
                {
                    case BrickPolicyImportMode.Override:
                        ReplaceOrAdd(rule, accumulator.Rules);
                        break;
                    case BrickPolicyImportMode.Narrow:
                        NarrowOrKeep(rule, accumulator);
                        break;
                    default:
                        accumulator.Rules.Add(rule);
                        break;
                }
            }
        }

        private static void ReplaceOrAdd(BrickRule rule, IList<BrickRule> rules)
        {
            var existingIndex = FindRuleIndex(rule.RuleId, rules);
            if (existingIndex < 0)
            {
                rules.Add(rule);
                return;
            }

            rules[existingIndex] = rule;
        }

        private static void NarrowOrKeep(BrickRule rule, Accumulator accumulator)
        {
            var existingIndex = FindRuleIndex(rule.RuleId, accumulator.Rules);
            if (existingIndex < 0)
            {
                accumulator.Rules.Add(rule);
                return;
            }

            var existing = accumulator.Rules[existingIndex];
            if (IsStricterOrEqual(rule, existing))
            {
                accumulator.Rules[existingIndex] = rule;
                return;
            }

            accumulator.Issues.Add(new BrickPolicyDocumentIssue(
                WeakerNarrowingRuleId,
                BrickSeverity.Warning,
                $"Narrowed rule '{rule.RuleId}' is weaker than the imported rule and was ignored."));
        }

        private static int FindRuleIndex(RuleId ruleId, IList<BrickRule> rules)
        {
            for (var i = 0; i < rules.Count; i++)
            {
                if (rules[i].RuleId == ruleId)
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool IsStricterOrEqual(BrickRule candidate, BrickRule existing) =>
            DecisionRank(candidate.Decision) >= DecisionRank(existing.Decision)
            & candidate.Severity >= existing.Severity;

        private static int DecisionRank(BrickDecision decision)
        {
            switch (decision)
            {
                case BrickDecision.Deny:
                    return 3;
                case BrickDecision.Require:
                    return 2;
                default:
                    return 1;
            }
        }

        private sealed class Accumulator
        {
            public List<BrickRule> Rules { get; } = new List<BrickRule>();
            public List<BrickRoleCombinationRule> CombinationRules { get; } = new List<BrickRoleCombinationRule>();
            public List<BrickRoleAssignment> ExternalAssignments { get; } = new List<BrickRoleAssignment>();
            public List<BrickAlias> Aliases { get; } = new List<BrickAlias>();
            public List<BrickPolicyCompositionStep> Steps { get; } = new List<BrickPolicyCompositionStep>();
            public List<BrickPolicyDocumentIssue> Issues { get; } = new List<BrickPolicyDocumentIssue>();
        }
    }
}
