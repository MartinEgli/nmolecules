using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    public static class BrickMemberCardinalityEvaluator
    {
        public static IReadOnlyList<BrickViolation> Evaluate(
            BrickElement element,
            IEnumerable<Attribute> contracts,
            IReadOnlyDictionary<Type, int> memberCounts)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var counts = memberCounts ?? new Dictionary<Type, int>();
            var violations = new List<BrickViolation>();
            foreach (var contract in contracts ?? Enumerable.Empty<Attribute>())
            {
                EvaluateContract(element, contract, counts, violations);
            }

            return violations;
        }

        private static void EvaluateContract(
            BrickElement element,
            Attribute contract,
            IReadOnlyDictionary<Type, int> memberCounts,
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
            }
        }

        private static int GetCount(IReadOnlyDictionary<Type, int> memberCounts, Type memberAttributeType)
        {
            int count;
            return memberCounts.TryGetValue(memberAttributeType, out count) ? count : 0;
        }

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
