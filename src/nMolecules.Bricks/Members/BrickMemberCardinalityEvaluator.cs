using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Evaluates member cardinality evaluator rules against Bricks model data and produces deterministic
/// assessment results.
/// </summary>
public static class BrickMemberCardinalityEvaluator
    {
        public static IReadOnlyList<BrickViolation> Evaluate(
            BrickElement element,
            IEnumerable<Attribute> contracts,
            IReadOnlyDictionary<Type, int> memberCounts)
        {
            return Evaluate(
                element,
                contracts,
                memberCounts,
                null);
        }

        public static IReadOnlyList<BrickViolation> Evaluate(
            BrickElement element,
            IEnumerable<Attribute> contracts,
            IReadOnlyDictionary<Type, int> memberCounts,
            IReadOnlyDictionary<Type, IReadOnlyDictionary<string, int>> namedMemberCounts)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var counts = memberCounts ?? new Dictionary<Type, int>();
            var namedCounts = namedMemberCounts ?? new Dictionary<Type, IReadOnlyDictionary<string, int>>();
            var violations = new List<BrickViolation>();
            foreach (var contract in contracts ?? Enumerable.Empty<Attribute>())
            {
                EvaluateContract(element, contract, counts, namedCounts, violations);
            }

            return violations;
        }

        private static void EvaluateContract(
            BrickElement element,
            Attribute contract,
            IReadOnlyDictionary<Type, int> memberCounts,
            IReadOnlyDictionary<Type, IReadOnlyDictionary<string, int>> namedMemberCounts,
            ICollection<BrickViolation> violations)
        {
            var exactlyOne = contract as RequireExactlyOneMemberAttribute;
            if (exactlyOne != null)
            {
                AddViolationWhen(
                    element,
                    violations,
                    GetCount(memberCounts, exactlyOne.MemberAttributeType) != 1,
                    "RequireExactlyOneMember",
                    $"{element.DisplayName} must declare exactly one member marked with {exactlyOne.MemberAttributeType.Name}.");
                return;
            }

            var allMembers = contract as RequireAllMembersAttribute;
            if (allMembers != null)
            {
                foreach (var markerType in allMembers.MemberAttributeTypes)
                {
                    AddViolationWhen(
                        element,
                        violations,
                        GetCount(memberCounts, markerType) == 0,
                        "RequireAllMembers",
                        $"{element.DisplayName} must declare at least one member marked with {markerType.Name}.");
                }

                return;
            }

            var memberCount = contract as RequireMemberCountAttribute;
            if (memberCount != null)
            {
                var actualCount = GetCount(memberCounts, memberCount.MemberAttributeType);
                AddViolationWhen(
                    element,
                    violations,
                    actualCount != memberCount.Count,
                    "RequireMemberCount",
                    $"{element.DisplayName} must declare {memberCount.MemberAttributeType.Name} count expected {memberCount.Count}, actual {actualCount}.");
                return;
            }

            var memberRange = contract as RequireMemberRangeAttribute;
            if (memberRange != null)
            {
                var actualCount = GetCount(memberCounts, memberRange.MemberAttributeType);
                AddViolationWhen(
                    element,
                    violations,
                    actualCount < memberRange.MinimumCount || actualCount > memberRange.MaximumCount,
                    "RequireMemberRange",
                    $"{element.DisplayName} must declare {memberRange.MemberAttributeType.Name} count between {memberRange.MinimumCount} and {memberRange.MaximumCount}, actual {actualCount}.");
                return;
            }

            var exclusiveChoice = contract as RequireExclusiveChoiceAttribute;
            if (exclusiveChoice != null)
            {
                var hasLeft = GetCount(memberCounts, exclusiveChoice.LeftMemberAttributeType) > 0;
                var hasRight = GetCount(memberCounts, exclusiveChoice.RightMemberAttributeType) > 0;
                AddViolationWhen(
                    element,
                    violations,
                    hasLeft == hasRight,
                    "RequireExclusiveChoice",
                    $"{element.DisplayName} must declare exactly one of {exclusiveChoice.LeftMemberAttributeType.Name} or {exclusiveChoice.RightMemberAttributeType.Name}.");
                return;
            }

            var forbidMember = contract as ForbidMemberAttribute;
            if (forbidMember != null)
            {
                var actualCount = GetCount(memberCounts, forbidMember.MemberAttributeType);
                AddViolationWhen(
                    element,
                    violations,
                    actualCount > 0,
                    "ForbidMember",
                    $"{element.DisplayName} must not declare members marked with {forbidMember.MemberAttributeType.Name}; actual {actualCount}.");
                return;
            }

            var namedMembers = contract as RequireNamedMembersAttribute;
            if (namedMembers != null)
            {
                foreach (var requiredName in namedMembers.RequiredNames.Distinct(StringComparer.Ordinal))
                {
                    AddViolationWhen(
                        element,
                        violations,
                        GetNamedCount(namedMemberCounts, namedMembers.MemberAttributeType, requiredName) == 0,
                        "RequireNamedMembers",
                        $"{element.DisplayName} must declare a {namedMembers.MemberAttributeType.Name} member named {FormatMarkerName(requiredName)}.");
                }

                return;
            }

            var uniqueNamedMember = contract as RequireUniqueNamedMemberAttribute;
            if (uniqueNamedMember != null)
            {
                foreach (var duplicate in GetDuplicateNamedCounts(namedMemberCounts, uniqueNamedMember.MemberAttributeType))
                {
                    AddViolationWhen(
                        element,
                        violations,
                        duplicate.Value > 1,
                        "RequireUniqueNamedMember",
                        $"{element.DisplayName} must declare at most one {uniqueNamedMember.MemberAttributeType.Name} member named {FormatMarkerName(duplicate.Key)}; actual {duplicate.Value}.");
                }
            }
        }

        private static int GetCount(IReadOnlyDictionary<Type, int> memberCounts, Type memberAttributeType)
        {
            int count;
            return memberCounts.TryGetValue(memberAttributeType, out count) ? count : 0;
        }

        private static IEnumerable<KeyValuePair<string, int>> GetDuplicateNamedCounts(
            IReadOnlyDictionary<Type, IReadOnlyDictionary<string, int>> namedMemberCounts,
            Type memberAttributeType)
        {
            IReadOnlyDictionary<string, int> counts;
            if (!namedMemberCounts.TryGetValue(memberAttributeType, out counts))
            {
                return Enumerable.Empty<KeyValuePair<string, int>>();
            }

            return counts.Where(count => count.Value > 1);
        }

        private static int GetNamedCount(
            IReadOnlyDictionary<Type, IReadOnlyDictionary<string, int>> namedMemberCounts,
            Type memberAttributeType,
            string requiredName)
        {
            IReadOnlyDictionary<string, int> counts;
            if (!namedMemberCounts.TryGetValue(memberAttributeType, out counts))
            {
                return 0;
            }

            int count;
            return counts.TryGetValue(requiredName, out count) ? count : 0;
        }

        private static string FormatMarkerName(string name) =>
            string.IsNullOrWhiteSpace(name) ? "<unnamed>" : $"'{name}'";

        private static void AddViolationWhen(
            BrickElement element,
            ICollection<BrickViolation> violations,
            bool condition,
            string ruleName,
            string message)
        {
            if (!condition)
            {
                return;
            }

            violations.Add(new BrickViolation(
                BrickViolationKind.MemberCardinality,
                element,
                message,
                BrickSeverity.Error,
                BrickViolationState.Active,
                ruleName: ruleName));
        }
    }
}
