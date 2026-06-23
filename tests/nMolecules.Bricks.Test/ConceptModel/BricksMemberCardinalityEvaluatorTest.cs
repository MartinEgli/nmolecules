using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksMemberCardinalityEvaluatorTest
    {
        [Fact]
        public void EvaluateExactlyOneMemberEmitsViolationWhenCountIsNotOne()
        {
            var element = Element();
            var contract = new RequireExactlyOneMemberAttribute(typeof(IdentityMemberAttribute));

            var missing = BrickMemberCardinalityEvaluator.Evaluate(element, new Attribute[] { contract }, Counts()).Single();
            var duplicate = BrickMemberCardinalityEvaluator.Evaluate(element, new Attribute[] { contract }, Counts(typeof(IdentityMemberAttribute), 2)).Single();
            var satisfied = BrickMemberCardinalityEvaluator.Evaluate(element, new Attribute[] { contract }, Counts(typeof(IdentityMemberAttribute), 1));

            Assert.Equal(BrickViolationKind.MemberCardinality, missing.Kind);
            Assert.Equal("RequireExactlyOneMember", missing.RuleName);
            Assert.Equal(element, missing.Source);
            Assert.Contains(nameof(IdentityMemberAttribute), missing.Message);
            Assert.Equal(BrickViolationState.Active, missing.State);
            Assert.Equal(BrickSeverity.Error, missing.Severity);
            Assert.Equal(BrickViolationKind.MemberCardinality, duplicate.Kind);
            Assert.Empty(satisfied);
        }

        [Fact]
        public void EvaluateAllMembersEmitsOneViolationPerMissingMarker()
        {
            var element = Element();
            var contract = new RequireAllMembersAttribute(typeof(IdentityMemberAttribute), typeof(VersionMemberAttribute));

            var violations = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(IdentityMemberAttribute), 1)).ToArray();

            Assert.Single(violations);
            Assert.Equal("RequireAllMembers", violations[0].RuleName);
            Assert.Contains(nameof(VersionMemberAttribute), violations[0].Message);
        }

        [Fact]
        public void EvaluateMemberCountEmitsViolationWhenCountDiffers()
        {
            var element = Element();
            var contract = new RequireMemberCountAttribute(typeof(VersionMemberAttribute), 0);

            var violations = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(VersionMemberAttribute), 1)).ToArray();

            Assert.Single(violations);
            Assert.Equal("RequireMemberCount", violations[0].RuleName);
            Assert.Contains("expected 0", violations[0].Message);
        }

        [Fact]
        public void EvaluateExclusiveChoiceRequiresExactlyOneSide()
        {
            var element = Element();
            var contract = new RequireExclusiveChoiceAttribute(typeof(IdentityMemberAttribute), typeof(VersionMemberAttribute));

            var none = BrickMemberCardinalityEvaluator.Evaluate(element, new Attribute[] { contract }, Counts()).Single();
            var both = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(IdentityMemberAttribute), 1, typeof(VersionMemberAttribute), 1)).Single();
            var leftOnly = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(IdentityMemberAttribute), 1));

            Assert.Equal("RequireExclusiveChoice", none.RuleName);
            Assert.Equal("RequireExclusiveChoice", both.RuleName);
            Assert.Empty(leftOnly);
        }

        [Fact]
        public void EvaluateNormalizesNullInputsAndIgnoresUnknownContracts()
        {
            var element = Element();

            Assert.Empty(BrickMemberCardinalityEvaluator.Evaluate(element, null, null));
            Assert.Empty(BrickMemberCardinalityEvaluator.Evaluate(element, new Attribute[] { new UnknownContractAttribute() }, null));
        }

        [Fact]
        public void EvaluateRequiresElement()
        {
            Assert.Throws<ArgumentNullException>(() => BrickMemberCardinalityEvaluator.Evaluate(null, null, null));
        }

        private static BrickElement Element() =>
            new BrickElement(BrickElementId.From("type:Order"), BrickElementKind.Type, "Order");

        private static IReadOnlyDictionary<Type, int> Counts(params object[] pairs)
        {
            var counts = new Dictionary<Type, int>();
            for (var index = 0; index < pairs.Length; index += 2)
            {
                counts.Add((Type)pairs[index], (int)pairs[index + 1]);
            }

            return counts;
        }

        private sealed class IdentityMemberAttribute : Attribute
        {
        }

        private sealed class VersionMemberAttribute : Attribute
        {
        }

        private sealed class UnknownContractAttribute : Attribute
        {
        }
    }
}
